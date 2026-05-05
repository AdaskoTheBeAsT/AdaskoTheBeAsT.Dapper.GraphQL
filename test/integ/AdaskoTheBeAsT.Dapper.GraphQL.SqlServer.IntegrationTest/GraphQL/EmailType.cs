using AdaskoTheBeAsT.Dapper.GraphQL.SqlServer.IntegrationTest.Models;
using GraphQL.Types;

namespace AdaskoTheBeAsT.Dapper.GraphQL.SqlServer.IntegrationTest.GraphQL
{
    public class EmailType :
        ObjectGraphType<Email>
    {
        public EmailType()
        {
            Name = "email";
            Description = "An email address.";

#pragma warning disable MA0056 // Do not call overridable members in constructor
            Field<IntGraphType>("id")
                .Description("A unique identifier for the email address.")
                .Resolve(context => context.Source?.Id);

            Field<StringGraphType>("address")
                .Description("The email address.")
                .Resolve(context => context.Source?.Address);
#pragma warning restore MA0056 // Do not call overridable members in constructor
        }
    }
}
