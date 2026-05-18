using System.Linq;
using AdaskoTheBeAsT.Dapper.GraphQL.Contexts;
using AdaskoTheBeAsT.Dapper.GraphQL.MySql.IntegrationTest.EntityMappers;
using AdaskoTheBeAsT.Dapper.GraphQL.MySql.IntegrationTest.Models;
using AwesomeAssertions;
using Xunit;

namespace AdaskoTheBeAsT.Dapper.GraphQL.MySql.IntegrationTest;

public class EntityMapContextTests : IClassFixture<TestFixture>
{
    [Fact(DisplayName = "EntityMap properly deduplicates")]
#pragma warning disable MA0051 // Method is too long
    public void EntityMapSucceeds()
#pragma warning restore MA0051 // Method is too long
    {
        var person1 = new Person
        {
            FirstName = "Doug",
            Id = 2,
            LastName = "Day",
            MergedToPersonId = 2,
        };
        var person2 = new Person
        {
            FirstName = "Douglas",
            Id = 2,
            LastName = "Day",
            MergedToPersonId = 2,
        };

        var email1 = new Email
        {
            Address = "dday@landmarkhw.com",
            Id = 2,
        };

        var email2 = new Email
        {
            Address = "dougrday@gmail.com",
            Id = 3,
        };

        var phone = new Phone
        {
            Id = 1,
            Number = "8011234567",
            Type = PhoneType.Mobile,
        };

        var splitOn = new[]
        {
            typeof(Person),
            typeof(Email),
            typeof(Phone),
        };

        var personEntityMapper = new PersonEntityMapper();

        const string graphql = @"
{
    query {
        firstName
        lastName
        id
        emails {
            id
            address
        }
        phones {
            id
            number
            type
        }
    }
}";

        var selectionSet = TestFixture.BuildGraphQlSelection(graphql);
        using (var context1 = new EntityMapContext
               {
                   Items = new object[]
                   {
                       person1,
                       email1,
                       phone,
                   },
                   SelectionSet = selectionSet,
                   SplitOn = splitOn,
               })
        {
            person1 = personEntityMapper.Map(context1);
            context1.MappedCount.Should().Be(3);

            person1?.Id.Should().Be(2);
            (person1?.FirstName).Should().Be("Doug");
            (person1?.Emails ?? Enumerable.Empty<Email>()).Should().ContainSingle();
            (person1?.Phones ?? Enumerable.Empty<Phone>()).Should().ContainSingle();

            using (var context2 = new EntityMapContext
                   {
                       Items = new object?[]
                       {
                           person2,
                           email2,
                           null,
                       },
                       SelectionSet = selectionSet,
                       SplitOn = splitOn,
                   })
            {
                person2 = personEntityMapper.Map(context2);
                context2.MappedCount.Should().Be(3);

                // The same reference should have been returned
                person2.Should().BeSameAs(person1);

                // A 2nd email was added to person
                person1?.Emails.Count.Should().Be(2);
            }
        }
    }
}
