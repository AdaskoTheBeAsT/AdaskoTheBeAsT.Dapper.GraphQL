using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AdaskoTheBeAsT.Dapper.GraphQL.Sqlite.Test;

public class ServiceCollectionExtensionsTests
{
    [Fact(DisplayName = "AddDapperGraphQlSqlite registers SqliteSqlBuilderOptions")]
    public void AddDapperGraphQlSqliteRegistersOptions()
    {
        var services = new ServiceCollection();

        services.AddDapperGraphQlSqlite(_ => { });

        using var provider = services.BuildServiceProvider();
        provider.GetService<SqliteSqlBuilderOptions>().Should().NotBeNull();
    }

    [Fact(DisplayName = "AddDapperGraphQlSqlite returns same service collection")]
    public void AddDapperGraphQlSqliteReturnsSameCollection()
    {
        var services = new ServiceCollection();

        var result = services.AddDapperGraphQlSqlite(_ => { });

        result.Should().BeSameAs(services);
    }

    [Fact(DisplayName = "AddDapperGraphQlSqlite invokes setup callback")]
    public void AddDapperGraphQlSqliteInvokesSetup()
    {
        var services = new ServiceCollection();
        var called = false;

        services.AddDapperGraphQlSqlite(_ => called = true);

        called.Should().BeTrue();
    }
}
