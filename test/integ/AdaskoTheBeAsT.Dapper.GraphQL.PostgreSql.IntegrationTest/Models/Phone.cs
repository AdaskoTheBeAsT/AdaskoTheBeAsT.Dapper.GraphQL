namespace AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest.Models
{
    public class Phone
    {
        public int Id { get; set; }

        public string? Number { get; set; }

        public PhoneType Type { get; set; }
    }
}
