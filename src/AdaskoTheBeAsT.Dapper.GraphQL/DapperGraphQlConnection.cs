using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace AdaskoTheBeAsT.Dapper.GraphQL;

[SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP005", Justification = "Wrapper")]
[SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP007", Justification = "Wrapper")]
public class DapperGraphQlConnection : DbConnection, IDapperGraphQlConnection
{
    private readonly DbConnection _inner;

    public DapperGraphQlConnection(DbConnection inner, SqlBuilderOptions options)
    {
        _inner = inner;
        Options = options;
    }

    public SqlBuilderOptions Options { get; }

#if NET8_0_OR_GREATER
    [AllowNull]
#endif
    public override string ConnectionString
    {
        get => _inner.ConnectionString;
        set => _inner.ConnectionString = value;
    }

    public override string Database => _inner.Database;

    public override string DataSource => _inner.DataSource;

    public override string ServerVersion => _inner.ServerVersion;

    public override ConnectionState State => _inner.State;

    public override void ChangeDatabase(string databaseName) => _inner.ChangeDatabase(databaseName);

    public override void Close() => _inner.Close();

    public override void Open() => _inner.Open();

    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) =>
        _inner.BeginTransaction(isolationLevel);

    protected override DbCommand CreateDbCommand() => _inner.CreateCommand();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _inner.Dispose();
        }

        base.Dispose(disposing);
    }
}
