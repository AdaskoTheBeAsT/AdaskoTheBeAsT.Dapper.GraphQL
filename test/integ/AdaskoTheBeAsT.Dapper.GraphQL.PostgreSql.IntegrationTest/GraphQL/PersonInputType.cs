using GraphQL.Types;

namespace AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest.GraphQL
{
    public class PersonInputType : InputObjectGraphType
    {
        public PersonInputType()
        {
            Name = "PersonInput";

#pragma warning disable MA0056 // Do not call overridable members in constructor
            Field<StringGraphType>("firstName");
            Field<StringGraphType>("lastName");
#pragma warning restore MA0056 // Do not call overridable members in constructor
        }
    }
}
