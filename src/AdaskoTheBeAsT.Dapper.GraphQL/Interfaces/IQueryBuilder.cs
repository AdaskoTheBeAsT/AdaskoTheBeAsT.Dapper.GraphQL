using AdaskoTheBeAsT.Dapper.GraphQL.Contexts;
using GraphQLParser.AST;

namespace AdaskoTheBeAsT.Dapper.GraphQL.Interfaces
{
    /// <summary>
    /// Builds queries for a given entity type.
    /// </summary>
#pragma warning disable S2326
    public interface IQueryBuilder<TModelType>
#pragma warning restore S2326
    {
        /// <summary>
        /// Builds a query using a baseline query, the GraphQL context, and the current table alias.
        /// </summary>
        /// <param name="query">The query to augment with additional information/data.</param>
        /// <param name="context">The GraphQL selection set for the area being built.</param>
        /// <param name="alias">The alias of the entity within the query to use.</param>
        /// <returns>A query for the given type.</returns>
        SqlQueryContext Build(SqlQueryContext query, IHasSelectionSetNode context, string alias);
    }
}
