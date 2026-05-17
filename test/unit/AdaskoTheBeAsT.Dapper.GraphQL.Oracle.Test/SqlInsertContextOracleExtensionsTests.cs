using System;
using System.Data;
using System.Threading.Tasks;
using AdaskoTheBeAsT.Dapper.GraphQL.Contexts;
using AdaskoTheBeAsT.Dapper.GraphQL.Oracle.Extensions;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace AdaskoTheBeAsT.Dapper.GraphQL.Oracle.Test;

public class SqlInsertContextOracleExtensionsTests
{
    [Fact(DisplayName = "ExecuteWithOracleIdentity throws for non-member expression")]
    public void ExecuteWithOracleIdentityThrowsForNonMemberExpression()
    {
        var context = new SqlInsertContext<Entity>(nameof(Entity), new Entity());
        var connection = new Mock<IDbConnection>(MockBehavior.Loose).Object;

        var act = () => context.ExecuteWithOracleIdentity<Entity, int>(connection, e => e.Id + 1);

        act.Should().Throw<NotSupportedException>();
    }

    [Fact(DisplayName = "ExecuteWithOracleIdentityAsync throws for non-member expression")]
    public Task ExecuteWithOracleIdentityAsyncThrowsForNonMemberExpression()
    {
        var context = new SqlInsertContext<Entity>(nameof(Entity), new Entity());
        var connection = new Mock<IDbConnection>(MockBehavior.Loose).Object;

        Func<Task<int>> act = () => context.ExecuteWithOracleIdentityAsync<Entity, int>(connection, e => e.Id + 1);

        return act.Should().ThrowAsync<NotSupportedException>();
    }

    public class Entity
    {
        public int Id { get; set; }
    }
}
