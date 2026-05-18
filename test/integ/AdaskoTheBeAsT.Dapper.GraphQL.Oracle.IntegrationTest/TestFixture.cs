using System;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AdaskoTheBeAsT.Dapper.GraphQL.Oracle;
using AdaskoTheBeAsT.Dapper.GraphQL.Oracle.IntegrationTest.GraphQL;
using AdaskoTheBeAsT.Dapper.GraphQL.Oracle.IntegrationTest.Models;
using AdaskoTheBeAsT.Dapper.GraphQL.Oracle.IntegrationTest.QueryBuilders;
using AdaskoTheBeAsT.Dapper.GraphQL.Oracle.IntegrationTest.Repositories;
using DbUp;
using DbUp.Oracle;
using GraphQL;
using GraphQL.Execution;
using GraphQL.NewtonsoftJson;
using GraphQL.Types.Relay;
using GraphQLParser.AST;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Testcontainers.Oracle;
using Xunit;
using PhoneType = AdaskoTheBeAsT.Dapper.GraphQL.Oracle.IntegrationTest.GraphQL.PhoneType;

namespace AdaskoTheBeAsT.Dapper.GraphQL.Oracle.IntegrationTest;

public sealed class TestFixture
    : IAsyncLifetime,
        IDisposable
{
    private DocumentExecuter? _documentExecuter;

    private OracleContainer? _oracleContainer;

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
    public async ValueTask InitializeAsync()
#else
        public async Task InitializeAsync()
#endif
    {
        _oracleContainer
            = new OracleBuilder("gvenzl/oracle-free:23-slim-faststart")
                .Build();

#if NET8_0_OR_GREATER
        await _oracleContainer!.StartAsync(Xunit.TestContext.Current.CancellationToken);
#else
        await _oracleContainer!.StartAsync();
#endif

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
    }

#if NET8_0_OR_GREATER
    public async ValueTask DisposeAsync()
#else
        public async Task DisposeAsync()
#endif
    {
        if (!IsDisposing)
        {
            IsDisposing = true;
            if (_oracleContainer != null)
            {
                await _oracleContainer.DisposeAsync().AsTask();
            }

            (ServiceProvider as IDisposable)?.Dispose();
        }
    }

    public void Dispose()
    {
        if (!IsDisposing)
        {
            IsDisposing = true;
#pragma warning disable VSTHRD002
            _oracleContainer?.DisposeAsync().AsTask().GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            (ServiceProvider as IDisposable)?.Dispose();
        }
    }

#pragma warning disable IDISP001, CA2000
    public IDbConnection GetDbConnection()
    {
        var connection = new BindByNameOracleConnection(ConnectionString!);
        var options = ServiceProvider?.GetService<OracleSqlBuilderOptions>() ?? new OracleSqlBuilderOptions();
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
        ConnectionString = _oracleContainer?.GetConnectionString();

        var upgrader = DeployChanges.To
            .OracleDatabaseWithDefaultDelimiter(ConnectionString)
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
        serviceCollection.AddDapperGraphQlOracle(options =>
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
