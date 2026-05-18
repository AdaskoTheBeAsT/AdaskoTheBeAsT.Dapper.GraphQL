using System.Collections.Generic;

namespace AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest.Models;

public class Company
{
    public Company()
    {
        Emails = [];
        Phones = [];
    }

    public int Id { get; set; }

    public string? Name { get; set; }

    public IList<Email> Emails { get; set; }

    public IList<Phone> Phones { get; set; }
}
