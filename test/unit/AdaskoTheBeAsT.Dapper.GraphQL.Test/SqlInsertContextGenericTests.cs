using System;
using System.Linq;
using System.Reflection;
using AdaskoTheBeAsT.Dapper.GraphQL.Contexts;
using AwesomeAssertions;
using Xunit;

namespace AdaskoTheBeAsT.Dapper.GraphQL.Test;

public class SqlInsertContextGenericTests
{
    [Fact(DisplayName = "Chained typed Insert stores inserts in parent list")]
    public void ChainedTypedInsertShouldStoreInParentList()
    {
        var context = SqlBuilder.Insert(new TestEntity { Id = 1, Name = "First" });
        context.Insert(new TestEntity { Id = 2, Name = "Second" });
        context.Insert(new TestEntity { Id = 3, Name = "Third" });

        var parentInserts = GetParentInsertsList(context);

        parentInserts.Should().NotBeNull();
        parentInserts!.Count.Should().Be(2);
    }

    [Fact(DisplayName = "Generic SqlInsertContext should not have its own _inserts field")]
    public void GenericSqlInsertContextShouldNotHaveOwnInsertsField()
    {
        var genericType = typeof(SqlInsertContext<TestEntity>);
        var ownFields = genericType.GetFields(
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

        var insertsField = ownFields.FirstOrDefault(
            f => string.Equals(f.Name, "_inserts", StringComparison.Ordinal));

        insertsField.Should().BeNull();
    }

    [Fact(DisplayName = "Chained inserts via base Insert<T> are reflected in ToString")]
    public void ChainedInsertsShouldBeAccessibleForExecution()
    {
        var context = SqlBuilder.Insert(new TestEntity { Id = 1, Name = "First" });
        context.Insert(new TestEntity { Id = 2, Name = "Second" });

        var sql = context.ToString();
        sql.Should().Contain("INSERT INTO TestEntity");
        sql.Should().Contain("Id");
        sql.Should().Contain("Name");
    }

    private static System.Collections.IList? GetParentInsertsList(SqlInsertContext context)
    {
        var field = typeof(SqlInsertContext).GetField(
            "_inserts",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
        return field?.GetValue(context) as System.Collections.IList;
    }

    public class TestEntity
    {
        public int Id { get; set; }

        public string? Name { get; set; }
    }
}
