using AdaskoTheBeAsT.Dapper.GraphQL.Contexts;
using AdaskoTheBeAsT.Dapper.GraphQL.Interfaces;

namespace AdaskoTheBeAsT.Dapper.GraphQL
{
    public class EntityMapper<TEntityType> :
        IEntityMapper<TEntityType>
        where TEntityType : class
    {
        public virtual TEntityType Map(EntityMapContext context)
        {
            var entity = context.Start<TEntityType>();
            return entity;
        }
    }
}
