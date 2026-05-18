using AdaskoTheBeAsT.Dapper.GraphQL.Contexts;
using AwesomeAssertions;
using Xunit;

namespace AdaskoTheBeAsT.Dapper.GraphQL.Test;

public class SqlDeleteContextTests
{
    [Fact(DisplayName = "Delete<T> uses entity type name as table")]
    public void DeleteGenericUsesTypeName()
    {
        var context = SqlDeleteContext.Delete<Person>(new { Id = 1 });

        context.Table.Should().Be(nameof(Person));
    }

    [Fact(DisplayName = "ToString builds DELETE FROM with WHERE clause from parameters")]
    public void ToStringBuildsDeleteSql()
    {
        var context = new SqlDeleteContext(nameof(Person), new { Id = 5 });

        var sql = context.ToString();

        sql.Should().Contain("DELETE FROM Person");
        sql.Should().Contain("WHERE");
        sql.Should().Contain("Id = @Id");
    }

    [Fact(DisplayName = "ToString joins multiple parameters with AND")]
    public void ToStringJoinsMultipleParametersWithAnd()
    {
        var context = new SqlDeleteContext(nameof(Person), new { Id = 5, Status = "Active" });

        var sql = context.ToString();

        sql.Should().Contain("Id = @Id");
        sql.Should().Contain("Status = @Status");
        sql.Should().Contain(" AND ");
    }

    [Fact(DisplayName = "Chained Delete returns same context (fluent API)")]
    public void ChainedDeleteReturnsSameContext()
    {
        var context = new SqlDeleteContext(nameof(Person), new { Id = 1 });

        var result = context.Delete("Email", new { Id = 2 });

        result.Should().BeSameAs(context);
    }

    public class Person
    {
        public int Id { get; set; }
    }
}
