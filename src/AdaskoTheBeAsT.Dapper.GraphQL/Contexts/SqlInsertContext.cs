using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdaskoTheBeAsT.Dapper.GraphQL.Extensions;
using Dapper;

namespace AdaskoTheBeAsT.Dapper.GraphQL.Contexts
{
    public class SqlInsertContext
    {
        private readonly HashSet<string> _insertParameterNames;

        private List<SqlInsertContext>? _inserts;

        public SqlInsertContext(
            string table,
            dynamic? parameters = null)
        {
            if (parameters != null && !(parameters is IEnumerable<KeyValuePair<string, object>>))
            {
                parameters = ParameterHelper.GetSetFlatProperties(parameters);
            }

            Parameters = new DynamicParameters(parameters);
            _insertParameterNames = new HashSet<string>(Parameters.ParameterNames, StringComparer.OrdinalIgnoreCase);
            Table = table;
        }

        public DynamicParameters Parameters { get; set; }

        public string Table { get; }

        /// <summary>
        /// Executes the INSERT statements with Dapper, using the provided database connection.
        /// </summary>
        /// <param name="connection">The database connection.</param>
        /// <param name="transaction">The transaction to execute under (optional).</param>
        /// <param name="options">The options for the command (optional).</param>
        public int Execute(IDbConnection connection, IDbTransaction? transaction = null, SqlMapperOptions? options = null)
        {
            if (options == null)
            {
                options = SqlMapperOptions.DefaultOptions;
            }

            var result = connection.Execute(BuildSql(), Parameters, transaction, options.CommandTimeout, options.CommandType);
            if (_inserts != null)
            {
                // Execute each insert and aggregate the results
                result = _inserts.Aggregate(result, (current, insert) => current + insert.Execute(connection, transaction, options));
            }

            return result;
        }

        /// <summary>
        /// Executes the INSERT statements with Dapper asynchronously, using the provided database connection.
        /// </summary>
        /// <param name="connection">The database connection.</param>
        /// <param name="transaction">The transaction to execute under (optional).</param>
        /// <param name="options">The options for the command (optional).</param>
        public async Task<int> ExecuteAsync(IDbConnection connection, IDbTransaction? transaction = null, SqlMapperOptions? options = null)
        {
            if (options == null)
            {
                options = SqlMapperOptions.DefaultOptions;
            }

            var result = await connection.ExecuteAsync(
                BuildSql(),
                Parameters,
                transaction,
                options.CommandTimeout,
                options.CommandType).ConfigureAwait(false);

            if (_inserts != null)
            {
                // Execute each insert and aggregate the results
                result = await _inserts.AggregateAsync(
                        result,
                        async (
                            current,
                            insert) => current + await insert.ExecuteAsync(connection, transaction, options)
                            .ConfigureAwait(false))
                    .ConfigureAwait(false);
            }

            return result;
        }

        /// <summary>
        /// Adds an additional INSERT statement after this one.
        /// </summary>
        /// <typeparam name="TEntityType">The type of entity to be inserted.</typeparam>
        /// <param name="obj">The data to be inserted.</param>
        /// <returns>The context of the INSERT statement.</returns>
        public virtual SqlInsertContext Insert<TEntityType>(TEntityType obj)
            where TEntityType : class
        {
            if (_inserts == null)
            {
                _inserts = [];
            }

            var insert = SqlBuilder.Insert(obj);
            _inserts.Add(insert);
            return this;
        }

        /// <summary>
        /// Adds an additional INSERT statement after this one.
        /// </summary>
        /// <param name="table">The table to insert data into.</param>
        /// <param name="parameters">The data to be inserted.</param>
        /// <returns>The context of the INSERT statement.</returns>
        public SqlInsertContext Insert(string table, dynamic? parameters = null)
        {
            if (_inserts == null)
            {
                _inserts = [];
            }

            var insert = SqlBuilder.Insert(table, parameters);
            _inserts.Add(insert);
            return this;
        }

        /// <summary>
        /// Renders the generated SQL statement.
        /// </summary>
        /// <returns>The rendered SQL statement.</returns>
        public override string ToString()
        {
            return BuildSql();
        }

        /// <summary>
        /// Builds the INSERT statement.
        /// </summary>
        /// <returns>A SQL INSERT statement.</returns>
        private string BuildSql()
        {
            var sb = new StringBuilder();
            sb.Append("INSERT INTO ").Append(Table).Append(" (");
            sb.Append(string.Join(", ", _insertParameterNames));
            sb.Append(") VALUES (");
            sb.Append(string.Join(", ", _insertParameterNames.Select(name => $"@{name}")));
            sb.Append(");");
            return sb.ToString();
        }
    }
}
