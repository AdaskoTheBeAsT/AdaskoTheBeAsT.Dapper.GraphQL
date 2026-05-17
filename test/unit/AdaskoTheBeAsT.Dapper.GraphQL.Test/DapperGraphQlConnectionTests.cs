using System.Data;
using System.Data.Common;
using AwesomeAssertions;
using Xunit;

namespace AdaskoTheBeAsT.Dapper.GraphQL.Test;

public class DapperGraphQlConnectionTests
{
    [Fact(DisplayName = "Constructor stores options")]
    public void ConstructorStoresOptions()
    {
        using var inner = new FakeDbConnection();
        var options = new SqlBuilderOptions { ParameterPrefix = ":" };

        using var sut = new DapperGraphQlConnection(inner, options);

        sut.Options.Should().BeSameAs(options);
    }

    [Fact(DisplayName = "ConnectionString proxies to inner connection")]
    public void ConnectionStringProxiesInner()
    {
        using var inner = new FakeDbConnection { ConnectionString = "host=server" };
        using var sut = new DapperGraphQlConnection(inner, new SqlBuilderOptions());

        sut.ConnectionString.Should().Be("host=server");

        sut.ConnectionString = "host=other";
        inner.ConnectionString.Should().Be("host=other");
    }

    [Fact(DisplayName = "Database/DataSource/ServerVersion/State proxy to inner connection")]
    public void ReadOnlyPropertiesProxyInner()
    {
        using var inner = new FakeDbConnection
        {
            DatabaseValue = "db",
            DataSourceValue = "ds",
            ServerVersionValue = "1.0",
            StateValue = ConnectionState.Open,
        };
        using var sut = new DapperGraphQlConnection(inner, new SqlBuilderOptions());

        sut.Database.Should().Be("db");
        sut.DataSource.Should().Be("ds");
        sut.ServerVersion.Should().Be("1.0");
        sut.State.Should().Be(ConnectionState.Open);
    }

    [Fact(DisplayName = "ChangeDatabase/Open/Close forward to inner connection")]
    public void LifecycleMethodsForwardToInner()
    {
        using var inner = new FakeDbConnection();
        using var sut = new DapperGraphQlConnection(inner, new SqlBuilderOptions());

        sut.Open();
        sut.ChangeDatabase("other");
        sut.Close();

        inner.OpenCalls.Should().Be(1);
        inner.CloseCalls.Should().Be(1);
        inner.ChangeDatabaseCalls.Should().Be(1);
        inner.LastChangeDatabaseName.Should().Be("other");
    }

    [Fact(DisplayName = "Dispose forwards to inner connection")]
    public void DisposeForwardsToInner()
    {
        using var inner = new FakeDbConnection();
        using (var sut = new DapperGraphQlConnection(inner, new SqlBuilderOptions()))
        {
            sut.Dispose();
        }

        inner.DisposeCalls.Should().BeGreaterThan(0);
    }

    [Fact(DisplayName = "WithDapperGraphQlOptions wraps a DbConnection")]
    public void WithDapperGraphQlOptionsWrapsConnection()
    {
        using var inner = new FakeDbConnection();
        var options = new SqlBuilderOptions { ParameterPrefix = "$" };

        using var wrapped = (DapperGraphQlConnection)inner.WithDapperGraphQlOptions(options);

        wrapped.Should().NotBeNull();
        wrapped.Options.ParameterPrefix.Should().Be("$");
    }

    private sealed class FakeDbConnection : DbConnection
    {
        public string DatabaseValue { get; set; } = string.Empty;

        public string DataSourceValue { get; set; } = string.Empty;

        public string ServerVersionValue { get; set; } = string.Empty;

        public ConnectionState StateValue { get; set; } = ConnectionState.Closed;

        public int OpenCalls { get; private set; }

        public int CloseCalls { get; private set; }

        public int ChangeDatabaseCalls { get; private set; }

        public int DisposeCalls { get; private set; }

        public string? LastChangeDatabaseName { get; private set; }

#pragma warning disable CS8765
        public override string ConnectionString { get; set; } = string.Empty;
#pragma warning restore CS8765

        public override string Database => DatabaseValue;

        public override string DataSource => DataSourceValue;

        public override string ServerVersion => ServerVersionValue;

        public override ConnectionState State => StateValue;

        public override void ChangeDatabase(string databaseName)
        {
            ChangeDatabaseCalls++;
            LastChangeDatabaseName = databaseName;
        }

        public override void Close() => CloseCalls++;

        public override void Open() => OpenCalls++;

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) =>
            throw new System.NotSupportedException();

        protected override DbCommand CreateDbCommand() =>
            throw new System.NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            DisposeCalls++;
            base.Dispose(disposing);
        }
    }
}
