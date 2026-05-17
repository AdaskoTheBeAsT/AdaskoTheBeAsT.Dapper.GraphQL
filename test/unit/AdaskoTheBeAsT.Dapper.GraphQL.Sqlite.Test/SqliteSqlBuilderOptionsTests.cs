using AdaskoTheBeAsT.Dapper.GraphQL.Sqlite;
using AwesomeAssertions;
using Xunit;

namespace AdaskoTheBeAsT.Dapper.GraphQL.Sqlite.Test;

public class SqliteSqlBuilderOptionsTests
{
    [Fact(DisplayName = "Sqlite options default to '@' parameter prefix")]
    public void DefaultParameterPrefixShouldBeAtSymbol()
    {
        var sut = new SqliteSqlBuilderOptions();

        sut.ParameterPrefix.Should().Be("@");
    }
}
