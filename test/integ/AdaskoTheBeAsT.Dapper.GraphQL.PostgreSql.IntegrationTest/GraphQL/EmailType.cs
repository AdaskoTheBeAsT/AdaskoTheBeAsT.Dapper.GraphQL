using AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest.Models;
using GraphQL.Types;

namespace AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest.GraphQL
{
    public class EmailType :
        ObjectGraphType<Email>
    {
        public EmailType()
        {
            Name = "email";
            Description = "An email address.";

            Field<IntGraphType>("id")
                .Description("A unique identifier for the email address.")
                .Resolve(context => context.Source?.Id);

            Field<StringGraphType>("address")
                .Description("The email address.")
                .Resolve(context => context.Source?.Address);
        }
    }
}
