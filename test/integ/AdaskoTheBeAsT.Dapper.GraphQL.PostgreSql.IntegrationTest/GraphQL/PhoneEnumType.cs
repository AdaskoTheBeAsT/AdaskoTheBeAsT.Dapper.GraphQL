using GraphQL.Types;

namespace AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest.GraphQL
{
    public class PhoneEnumType
        : EnumerationGraphType<Models.PhoneType>
    {
        public PhoneEnumType()
        {
            Name = "PhoneType";
        }
    }
}
