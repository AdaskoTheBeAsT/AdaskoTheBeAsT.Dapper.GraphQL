using AwesomeAssertions;
using Xunit;

namespace AdaskoTheBeAsT.Dapper.GraphQL.SqlServer.Test;

public class SqlServerSqlBuilderOptionsTests
{
    [Fact(DisplayName = "SqlServer options default to '@' parameter prefix")]
    public void DefaultParameterPrefixShouldBeAtSymbol()
    {
        var sut = new SqlServerSqlBuilderOptions();

        sut.ParameterPrefix.Should().Be("@");
    }
}
