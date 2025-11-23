using System;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest.GraphQL;
using AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest.Models;
using AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest.QueryBuilders;
using AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest.Repositories;
using AdaskoTheBeAsT.Dapper.GraphQL.ServiceCollection;
using DbUp;
using GraphQL;
using GraphQL.Execution;
using GraphQL.NewtonsoftJson;
using GraphQL.Types.Relay;
using GraphQLParser.AST;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;
using PhoneType = AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest.GraphQL.PhoneType;

namespace AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest
{
    public sealed class TestFixture
        : IAsyncLifetime,
            IDisposable
    {
        private const string Chars = "abcdefghijklmnopqrstuvwxyz0123456789";
        private static readonly Random Random = new((int)(DateTime.Now.Ticks << 32));

        private readonly string _databaseName;
        private DocumentExecuter? _documentExecuter;

        private PostgreSqlContainer? _postgreSqlContainer;

        public TestFixture()
        {
            _databaseName = "test-" + new string([.. Chars.OrderBy(_ => Random.Next())]);
        }

        public PersonSchema? Schema { get; set; }

        public IServiceProvider? ServiceProvider { get; private set; }

        private string? ConnectionString { get; set; }

        private bool IsDisposing { get; set; } = false;

        public async Task InitializeAsync()
        {
            _postgreSqlContainer
                = new PostgreSqlBuilder()
                    .WithImage("postgres:18.1")
                    .WithDatabase(_databaseName)
                    .WithUsername("admin")
                    .WithPassword("TestPass123!")
                    .WithPortBinding(5432, true)
                    .Build();

            await _postgreSqlContainer!.StartAsync();

            _documentExecuter = new DocumentExecuter();
            var serviceCollection = new Microsoft.Extensions.DependencyInjection.ServiceCollection();

            SetupDatabaseConnection();
            SetupDapperGraphQl(serviceCollection);

            (ServiceProvider as IDisposable)?.Dispose();
            ServiceProvider = serviceCollection.BuildServiceProvider();
            Schema = ServiceProvider.GetRequiredService<PersonSchema>();
        }

        public async Task DisposeAsync()
        {
            if (!IsDisposing)
            {
                IsDisposing = true;
                if (_postgreSqlContainer != null)
                {
                    await _postgreSqlContainer.DisposeAsync().AsTask();
                }

                (ServiceProvider as IDisposable)?.Dispose();
            }
        }

#pragma warning disable S2325 // Methods and properties that don't access instance data should be static
        public IHasSelectionSetNode? BuildGraphQlSelection(string body)
        {
            var document = new GraphQLDocumentBuilder().Build(body);
            return document
                .Definitions
                .OfType<IHasSelectionSetNode>()
                .First()
                .SelectionSet?
                .Selections
                .OfType<GraphQLField>()
                .FirstOrDefault();
        }
#pragma warning restore S2325 // Methods and properties that don't access instance data should be static

        public void Dispose()
        {
            if (!IsDisposing)
            {
                IsDisposing = true;
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
                _postgreSqlContainer?.DisposeAsync().GetAwaiter().GetResult();
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
                (ServiceProvider as IDisposable)?.Dispose();
            }
        }

        public IDbConnection GetDbConnection()
        {
            var connection = new NpgsqlConnection(ConnectionString);
            return connection;
        }

#pragma warning disable S2325 // Methods and properties that don't access instance data should be static
        public bool JsonEquals(string expectedJson, string actualJson)
        {
            // To ensure formatting doesn't affect our results, we first convert to JSON tokens
            // and only compare the structure of the resulting objects.
            return JToken.DeepEquals(JObject.Parse(expectedJson), JObject.Parse(actualJson));
        }
#pragma warning restore S2325 // Methods and properties that don't access instance data should be static

        public async Task<string> QueryGraphQlAsync(string query)
        {
            var result = await _documentExecuter!
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
            var result = await _documentExecuter!
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
            ConnectionString = _postgreSqlContainer?.GetConnectionString();

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

            // Support for GraphQL paging - GraphQL.NET v8 requires both ConnectionType<> and ConnectionType<,>
            serviceCollection.AddTransient(typeof(ConnectionType<>));
            serviceCollection.AddTransient(typeof(ConnectionType<,>));
            serviceCollection.AddTransient(typeof(EdgeType<>));
            serviceCollection.AddTransient<PageInfoType>();

            serviceCollection.AddTransient<IDbConnection>(_ => GetDbConnection());
        }
    }
}
