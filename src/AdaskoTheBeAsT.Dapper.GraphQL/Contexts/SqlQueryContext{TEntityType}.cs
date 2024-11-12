namespace AdaskoTheBeAsT.Dapper.GraphQL.Contexts
{
    public class SqlQueryContext<TEntityType> :
        SqlQueryContext
        where TEntityType : class
    {
        public SqlQueryContext(string? alias = null)
            : base(alias == null ? typeof(TEntityType).Name : $"{typeof(TEntityType).Name} {alias}")
        {
            Types.Add(typeof(TEntityType));
        }
    }
}
