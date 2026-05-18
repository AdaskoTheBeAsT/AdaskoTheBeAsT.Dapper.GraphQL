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

        inner.DisposeCalls.Should().BePositive();
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

    [Fact(DisplayName = "BeginTransaction forwards to inner connection")]
    public void BeginTransactionForwardsToInner()
    {
        using var inner = new FakeDbConnection();
        using var sut = new DapperGraphQlConnection(inner, new SqlBuilderOptions());

        using var transaction = sut.BeginTransaction(IsolationLevel.Serializable);

        transaction.Should().NotBeNull();
        inner.LastIsolationLevel.Should().Be(IsolationLevel.Serializable);
    }

    [Fact(DisplayName = "CreateCommand forwards to inner connection")]
    public void CreateCommandForwardsToInner()
    {
        using var inner = new FakeDbConnection();
        using var sut = new DapperGraphQlConnection(inner, new SqlBuilderOptions());

        using var command = sut.CreateCommand();

        command.Should().NotBeNull();
        inner.CreateCommandCalls.Should().Be(1);
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

        public int CreateCommandCalls { get; private set; }

        public string? LastChangeDatabaseName { get; private set; }

        public IsolationLevel? LastIsolationLevel { get; private set; }

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

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            LastIsolationLevel = isolationLevel;
            return new FakeDbTransaction(this, isolationLevel);
        }

        protected override DbCommand CreateDbCommand()
        {
            CreateCommandCalls++;
            return new FakeDbCommand();
        }

        protected override void Dispose(bool disposing)
        {
            DisposeCalls++;
            base.Dispose(disposing);
        }
    }

    private sealed class FakeDbTransaction : DbTransaction
    {
        public FakeDbTransaction(DbConnection connection, IsolationLevel isolationLevel)
        {
            DbConnection = connection;
            IsolationLevel = isolationLevel;
        }

        public override IsolationLevel IsolationLevel { get; }

        protected override DbConnection DbConnection { get; }

        public override void Commit()
        {
        }

        public override void Rollback()
        {
        }
    }

    private sealed class FakeDbCommand : DbCommand
    {
#pragma warning disable CS8765
        public override string CommandText { get; set; } = string.Empty;
#pragma warning restore CS8765

        public override int CommandTimeout { get; set; }

        public override CommandType CommandType { get; set; }

        public override bool DesignTimeVisible { get; set; }

        public override UpdateRowSource UpdatedRowSource { get; set; }

        protected override DbConnection? DbConnection { get; set; }

        protected override DbParameterCollection DbParameterCollection { get; } = new FakeDbParameterCollection();

        protected override DbTransaction? DbTransaction { get; set; }

        public override void Cancel()
        {
        }

        public override int ExecuteNonQuery() => 0;

        public override object? ExecuteScalar() => null;

        public override void Prepare()
        {
        }

        protected override DbParameter CreateDbParameter() => throw new System.NotSupportedException();

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior) =>
            throw new System.NotSupportedException();
    }

    private sealed class FakeDbParameterCollection : DbParameterCollection
    {
        private readonly System.Collections.Generic.List<object> _list = [];

        public override int Count => _list.Count;

        public override object SyncRoot { get; } = new object();

        public override int Add(object value)
        {
            _list.Add(value);
            return _list.Count - 1;
        }

        public override void AddRange(System.Array values)
        {
        }

        public override void Clear() => _list.Clear();

        public override bool Contains(object value) => _list.Contains(value);

        public override bool Contains(string value) => false;

        public override void CopyTo(System.Array array, int index)
        {
        }

        public override System.Collections.IEnumerator GetEnumerator() => _list.GetEnumerator();

        public override int IndexOf(object value) => _list.IndexOf(value);

        public override int IndexOf(string parameterName) => -1;

        public override void Insert(int index, object value) => _list.Insert(index, value);

        public override void Remove(object value) => _list.Remove(value);

        public override void RemoveAt(int index) => _list.RemoveAt(index);

        public override void RemoveAt(string parameterName)
        {
        }

        protected override DbParameter GetParameter(int index) => throw new System.NotSupportedException();

        protected override DbParameter GetParameter(string parameterName) => throw new System.NotSupportedException();

        protected override void SetParameter(int index, DbParameter value)
        {
        }

        protected override void SetParameter(string parameterName, DbParameter value)
        {
        }
    }
}
