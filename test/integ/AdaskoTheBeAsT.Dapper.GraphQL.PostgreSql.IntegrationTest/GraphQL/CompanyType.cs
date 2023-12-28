using AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest.Models;
using GraphQL.Types;

namespace AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest.GraphQL
{
    public class CompanyType :
        ObjectGraphType<Company>
    {
        public CompanyType()
        {
            Name = "company";
            Description = "A company.";

#pragma warning disable MA0056 // Do not call overridable members in constructor
            Field<IntGraphType>("id")
                .Description("A unique identifier for the company.")
                .Resolve(context => context.Source?.Id);

            Field<StringGraphType>("name")
                .Description("The name of the company.")
                .Resolve(context => context.Source?.Name);

            Field<ListGraphType<EmailType>>("emails")
                .Description("A list of email addresses for the company.")
                .Resolve(context => context.Source?.Emails);

            Field<ListGraphType<PhoneType>>("phones")
                .Description("A list of phone numbers for the company.")
                .Resolve(context => context.Source?.Phones);
#pragma warning restore MA0056 // Do not call overridable members in constructor
        }
    }
}
