using System.Linq;
using AdaskoTheBeAsT.Dapper.GraphQL.Contexts;
using AdaskoTheBeAsT.Dapper.GraphQL.Extensions;
using AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest.Models;

namespace AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest.EntityMappers
{
    public class CompanyEntityMapper :
        DeduplicatingEntityMapper<Company>
    {
        public CompanyEntityMapper()
        {
            PrimaryKey = c => c.Id;
        }

        public override Company? Map(EntityMapContext context)
        {
            // NOTE: Order is very important here.  We must map the objects in
            // the same order they were queried in the QueryBuilder.
            var company = Deduplicate(context.Start<Company>());
            var email = context.Next<Email>("emails");
            var phone = context.Next<Phone>("phones");

            if (company != null)
            {
                if (email != null &&

                    // Eliminate duplicates
                    !company.Emails.Any(e => string.Equals(e.Address, email.Address, System.StringComparison.OrdinalIgnoreCase)))
                {
                    company.Emails.Add(email);
                }

                if (phone != null &&

                    // Eliminate duplicates
                    !company.Phones.Any(p => string.Equals(p.Number, phone.Number, System.StringComparison.OrdinalIgnoreCase)))
                {
                    company.Phones.Add(phone);
                }
            }

            return company;
        }
    }
}
