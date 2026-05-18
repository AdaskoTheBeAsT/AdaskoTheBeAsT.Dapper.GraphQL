using AdaskoTheBeAsT.Dapper.GraphQL.Contexts;
using AwesomeAssertions;
using Xunit;

namespace AdaskoTheBeAsT.Dapper.GraphQL.Test;

public class SqlQueryContextTests
{
    [Fact(DisplayName = "Select adds column to generated SQL")]
    public void SelectAddsColumnToSql()
    {
        var context = new SqlQueryContext("Person p");

        context.Select("p.Id").Select("p.Name");

        var sql = context.ToString();
        sql.Should().Contain("p.Id");
        sql.Should().Contain("p.Name");
        sql.Should().Contain("FROM Person p");
    }

    [Fact(DisplayName = "Select with array adds all columns to generated SQL")]
    public void SelectWithArrayAddsAllColumns()
    {
        var context = new SqlQueryContext("Person p");

        context.Select(new[] { "p.Id", "p.Name", "p.LastName" });

        var sql = context.ToString();
        sql.Should().Contain("p.Id");
        sql.Should().Contain("p.Name");
        sql.Should().Contain("p.LastName");
    }

    [Fact(DisplayName = "Where adds WHERE clause to generated SQL")]
    public void WhereAddsWhereClause()
    {
        var context = new SqlQueryContext("Person p");

        context.Select("p.Id").Where("p.Id = @id", new { id = 1 });

        var sql = context.ToString();
        sql.Should().Contain("WHERE");
        sql.Should().Contain("p.Id = @id");
    }

    [Fact(DisplayName = "OrWhere adds WHERE clause joined by OR")]
    public void OrWhereAddsWhereJoinedByOr()
    {
        var context = new SqlQueryContext("Person p");

        context.Select("p.Id")
            .Where("p.Id = @id", new { id = 1 })
            .OrWhere("p.Status = @status", new { status = "Active" });

        var sql = context.ToString();
        sql.Should().Contain("p.Id = @id");
        sql.Should().Contain("p.Status = @status");
    }

    [Fact(DisplayName = "InnerJoin appends INNER JOIN clause")]
    public void InnerJoinAppendsClause()
    {
        var context = new SqlQueryContext("Person p");

        context.Select("p.Id").InnerJoin("Email e ON e.PersonId = p.Id");

        var sql = context.ToString();
        sql.Should().Contain("INNER JOIN Email e ON e.PersonId = p.Id");
    }

    [Fact(DisplayName = "LeftJoin appends LEFT OUTER JOIN clause")]
    public void LeftJoinAppendsClause()
    {
        var context = new SqlQueryContext("Person p");

        context.Select("p.Id").LeftJoin("Email e ON e.PersonId = p.Id");

        var sql = context.ToString();
        sql.Should().Contain("LEFT JOIN Email e ON e.PersonId = p.Id");
    }

    [Fact(DisplayName = "OrderBy appends ORDER BY clause")]
    public void OrderByAppendsClause()
    {
        var context = new SqlQueryContext("Person p");

        context.Select("p.Id").OrderBy("p.Name");

        var sql = context.ToString();
        sql.Should().Contain("ORDER BY p.Name");
    }

    [Fact(DisplayName = "Offset appends OFFSET clause")]
    public void OffsetAppendsClause()
    {
        var context = new SqlQueryContext("Person p");

        context.Select("p.Id").OrderBy("p.Id").Offset(20);

        var sql = context.ToString();
        sql.Should().Contain("OFFSET 20");
    }

    [Fact(DisplayName = "Fetch is chainable and returns same context")]
    public void FetchIsChainable()
    {
        var context = new SqlQueryContext("Person p");

        var result = context.Select("p.Id").OrderBy("p.Id").Offset(0).Fetch(10);

        result.Should().BeSameAs(context);
    }

    [Fact(DisplayName = "SplitOn<T> adds entity type and column for multi-mapping")]
    public void SplitOnGenericAddsTypeAndColumn()
    {
        var context = new SqlQueryContext("Person p");

        var result = context.Select("p.Id").SplitOn<PersonEntity>("Id");

        result.Should().BeSameAs(context);
        context.GetSplitOnTypes().Should().Contain(typeof(PersonEntity));
    }

    [Fact(DisplayName = "SplitOn with Type adds entity type and column for multi-mapping")]
    public void SplitOnTypeAddsTypeAndColumn()
    {
        var context = new SqlQueryContext("Person p");

        var result = context.Select("p.Id").SplitOn("Id", typeof(PersonEntity));

        result.Should().BeSameAs(context);
        context.GetSplitOnTypes().Should().Contain(typeof(PersonEntity));
    }

    [Fact(DisplayName = "InnerJoin clears single-table type list when no SplitOn was used")]
    public void InnerJoinClearsSingleTableTypes()
    {
        var context = new SqlQueryContext("Person p");
        context.Select("p.Id");

        context.InnerJoin("Email e ON e.PersonId = p.Id");

        context.GetSplitOnTypes().Should().BeEmpty();
    }

    public class PersonEntity
    {
        public int Id { get; set; }
    }
}
