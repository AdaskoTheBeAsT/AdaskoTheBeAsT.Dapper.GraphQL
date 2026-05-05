using System;
using Microsoft.Extensions.DependencyInjection;

namespace AdaskoTheBeAsT.Dapper.GraphQL.MySql.IntegrationTest.GraphQL
{
    public class PersonSchema :
        global::GraphQL.Types.Schema
    {
        public PersonSchema(IServiceProvider services)
            : base(services)
        {
            Query = services.GetService<PersonQuery>()!;
            Mutation = services.GetService<PersonMutation>();
        }
    }
}
