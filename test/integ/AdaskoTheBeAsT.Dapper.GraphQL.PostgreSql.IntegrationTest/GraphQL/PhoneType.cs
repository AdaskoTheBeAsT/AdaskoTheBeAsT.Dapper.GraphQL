using AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest.Models;
using GraphQL.Types;

namespace AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest.GraphQL
{
    public class PhoneType :
        ObjectGraphType<Phone>
    {
        public PhoneType()
        {
            Name = "phone";
            Description = "A phone number.";

#pragma warning disable MA0056 // Do not call overridable members in constructor
            Field<IntGraphType>("id")
                .Description("A unique identifier for the phone number.")
                .Resolve(context => context.Source?.Id);

            Field<StringGraphType>("number")
                .Description("The phone number.")
                .Resolve(context => context.Source?.Number);

            Field<PhoneEnumType>("type")
                .Description("The type of phone number.  One of 'home', 'work', 'mobile', or 'other'.")
                .Resolve(context => context.Source?.Type);
#pragma warning restore MA0056 // Do not call overridable members in constructor
        }
    }
}
