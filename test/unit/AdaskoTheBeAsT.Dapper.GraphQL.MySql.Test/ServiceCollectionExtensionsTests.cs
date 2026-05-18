using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AdaskoTheBeAsT.Dapper.GraphQL.MySql.Test;

public class ServiceCollectionExtensionsTests
{
    [Fact(DisplayName = "AddDapperGraphQlMySql registers MySqlSqlBuilderOptions")]
    public void AddDapperGraphQlMySqlRegistersOptions()
    {
        var services = new ServiceCollection();

        services.AddDapperGraphQlMySql(_ => { });

        using var provider = services.BuildServiceProvider();
        provider.GetService<MySqlSqlBuilderOptions>().Should().NotBeNull();
    }

    [Fact(DisplayName = "AddDapperGraphQlMySql returns same service collection")]
    public void AddDapperGraphQlMySqlReturnsSameCollection()
    {
        var services = new ServiceCollection();

        var result = services.AddDapperGraphQlMySql(_ => { });

        result.Should().BeSameAs(services);
    }

    [Fact(DisplayName = "AddDapperGraphQlMySql invokes setup callback")]
    public void AddDapperGraphQlMySqlInvokesSetup()
    {
        var services = new ServiceCollection();
        var called = false;

        services.AddDapperGraphQlMySql(_ => called = true);

        called.Should().BeTrue();
    }
}
