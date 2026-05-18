using AwesomeAssertions;
using Xunit;

namespace AdaskoTheBeAsT.Dapper.GraphQL.Test;

public class SqlInsertContextGenericTests
{
    [Fact(DisplayName = "Chained Insert returns same context (fluent API)")]
    public void ChainedInsertReturnsSameContext()
    {
        var context = SqlBuilder.Insert(new TestEntity { Id = 1, Name = "First" });

        var result = context.Insert(new TestEntity { Id = 2, Name = "Second" });

        result.Should().BeSameAs(context);
    }

    [Fact(DisplayName = "Insert produces INSERT INTO with all parameter columns")]
    public void InsertProducesInsertIntoStatement()
    {
        var context = SqlBuilder.Insert(new TestEntity { Id = 1, Name = "First" });

        var sql = context.ToString();

        sql.Should().Contain("INSERT INTO TestEntity");
        sql.Should().Contain("Id");
        sql.Should().Contain("Name");
    }

    [Fact(DisplayName = "Generic Insert uses entity type name as table")]
    public void GenericInsertUsesEntityTypeName()
    {
        var context = SqlBuilder.Insert(new TestEntity { Id = 1, Name = "First" });

        context.Table.Should().Be(nameof(TestEntity));
    }

    public class TestEntity
    {
        public int Id { get; set; }

        public string? Name { get; set; }
    }
}
