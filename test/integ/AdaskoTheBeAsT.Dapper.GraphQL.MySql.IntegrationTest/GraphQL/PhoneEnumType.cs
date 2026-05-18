using GraphQL.Types;

namespace AdaskoTheBeAsT.Dapper.GraphQL.MySql.IntegrationTest.GraphQL;

public class PhoneEnumType
    : EnumerationGraphType<Models.PhoneType>
{
    public PhoneEnumType()
    {
#pragma warning disable CC0021 // Use nameof
        Name = "PhoneType";
#pragma warning restore CC0021 // Use nameof
    }
}
