using AwesomeAssertions;
using Xunit;

namespace AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.Test;

public class PostgreSqlSqlBuilderOptionsTests
{
    [Fact(DisplayName = "PostgreSql options default to '@' parameter prefix")]
    public void DefaultParameterPrefixShouldBeAtSymbol()
    {
        var sut = new PostgreSqlSqlBuilderOptions();

        sut.ParameterPrefix.Should().Be("@");
    }
}
