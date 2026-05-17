using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AdaskoTheBeAsT.Dapper.GraphQL.SqlServer.Test;

public class ServiceCollectionExtensionsTests
{
    [Fact(DisplayName = "AddDapperGraphQlSqlServer registers SqlServerSqlBuilderOptions")]
    public void AddDapperGraphQlSqlServerRegistersOptions()
    {
        var services = new ServiceCollection();

        services.AddDapperGraphQlSqlServer(_ => { });

        using var provider = services.BuildServiceProvider();
        provider.GetService<SqlServerSqlBuilderOptions>().Should().NotBeNull();
    }

    [Fact(DisplayName = "AddDapperGraphQlSqlServer returns same service collection")]
    public void AddDapperGraphQlSqlServerReturnsSameCollection()
    {
        var services = new ServiceCollection();

        var result = services.AddDapperGraphQlSqlServer(_ => { });

        result.Should().BeSameAs(services);
    }

    [Fact(DisplayName = "AddDapperGraphQlSqlServer invokes setup callback")]
    public void AddDapperGraphQlSqlServerInvokesSetup()
    {
        var services = new ServiceCollection();
        var called = false;

        services.AddDapperGraphQlSqlServer(_ => called = true);

        called.Should().BeTrue();
    }
}
