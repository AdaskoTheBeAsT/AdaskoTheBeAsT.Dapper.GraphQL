using System.Collections.Generic;

namespace AdaskoTheBeAsT.Dapper.GraphQL.Contexts
{
    public class SqlInsertContext<TEntityType> :
        SqlInsertContext
        where TEntityType : class
    {
        private List<SqlInsertContext<TEntityType>>? _inserts;

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
            if (_inserts == null)
            {
                _inserts = new List<SqlInsertContext<TEntityType>>();
            }

            var insert = SqlBuilder.Insert(obj);
            _inserts.Add(insert);
            return this;
        }
    }
}
