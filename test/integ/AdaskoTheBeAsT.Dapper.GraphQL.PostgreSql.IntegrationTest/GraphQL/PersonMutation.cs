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
#pragma warning disable MA0056 // Do not call overridable members in constructor
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

                        var success = SqlBuilder
                            .Insert(person)
                            .Execute(connection) > 0;

                        if (success)
                        {
                            var personMapper = new PersonEntityMapper();

                            var query = SqlBuilder
                                .From<Person>(nameof(Person))
                                .Select(new[] { "FirstName, LastName" })
                                .Where("ID = @personId", new { personId = person.Id });

                            var results = query
                                .Execute(connection, context.FieldAst, personMapper)
                                .Distinct();
                            return results.FirstOrDefault();
                        }

                        return null;
                    }
                });
#pragma warning restore MA0056 // Do not call overridable members in constructor
        }
    }
}
