using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AdaskoTheBeAsT.Dapper.GraphQL.Oracle.Test;

public class ServiceCollectionExtensionsTests
{
    [Fact(DisplayName = "AddDapperGraphQlOracle registers OracleSqlBuilderOptions")]
    public void AddDapperGraphQlOracleRegistersOptions()
    {
        var services = new ServiceCollection();

        services.AddDapperGraphQlOracle(_ => { });

        using var provider = services.BuildServiceProvider();
        provider.GetService<OracleSqlBuilderOptions>().Should().NotBeNull();
    }

    [Fact(DisplayName = "AddDapperGraphQlOracle returns same service collection")]
    public void AddDapperGraphQlOracleReturnsSameCollection()
    {
        var services = new ServiceCollection();

        var result = services.AddDapperGraphQlOracle(_ => { });

        result.Should().BeSameAs(services);
    }

    [Fact(DisplayName = "AddDapperGraphQlOracle invokes setup callback")]
    public void AddDapperGraphQlOracleInvokesSetup()
    {
        var services = new ServiceCollection();
        var called = false;

        services.AddDapperGraphQlOracle(_ => called = true);

        called.Should().BeTrue();
    }
}
