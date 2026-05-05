using System;
using System.Data;
using System.Linq;
using AdaskoTheBeAsT.Dapper.GraphQL.Interfaces;
using AdaskoTheBeAsT.Dapper.GraphQL.Sqlite.Extensions;
using AdaskoTheBeAsT.Dapper.GraphQL.Sqlite.IntegrationTest.EntityMappers;
using AdaskoTheBeAsT.Dapper.GraphQL.Sqlite.IntegrationTest.Models;
using GraphQL;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;

namespace AdaskoTheBeAsT.Dapper.GraphQL.Sqlite.IntegrationTest.GraphQL
{
    public class PersonMutation : ObjectGraphType
    {
        public PersonMutation(IQueryBuilder<Person> personQueryBuilder, IServiceProvider serviceProvider)
        {
#pragma warning disable MA0056
            Field<PersonType>("addPerson")
                .Description("Adds new person.")
                .Arguments(new QueryArguments(
                    new QueryArgument<PersonInputType> { Name = "person" }))
                .Resolve(context =>
                {
                    var person = context.GetArgument<Person>("person");

                    using var connection = serviceProvider.GetRequiredService<IDbConnection>();
                    connection.Open();

                    var newId = SqlBuilder
                        .Insert(person)
                        .ExecuteWithSqliteIdentity(connection);

                    person.Id = person.MergedToPersonId = newId;

                    SqlBuilder
                        .Update(nameof(Person), new { MergedToPersonId = newId })
                        .Where("Id = @id", new { id = newId })
                        .Execute(connection);

                    var personMapper = new PersonEntityMapper();

                    var query = SqlBuilder
                        .From<Person>(nameof(Person))
                        .Select(["FirstName, LastName"])
                        .Where("ID = @personId", new { personId = person.Id });

                    var results = query
                        .Execute(connection, context.FieldAst, personMapper)
                        .Distinct();
                    return results.FirstOrDefault();
                });
#pragma warning restore MA0056
        }
    }
}
