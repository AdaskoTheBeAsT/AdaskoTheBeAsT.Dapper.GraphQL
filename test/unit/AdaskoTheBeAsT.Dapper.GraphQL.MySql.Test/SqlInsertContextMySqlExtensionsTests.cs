using System;
using System.Data;
using System.Threading.Tasks;
using AdaskoTheBeAsT.Dapper.GraphQL.Contexts;
using AdaskoTheBeAsT.Dapper.GraphQL.MySql.Extensions;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace AdaskoTheBeAsT.Dapper.GraphQL.MySql.Test;

public class SqlInsertContextMySqlExtensionsTests
{
    [Fact(DisplayName = "ExecuteWithMySqlIdentity throws for unsupported identity type")]
    public void ExecuteWithMySqlIdentityThrowsForUnsupportedType()
    {
        var context = new SqlInsertContext("Foo", new { Id = 1 });
        var connection = new Mock<IDbConnection>(MockBehavior.Loose).Object;

        var act = () => context.ExecuteWithMySqlIdentity<Guid>(connection);

        act.Should().Throw<InvalidCastException>()
            .WithMessage("*is not supported in the MySQL context*");
    }

    [Fact(DisplayName = "ExecuteWithMySqlIdentityAsync throws for unsupported identity type")]
    public Task ExecuteWithMySqlIdentityAsyncThrowsForUnsupportedType()
    {
        var context = new SqlInsertContext("Foo", new { Id = 1 });
        var connection = new Mock<IDbConnection>(MockBehavior.Loose).Object;

        Func<Task<Guid>> act = () => context.ExecuteWithMySqlIdentityAsync<Guid>(connection);

        return act.Should().ThrowAsync<InvalidCastException>();
    }
}
