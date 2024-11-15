using System;
using System.Linq;
using AdaskoTheBeAsT.Dapper.GraphQL.Contexts;
using AdaskoTheBeAsT.Dapper.GraphQL.Interfaces;
using AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest.Models;
using GraphQLParser.AST;

namespace AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest.QueryBuilders
{
    public class PersonQueryBuilder :
        IQueryBuilder<Person>
    {
        private readonly IQueryBuilder<Company> _companyQueryBuilder;
        private readonly IQueryBuilder<Email> _emailQueryBuilder;
        private readonly IQueryBuilder<Phone> _phoneQueryBuilder;

        public PersonQueryBuilder(
            IQueryBuilder<Company> companyQueryBuilder,
            IQueryBuilder<Email> emailQueryBuilder,
            IQueryBuilder<Phone> phoneQueryBuilder)
        {
            _companyQueryBuilder = companyQueryBuilder;
            _emailQueryBuilder = emailQueryBuilder;
            _phoneQueryBuilder = phoneQueryBuilder;
        }

#pragma warning disable MA0051 // Method is too long
        public SqlQueryContext Build(SqlQueryContext query, IHasSelectionSetNode context, string alias)
#pragma warning restore MA0051 // Method is too long
        {
            var mergedAlias = $"{alias}Merged";

            // Deduplicate the person
            query.LeftJoin($"Person {mergedAlias} ON {alias}.MergedToPersonId = {mergedAlias}.MergedToPersonId");
            _ = query.Select(new[] { $"{alias}.Id", $"{alias}.MergedToPersonId" });
            query.SplitOn<Person>("Id");

            var fields = QueryBuilderHelper.CollectFields(context.SelectionSet);

            if (QueryBuilderHelper.IsConnection(context.SelectionSet))
            {
                query.Select($"{alias}.CreateDate");
            }

            if (fields.Keys.Any(k => k.StringValue.Equals("firstName", StringComparison.OrdinalIgnoreCase)))
            {
                query.Select($"{mergedAlias}.FirstName");
            }

            if (fields.Keys.Any(k => k.StringValue.Equals("lastName", StringComparison.OrdinalIgnoreCase)))
            {
                query.Select($"{mergedAlias}.LastName");
            }

            var companiesKey =
                fields.Keys.FirstOrDefault(k => k.StringValue.Equals("companies", StringComparison.OrdinalIgnoreCase));
            if (companiesKey != (GraphQLName?)null)
            {
                var personCompanyAlias = $"{alias}PersonCompany";
                var companyAlias = $"{alias}Company";
                query
                    .LeftJoin($"PersonCompany {personCompanyAlias} ON {mergedAlias}.Id = {personCompanyAlias}.PersonId")
                    .LeftJoin($"Company {companyAlias} ON {personCompanyAlias}.CompanyId = {companyAlias}.Id");
                query = _companyQueryBuilder.Build(query, fields[companiesKey], companyAlias);
            }

            var emailsKey =
                fields.Keys.FirstOrDefault(k => k.StringValue.Equals("emails", StringComparison.OrdinalIgnoreCase));
            if (emailsKey != (GraphQLName?)null)
            {
                var personEmailAlias = $"{alias}PersonEmail";
                var emailAlias = $"{alias}Email";
                query
                    .LeftJoin($"PersonEmail {personEmailAlias} ON {mergedAlias}.Id = {personEmailAlias}.PersonId")
                    .LeftJoin($"Email {emailAlias} ON {personEmailAlias}.EmailId = {emailAlias}.Id");
                query = _emailQueryBuilder.Build(query, fields[emailsKey], emailAlias);
            }

            var phonesKey =
                fields.Keys.FirstOrDefault(k => k.StringValue.Equals("phones", StringComparison.OrdinalIgnoreCase));
            if (phonesKey != (GraphQLName?)null)
            {
                var personPhoneAlias = $"{alias}PersonPhone";
                var phoneAlias = $"{alias}Phone";
                query
                    .LeftJoin($"PersonPhone {personPhoneAlias} ON {mergedAlias}.Id = {personPhoneAlias}.PersonId")
                    .LeftJoin($"Phone {phoneAlias} ON {personPhoneAlias}.PhoneId = {phoneAlias}.Id");
                query = _phoneQueryBuilder.Build(query, fields[phonesKey], phoneAlias);
            }

            var supervisorKey =
                fields.Keys.FirstOrDefault(k => k.StringValue.Equals("supervisor", StringComparison.OrdinalIgnoreCase));
            if (supervisorKey != (GraphQLName?)null)
            {
                var supervisorAlias = $"{alias}Supervisor";
                query.LeftJoin($"Person {supervisorAlias} ON {mergedAlias}.SupervisorId = {supervisorAlias}.Id");
                query = Build(query, fields[supervisorKey], supervisorAlias);
            }

            var careerCounselorKey =
                fields.Keys.FirstOrDefault(k => k.StringValue.Equals("careerCounselor", StringComparison.OrdinalIgnoreCase));
            if (careerCounselorKey != (GraphQLName?)null)
            {
                var careerCounselorAlias = $"{alias}CareerCounselor";
                query.LeftJoin($"Person {careerCounselorAlias} ON {mergedAlias}.CareerCounselorId = {careerCounselorAlias}.Id");
                query = Build(query, fields[careerCounselorKey], careerCounselorAlias);
            }

            return query;
        }
    }
}
