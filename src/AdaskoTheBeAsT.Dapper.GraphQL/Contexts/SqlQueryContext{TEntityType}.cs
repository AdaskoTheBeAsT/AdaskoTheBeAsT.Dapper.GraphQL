using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AdaskoTheBeAsT.Dapper.GraphQL.Interfaces;
using Dapper;
using GraphQLParser.AST;

namespace AdaskoTheBeAsT.Dapper.GraphQL.Contexts
{
    public class SqlQueryContext<TEntityType> :
        SqlQueryContext
        where TEntityType : class
    {
        public SqlQueryContext(string alias = null)
            : base(alias == null ? typeof(TEntityType).Name : $"{typeof(TEntityType).Name} {alias}")
        {
            _types.Add(typeof(TEntityType));
        }
    }
}
