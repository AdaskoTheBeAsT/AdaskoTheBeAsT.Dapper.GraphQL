using AwesomeAssertions;
using Xunit;

namespace AdaskoTheBeAsT.Dapper.GraphQL.Test;

public class SqlBuilderOptionsTests
{
    [Fact(DisplayName = "Default options use '@' as parameter prefix")]
    public void DefaultShouldUseAtSymbolAsParameterPrefix()
    {
        SqlBuilderOptions.Default.ParameterPrefix.Should().Be("@");
    }

    [Fact(DisplayName = "New options instance uses '@' as default parameter prefix")]
    public void NewInstanceShouldDefaultToAtSymbol()
    {
        var options = new SqlBuilderOptions();

        options.ParameterPrefix.Should().Be("@");
    }

    [Fact(DisplayName = "Parameter prefix can be overridden")]
    public void ParameterPrefixCanBeOverridden()
    {
        var options = new SqlBuilderOptions { ParameterPrefix = ":" };

        options.ParameterPrefix.Should().Be(":");
    }
}
