using AdaskoTheBeAsT.Dapper.GraphQL.Contexts;
using AwesomeAssertions;
using Xunit;

namespace AdaskoTheBeAsT.Dapper.GraphQL.Test;

public class SqlUpdateContextTests
{
    [Fact(DisplayName = "Where passes parameters to SQL builder template")]
    public void WhereShouldIncludeParametersInGeneratedSql()
    {
        var context = new SqlUpdateContext(
            "Person",
            new { FirstName = "Douglas" });

        context.Where("Id = @id", new { id = 42 });

        var sql = context.ToString();

        sql.Should().Contain("WHERE");
        sql.Should().Contain("Id = @id");
    }

    [Fact(DisplayName = "Where with parameters produces same SQL as AndWhere")]
    public void WhereShouldProduceSameSqlAsAndWhere()
    {
        var contextWhere = new SqlUpdateContext(
            "Person",
            new { FirstName = "Douglas" });
        contextWhere.Where("Id = @id", new { id = 42 });

        var contextAndWhere = new SqlUpdateContext(
            "Person",
            new { FirstName = "Douglas" });
        contextAndWhere.AndWhere("Id = @id", new { id = 42 });

        contextWhere.ToString().Should().Be(contextAndWhere.ToString());
    }

    [Fact(DisplayName = "Where should not duplicate SET columns with where parameters")]
    public void WhereShouldNotDuplicateSetColumnsWithWhereParameters()
    {
        var context = new SqlUpdateContext(
            "Person",
            new { FirstName = "Douglas" });

        context.Where("Id = @id", new { id = 42 });

        var sql = context.ToString();

        sql.Should().Contain("SET FirstName = @FirstName");
        sql.Should().NotContain("SET FirstName = @FirstName, id = @id");
    }

    [Fact(DisplayName = "Multiple Where clauses are joined with AND")]
    public void MultipleWhereClausesShouldBeJoinedWithAnd()
    {
        var context = new SqlUpdateContext(
            "Person",
            new { FirstName = "Douglas" });

        context.Where("Id = @id", new { id = 42 });
        context.Where("LastName = @lastName", new { lastName = "Day" });

        var sql = context.ToString();

        sql.Should().Contain("Id = @id");
        sql.Should().Contain("LastName = @lastName");
    }
}
