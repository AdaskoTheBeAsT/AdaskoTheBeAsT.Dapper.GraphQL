using System;
using System.Data;
using System.Threading.Tasks;
using AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.Extensions;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.Test;

public class PostgreSqlIdentityTests
{
    [Fact(DisplayName = "NextIdentity throws for non-member expression")]
    public void NextIdentityThrowsForNonMemberExpression()
    {
        var connection = new Mock<IDbConnection>(MockBehavior.Strict).Object;

        var act = () => PostgreSqlIdentity.NextIdentity<Entity, int>(connection, e => e.Id + 1);

        act.Should().Throw<NotSupportedException>();
    }

    [Fact(DisplayName = "NextIdentityAsync throws for non-member expression")]
    public Task NextIdentityAsyncThrowsForNonMemberExpressionAsync()
    {
        var connection = new Mock<IDbConnection>(MockBehavior.Strict).Object;

        Func<Task<int>> act = () => PostgreSqlIdentity.NextIdentityAsync<Entity, int>(connection, e => e.Id + 1);

        return act.Should().ThrowAsync<NotSupportedException>();
    }

    public class Entity
    {
        public int Id { get; set; }
    }
}
