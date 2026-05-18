using AwesomeAssertions;
using Xunit;

namespace AdaskoTheBeAsT.Dapper.GraphQL.MySql.Test;

public class MySqlSqlBuilderOptionsTests
{
    [Fact(DisplayName = "MySql options default to '@' parameter prefix")]
    public void DefaultParameterPrefixShouldBeAtSymbol()
    {
        var sut = new MySqlSqlBuilderOptions();

        sut.ParameterPrefix.Should().Be("@");
    }
}
