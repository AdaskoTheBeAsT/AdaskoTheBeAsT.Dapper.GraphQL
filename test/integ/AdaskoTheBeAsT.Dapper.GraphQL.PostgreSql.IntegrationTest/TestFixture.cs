using System;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AdaskoTheBeAsT.Dapper.GraphQL.Extensions;
using AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest.GraphQL;
using AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest.Models;
using AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest.QueryBuilders;
using AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest.Repositories;
using DbUp;
using GraphQL;
using GraphQL.Execution;
using GraphQL.NewtonsoftJson;
using GraphQL.Types.Relay;
using GraphQLParser.AST;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Npgsql;
using PhoneType = AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest.GraphQL.PhoneType;

namespace AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest
{
    public class TestFixture
        : IDisposable
    {
        private static readonly string _chars = "abcdefghijklmnopqrstuvwxyz0123456789";
        private static readonly Random _random = new Random((int)(DateTime.Now.Ticks << 32));

        private readonly string _databaseName;
        private readonly DocumentExecuter _documentExecuter;

        public TestFixture()
        {
            _databaseName = "test-" + new string(_chars.OrderBy(c => _random.Next()).ToArray());

            _documentExecuter = new DocumentExecuter();
            var serviceCollection = new ServiceCollection();

            SetupDatabaseConnection();
            SetupDapperGraphQl(serviceCollection);

            ServiceProvider = serviceCollection.BuildServiceProvider();
            Schema = ServiceProvider.GetRequiredService<PersonSchema>();
        }

        public PersonSchema Schema { get; set; }

        public IServiceProvider ServiceProvider { get; set; }

        private string ConnectionString { get; set; } = null;

        private bool IsDisposing { get; set; } = false;

        public IHasSelectionSetNode? BuildGraphQlSelection(string body)
        {
            var document = new GraphQLDocumentBuilder().Build(body);
            return document
                .Definitions
                .OfType<IHasSelectionSetNode>()
                .First()?
                .SelectionSet?
                .Selections
                .OfType<GraphQLField>()
                .FirstOrDefault();
        }

        public void Dispose()
        {
            if (!IsDisposing)
            {
                IsDisposing = true;
                TeardownDatabase();
            }
        }

        public IDbConnection GetDbConnection()
        {
            var connection = new NpgsqlConnection(ConnectionString);
            return connection;
        }

        public bool JsonEquals(string expectedJson, string actualJson)
        {
            // To ensure formatting doesn't affect our results, we first convert to JSON tokens
            // and only compare the structure of the resulting objects.
            return JToken.DeepEquals(JObject.Parse(expectedJson), JObject.Parse(actualJson));
        }

        public async Task<string> QueryGraphQlAsync(string query)
        {
            var result = await _documentExecuter
                .ExecuteAsync(options =>
                {
                    options.Schema = Schema;
                    options.Query = query;
                })
                .ConfigureAwait(false);

            var json = new GraphQLSerializer(indent: true).Serialize(result);
            return json;
        }

        public async Task<string> QueryGraphQlAsync(GraphQlQuery query)
        {
            var serializer = new GraphQLSerializer();
            var inputs = serializer.ReadNode<Inputs>(query.Variables) ?? Inputs.Empty;
            var result = await _documentExecuter
                .ExecuteAsync(options =>
                {
                    options.Schema = Schema;
                    options.Query = query.Query;
                    options.Variables = inputs;
                })
                .ConfigureAwait(false);

            var json = new GraphQLSerializer(indent: true).Serialize(result);
            return json;
        }

        public void SetupDatabaseConnection()
        {
            // Generate a random db name

            ConnectionString = $"Server=localhost;Port=5432;Database={_databaseName};User Id=postgres;Password=dapper-graphql;";

            // Ensure the database exists
            EnsureDatabase.For.PostgresqlDatabase(ConnectionString);

            var upgrader = DeployChanges.To
                .PostgresqlDatabase(ConnectionString)
                .WithScriptsEmbeddedInAssembly(typeof(Person).GetTypeInfo().Assembly)
                .LogToConsole()
                .Build();

            var upgradeResult = upgrader.PerformUpgrade();
            if (!upgradeResult.Successful)
            {
                throw new InvalidOperationException("The database upgrade did not succeed for unit testing.", upgradeResult.Error);
            }
        }

        public void TeardownDatabase()
        {
            // Connect to a different database, so we can drop the one we were working with
            var dropConnectionString = ConnectionString.Replace(_databaseName, "template1");
            using (var connection = new NpgsqlConnection(dropConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    // NOTE: I'm not sure why there are active connections to the database at
                    // this point, as we're the only ones using this database, and the connection
                    // is closed at this point.  In any case, we need to take an extra step of
                    // dropping all connections to the database before dropping it.
                    //
                    // See https://stackoverflow.com/a/59021507
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
#pragma warning disable SCS0002 // Potential SQL injection vulnerability was found where '{0}' in '{1}' may be tainted by user-controlled data from '{2}' in method '{3}'.
                    command.CommandText = $@"DROP DATABASE ""{_databaseName}"" WITH (FORCE);";
#pragma warning restore SCS0002 // Potential SQL injection vulnerability was found where '{0}' in '{1}' may be tainted by user-controlled data from '{2}' in method '{3}'.
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
                    command.CommandType = CommandType.Text;

                    // Drop the database
                    command.ExecuteNonQuery();
                }
            }
        }

        private void SetupDapperGraphQl(IServiceCollection serviceCollection)
        {
            serviceCollection.AddDapperGraphQl(options =>
            {
                // Add GraphQL types
                options.AddType<CompanyType>();
                options.AddType<EmailType>();
                options.AddType<PersonType>();
                options.AddType<PhoneEnumType>();
                options.AddType<PhoneType>();
                options.AddType<PersonQuery>();
                options.AddType<PersonMutation>();
                options.AddType<PersonInputType>();

                // Add the GraphQL schema
                options.AddSchema<PersonSchema>();

                // Add query builders for dapper
                options.AddQueryBuilder<Company, CompanyQueryBuilder>();
                options.AddQueryBuilder<Email, EmailQueryBuilder>();
                options.AddQueryBuilder<Person, PersonQueryBuilder>();
                options.AddQueryBuilder<Phone, PhoneQueryBuilder>();
            });

            serviceCollection.AddSingleton<IPersonRepository, PersonRepository>();

            // Support for GraphQL paging
            serviceCollection.AddTransient(typeof(ConnectionType<>));
            serviceCollection.AddTransient(typeof(EdgeType<>));
            serviceCollection.AddTransient<PageInfoType>();

            serviceCollection.AddTransient<IDbConnection>(serviceProvider => GetDbConnection());
        }
    }
}
