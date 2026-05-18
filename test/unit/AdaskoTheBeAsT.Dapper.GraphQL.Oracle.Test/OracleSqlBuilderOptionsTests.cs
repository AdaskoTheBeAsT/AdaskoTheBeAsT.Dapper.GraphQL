using AwesomeAssertions;
using Xunit;

namespace AdaskoTheBeAsT.Dapper.GraphQL.Oracle.Test;

public class OracleSqlBuilderOptionsTests
{
    [Fact(DisplayName = "Oracle options default to ':' parameter prefix")]
    public void DefaultParameterPrefixShouldBeColon()
    {
        var sut = new OracleSqlBuilderOptions();

        sut.ParameterPrefix.Should().Be(":");
    }
}
