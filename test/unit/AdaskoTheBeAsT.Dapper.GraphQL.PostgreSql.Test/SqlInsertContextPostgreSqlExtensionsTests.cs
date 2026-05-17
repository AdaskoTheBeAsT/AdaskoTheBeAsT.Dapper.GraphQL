using System;
using System.Data;
using System.Threading.Tasks;
using AdaskoTheBeAsT.Dapper.GraphQL.Contexts;
using AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.Extensions;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.Test;

public class SqlInsertContextPostgreSqlExtensionsTests
{
    [Fact(DisplayName = "ExecuteWithPostgreSqlIdentity throws for non-member expression")]
    public void ExecuteWithPostgreSqlIdentityThrowsForNonMemberExpression()
    {
        var context = new SqlInsertContext<Entity>(nameof(Entity), new Entity());
        var connection = new Mock<IDbConnection>(MockBehavior.Loose).Object;

        var act = () => context.ExecuteWithPostgreSqlIdentity<Entity, int>(connection, e => e.Id + 1);

        act.Should().Throw<NotSupportedException>();
    }

    [Fact(DisplayName = "ExecuteWithPostgreSqlIdentityAsync throws for non-member expression")]
    public Task ExecuteWithPostgreSqlIdentityAsyncThrowsForNonMemberExpression()
    {
        var context = new SqlInsertContext<Entity>(nameof(Entity), new Entity());
        var connection = new Mock<IDbConnection>(MockBehavior.Loose).Object;

        Func<Task<int>> act = () => context.ExecuteWithPostgreSqlIdentityAsync<Entity, int>(connection, e => e.Id + 1);

        return act.Should().ThrowAsync<NotSupportedException>();
    }

    public class Entity
    {
        public int Id { get; set; }
    }
}
