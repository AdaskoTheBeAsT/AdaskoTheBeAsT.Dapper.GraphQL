using System.Globalization;

namespace AdaskoTheBeAsT.Dapper.GraphQL
{
    /// <summary>
    /// A builder for SQL queries and statements inheriting the official Dapper.Sql Builder to extend its functions.
    /// </summary>
    public class DapperSqlBuilder : global::Dapper.SqlBuilder
    {
        /// <summary>
        /// If the object has an offset there is no need to the fetch function to add an offset with 0 rows to skip
        /// (offset clause is a must when using the fetch clause).
        /// </summary>
        private bool _hasOffset;

        /// <summary>
        /// Adds an Offset clause to allow pagination (it will skip N rows).
        /// </summary>
        /// <remarks>
        /// Order by clause is a must when using offset.
        /// </remarks>
        /// <example>
        ///     var queryBuilder = new SqlQueryBuilder();
        ///     queryBuilder.From("Customer customer");
        ///     queryBuilder.Select(
        ///         "customer.id",
        ///         "customer.name",
        ///     );
        ///     queryBuilder.SplitOn&lt;Customer&gt;("id");
        ///     queryBuilder.Where("customer.id == @id");
        ///     queryBuilder.Parameters.Add("id", 1);
        ///     queryBuilder.Orderby("customer.name");
        ///     queryBuilder.Offset(20);
        ///     var customer = queryBuilder
        ///         .Execute&lt;Customer&gt;(dbConnection, graphQLSelectionSet);
        ///         .FirstOrDefault();
        ///
        ///     // SELECT customer.id, customer.name
        ///     // FROM Customer customer
        ///     // WHERE customer.id == @id
        ///     // ORDER BY customer.name
        ///     // .
        /// </example>
        /// <param name="rowsToSkip">total of rows to skip.</param>
        /// <returns>The query builder.</returns>
        public DapperSqlBuilder Offset(int rowsToSkip)
        {
            _hasOffset = true;
            return (DapperSqlBuilder)AddClause(
                "offset",
                $"{rowsToSkip.ToString(CultureInfo.InvariantCulture)}",
                parameters: null,
                " + ",
                "OFFSET ",
                " ROWS\n",
                isInclusive: false);
        }

        /// <summary>
        /// Adds a fetch clause to allow pagination.
        /// </summary>
        /// <remarks>
        /// Order by clause is a must when using fetch.
        /// </remarks>
        /// <example>
        ///     var queryBuilder = new SqlQueryBuilder();
        ///     queryBuilder.From("Customer customer");
        ///     queryBuilder.Select(
        ///         "customer.id",
        ///         "customer.name",
        ///     );
        ///     queryBuilder.SplitOn&lt;Customer&gt;("id");
        ///     queryBuilder.Where("customer.id == @id");
        ///     queryBuilder.Parameters.Add("id", 1);
        ///     queryBuilder.Orderby("customer.name");
        ///     queryBuilder.Offset(20);
        ///     queryBuilder.Fetch(10);
        ///     var customer = queryBuilder
        ///         .Execute&lt;Customer&gt;(dbConnection, graphQLSelectionSet);
        ///         .FirstOrDefault();
        ///
        ///     // SELECT customer.id, customer.name
        ///     // FROM Customer customer
        ///     // WHERE customer.id == @id
        ///     // ORDER BY customer.name
        ///     // .
        /// </example>
        /// <param name="rowsToReturn">total of rows to return.</param>
        /// <returns>The query builder.</returns>
        public DapperSqlBuilder Fetch(int rowsToReturn)
        {
            if (!_hasOffset)
            {
                Offset(0);
            }

            return (DapperSqlBuilder)AddClause(
                "fetch",
                $"{rowsToReturn.ToString(CultureInfo.InvariantCulture)}",
                parameters: null,
                " + ",
                "FETCH FIRST ",
                " ROWS ONLY\n",
                isInclusive: false);
        }
    }
}
