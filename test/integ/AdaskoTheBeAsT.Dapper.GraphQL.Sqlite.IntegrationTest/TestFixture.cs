using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AdaskoTheBeAsT.Dapper.GraphQL.Sqlite.IntegrationTest.GraphQL;
using AdaskoTheBeAsT.Dapper.GraphQL.Sqlite.IntegrationTest.Models;
using AdaskoTheBeAsT.Dapper.GraphQL.Sqlite.IntegrationTest.QueryBuilders;
using AdaskoTheBeAsT.Dapper.GraphQL.Sqlite.IntegrationTest.Repositories;
using DbUp;
using GraphQL;
using GraphQL.Execution;
using GraphQL.NewtonsoftJson;
using GraphQL.Types.Relay;
using GraphQLParser.AST;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Xunit;
using PhoneType = AdaskoTheBeAsT.Dapper.GraphQL.Sqlite.IntegrationTest.GraphQL.PhoneType;

namespace AdaskoTheBeAsT.Dapper.GraphQL.Sqlite.IntegrationTest;

public sealed class TestFixture
    : IAsyncLifetime,
        IDisposable
{
    private DocumentExecuter? _documentExecuter;

    private string? _databaseFile;

    public PersonSchema? Schema { get; set; }

    public IServiceProvider? ServiceProvider { get; private set; }

    private string? ConnectionString { get; set; }

    private bool IsDisposing { get; set; }

    public static IHasSelectionSetNode? BuildGraphQlSelection(string body)
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

    public static bool JsonEquals(string expectedJson, string actualJson) =>
        JToken.DeepEquals(JObject.Parse(expectedJson), JObject.Parse(actualJson));

#if NET8_0_OR_GREATER
    public ValueTask InitializeAsync()
#else
    public Task InitializeAsync()
#endif
    {
        _databaseFile = Path.Combine(Path.GetTempPath(), $"dapper-graphql-test-{Guid.NewGuid():N}.db");
        ConnectionString = $"Data Source={_databaseFile}";

#if NET6_0_OR_GREATER
        global::Dapper.SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
#endif

        _documentExecuter = new DocumentExecuter();
        var serviceCollection = new ServiceCollection();

        SetupDatabaseConnection();
        SetupDapperGraphQl(serviceCollection);

        (ServiceProvider as IDisposable)?.Dispose();
        ServiceProvider = serviceCollection.BuildServiceProvider();
        Schema = ServiceProvider.GetRequiredService<PersonSchema>();

#if NET8_0_OR_GREATER
        return ValueTask.CompletedTask;
#else
        return Task.CompletedTask;
#endif
    }

#if NET8_0_OR_GREATER
    public ValueTask DisposeAsync()
#else
    public Task DisposeAsync()
#endif
    {
        if (!IsDisposing)
        {
            IsDisposing = true;
            SqliteConnection.ClearAllPools();
            if (!string.IsNullOrEmpty(_databaseFile) && File.Exists(_databaseFile))
            {
                try
                {
#pragma warning disable SCS0018, SEC0116
                    File.Delete(_databaseFile);
#pragma warning restore SCS0018, SEC0116
                }
#pragma warning disable CC0004, S108
                catch (IOException)
                {
                    // Best effort cleanup; sqlite file may still be locked.
                }
#pragma warning restore CC0004, S108
            }

            (ServiceProvider as IDisposable)?.Dispose();
        }

#if NET8_0_OR_GREATER
        return ValueTask.CompletedTask;
#else
        return Task.CompletedTask;
#endif
    }

    public void Dispose()
    {
#pragma warning disable VSTHRD002
#if NET8_0_OR_GREATER
        DisposeAsync().AsTask().GetAwaiter().GetResult();
#else
        DisposeAsync().GetAwaiter().GetResult();
#endif
#pragma warning restore VSTHRD002
    }

#pragma warning disable IDISP001, CA2000
    public IDbConnection GetDbConnection()
    {
        var connection = new SqliteConnection(ConnectionString);
        var options = ServiceProvider?.GetService<SqliteSqlBuilderOptions>() ?? new SqliteSqlBuilderOptions();
        return connection.WithDapperGraphQlOptions(options);
    }
#pragma warning restore IDISP001, CA2000

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
        using (var c = new SqliteConnection(ConnectionString))
        {
            c.Open();
            c.Close();
        }

        var upgrader = DeployChanges.To
            .SqliteDatabase(ConnectionString)
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
        serviceCollection.AddDapperGraphQlSqlite(options =>
        {
            options.AddType<CompanyType>();
            options.AddType<EmailType>();
            options.AddType<PersonType>();
            options.AddType<PhoneEnumType>();
            options.AddType<PhoneType>();
            options.AddType<PersonQuery>();
            options.AddType<PersonMutation>();
            options.AddType<PersonInputType>();

            options.AddSchema<PersonSchema>();

            options.AddQueryBuilder<Company, CompanyQueryBuilder>();
            options.AddQueryBuilder<Email, EmailQueryBuilder>();
            options.AddQueryBuilder<Person, PersonQueryBuilder>();
            options.AddQueryBuilder<Phone, PhoneQueryBuilder>();
        });

        serviceCollection.AddSingleton<IPersonRepository, PersonRepository>();

        serviceCollection.AddTransient(typeof(ConnectionType<>));
        serviceCollection.AddTransient(typeof(ConnectionType<,>));
        serviceCollection.AddTransient(typeof(EdgeType<>));
        serviceCollection.AddTransient<PageInfoType>();

        serviceCollection.AddTransient<IDbConnection>(_ => GetDbConnection());
    }
}
