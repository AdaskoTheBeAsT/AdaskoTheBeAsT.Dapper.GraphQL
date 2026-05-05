namespace AdaskoTheBeAsT.Dapper.GraphQL.Contexts
{
    public class SqlInsertContext<TEntityType> :
        SqlInsertContext
        where TEntityType : class
    {
        public SqlInsertContext(string table, TEntityType obj)
            : base(table, obj)
        {
        }

        /// <summary>
        /// Adds an additional INSERT statement after this one.
        /// </summary>
        /// <param name="obj">The data to be inserted.</param>
        /// <returns>The context of the INSERT statement.</returns>
        public virtual SqlInsertContext Insert(TEntityType obj)
        {
            return base.Insert(obj);
        }
    }
}
