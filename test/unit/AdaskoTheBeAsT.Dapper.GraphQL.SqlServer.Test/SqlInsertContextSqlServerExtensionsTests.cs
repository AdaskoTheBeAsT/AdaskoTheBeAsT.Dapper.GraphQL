using System;
using System.Data;
using System.Threading.Tasks;
using AdaskoTheBeAsT.Dapper.GraphQL.Contexts;
using AdaskoTheBeAsT.Dapper.GraphQL.SqlServer.Extensions;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace AdaskoTheBeAsT.Dapper.GraphQL.SqlServer.Test;

public class SqlInsertContextSqlServerExtensionsTests
{
    [Fact(DisplayName = "ExecuteWithSqlServerIdentity throws for unsupported identity type")]
    public void ExecuteWithSqlServerIdentityThrowsForUnsupportedType()
    {
        var context = new SqlInsertContext("Foo", new { Id = 1 });
        var connection = new Mock<IDbConnection>(MockBehavior.Loose).Object;

        var act = () => context.ExecuteWithSqlServerIdentity<Guid>(connection);

        act.Should().Throw<InvalidCastException>()
            .WithMessage("*is not supported in this SQL Server context*");
    }

    [Fact(DisplayName = "ExecuteWithSqlServerIdentityAsync throws for unsupported identity type")]
    public Task ExecuteWithSqlServerIdentityAsyncThrowsForUnsupportedType()
    {
        var context = new SqlInsertContext("Foo", new { Id = 1 });
        var connection = new Mock<IDbConnection>(MockBehavior.Loose).Object;

        Func<Task<Guid>> act = () => context.ExecuteWithSqlServerIdentityAsync<Guid>(connection);

        return act.Should().ThrowAsync<InvalidCastException>()
            .WithMessage("*is not supported in this SQL Server context*");
    }
}
