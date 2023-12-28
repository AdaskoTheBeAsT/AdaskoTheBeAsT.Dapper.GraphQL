using System;
using System.Data;
using System.Linq;
using AdaskoTheBeAsT.Dapper.GraphQL.Interfaces;
using AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest.EntityMappers;
using AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest.Models;
using GraphQL;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;

namespace AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest.GraphQL
{
    public class PersonMutation : ObjectGraphType
    {
        public PersonMutation(IQueryBuilder<Person> personQueryBuilder, IServiceProvider serviceProvider)
        {
            Field<PersonType>("addPerson")
                .Description("Adds new person.")
                .Arguments(new QueryArguments(
                    new QueryArgument<PersonInputType> { Name = "person" }))
                .Resolve(context =>
                {
                    var person = context.GetArgument<Person>("person");

                    using (var connection = serviceProvider.GetRequiredService<IDbConnection>())
                    {
                        person.Id = person.MergedToPersonId = Extensions.PostgreSql.NextIdentity(connection, (Person p) => p.Id);

                        var success = AdaskoTheBeAsT.Dapper.GraphQL.SqlBuilder
                            .Insert(person)
                            .Execute(connection) > 0;

                        if (success)
                        {
                            var personMapper = new PersonEntityMapper();

                            var query = AdaskoTheBeAsT.Dapper.GraphQL.SqlBuilder
                                .From<Person>("Person")
                                .Select("FirstName, LastName")
                                .Where("ID = @personId", new { personId = person.Id });

                            var results = query
                                .Execute(connection, context.FieldAst, personMapper)
                                .Distinct();
                            return results.FirstOrDefault();
                        }

                        return null;
                    }
                });
        }
    }
}
