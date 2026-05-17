using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.Test;

public class ServiceCollectionExtensionsTests
{
    [Fact(DisplayName = "AddDapperGraphQlPostgreSql registers PostgreSqlSqlBuilderOptions")]
    public void AddDapperGraphQlPostgreSqlRegistersOptions()
    {
        var services = new ServiceCollection();

        services.AddDapperGraphQlPostgreSql(_ => { });

        using var provider = services.BuildServiceProvider();
        provider.GetService<PostgreSqlSqlBuilderOptions>().Should().NotBeNull();
    }

    [Fact(DisplayName = "AddDapperGraphQlPostgreSql returns same service collection")]
    public void AddDapperGraphQlPostgreSqlReturnsSameCollection()
    {
        var services = new ServiceCollection();

        var result = services.AddDapperGraphQlPostgreSql(_ => { });

        result.Should().BeSameAs(services);
    }

    [Fact(DisplayName = "AddDapperGraphQlPostgreSql invokes setup callback")]
    public void AddDapperGraphQlPostgreSqlInvokesSetup()
    {
        var services = new ServiceCollection();
        var called = false;

        services.AddDapperGraphQlPostgreSql(_ => called = true);

        called.Should().BeTrue();
    }
}
