using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdaskoTheBeAsT.Dapper.GraphQL.Extensions;
using Dapper;

namespace AdaskoTheBeAsT.Dapper.GraphQL.Contexts;

public class SqlDeleteContext
{
    private List<SqlDeleteContext>? _deletes;

    public SqlDeleteContext(
        string table,
        dynamic? parameters = null)
    {
        if (parameters != null && !(parameters is IEnumerable<KeyValuePair<string, object>>))
        {
            parameters = ParameterHelper.GetSetFlatProperties(parameters);
        }

        Parameters = new DynamicParameters(parameters);
        Table = table;
    }

    public DynamicParameters Parameters { get; set; }

    public string Table { get; }

    public static SqlDeleteContext Delete<TEntityType>(dynamic? parameters = null)
    {
        return new SqlDeleteContext(typeof(TEntityType).Name, parameters);
    }

    /// <summary>
    /// Adds an additional DELETE statement after this one.
    /// </summary>
    /// <param name="table">The table to delete data from.</param>
    /// <param name="parameters">The data to be deleted.</param>
    /// <returns>The context of the DELETE statement.</returns>
    public SqlDeleteContext Delete(string table, dynamic? parameters = null)
    {
        _deletes ??= [];

        var delete = SqlBuilder.Delete(table, parameters);
        _deletes.Add(delete);
        return this;
    }

    /// <summary>
    /// Executes the DELETE statement with Dapper, using the provided database connection.
    /// </summary>
    /// <param name="connection">The database connection.</param>
    /// <param name="transaction">The transaction to execute under (optional).</param>
    /// <param name="options">The options for the command (optional).</param>
    public int Execute(IDbConnection connection, IDbTransaction? transaction = null, SqlMapperOptions? options = null)
    {
        options ??= SqlMapperOptions.DefaultOptions;

        var result = connection.Execute(BuildSql(GetPrefix(connection)), Parameters, transaction, options.CommandTimeout, options.CommandType);
        if (_deletes != null)
        {
            // Execute each delete and aggregate the results
            result = _deletes.Aggregate(result, (current, delete) => current + delete.Execute(connection, transaction, options));
        }

        return result;
    }

    /// <summary>
    /// Executes the DELETE statement with Dapper asynchronously, using the provided database connection.
    /// </summary>
    /// <param name="connection">The database connection.</param>
    /// <param name="transaction">The transaction to execute under (optional).</param>
    /// <param name="options">The options for the command (optional).</param>
    public async Task<int> ExecuteAsync(IDbConnection connection, IDbTransaction? transaction = null, SqlMapperOptions? options = null)
    {
        options ??= SqlMapperOptions.DefaultOptions;

        var result = await connection.ExecuteAsync(BuildSql(GetPrefix(connection)), Parameters, transaction, options.CommandTimeout, options.CommandType)
            .ConfigureAwait(false);

        if (_deletes != null)
        {
            // Execute each delete and aggregate the results
            result = await _deletes.AggregateAsync(
                    result,
                    async (
                        current,
                        delete) => current + await delete.ExecuteAsync(connection, transaction, options)
                        .ConfigureAwait(false))
                .ConfigureAwait(false);
        }

        return result;
    }

    /// <summary>
    /// Renders the generated SQL statement.
    /// </summary>
    /// <returns>The rendered SQL statement.</returns>
    public override string ToString()
    {
        return BuildSql(SqlBuilderOptions.Default.ParameterPrefix);
    }

    private static string GetPrefix(IDbConnection connection)
    {
        return connection is IDapperGraphQlConnection wrapper
            ? wrapper.Options.ParameterPrefix
            : SqlBuilderOptions.Default.ParameterPrefix;
    }

    private string BuildSql(string parameterPrefix)
    {
        var sb = new StringBuilder();
        sb.Append("DELETE FROM ").Append(Table).Append(" WHERE ");
#if NET8_0_OR_GREATER
        sb.AppendJoin(" AND ", Parameters.ParameterNames.Select(name => $"{name} = {parameterPrefix}{name}"));
#else
        sb.Append(string.Join(" AND ", Parameters.ParameterNames.Select(name => $"{name} = {parameterPrefix}{name}")));
#endif
        return sb.ToString();
    }
}
