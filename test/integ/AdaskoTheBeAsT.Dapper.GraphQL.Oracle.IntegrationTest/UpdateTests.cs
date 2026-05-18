using System.Linq;
using System.Threading.Tasks;
using AdaskoTheBeAsT.Dapper.GraphQL.Oracle.IntegrationTest.Models;
using AwesomeAssertions;
using Xunit;
using Xunit.Sdk;

namespace AdaskoTheBeAsT.Dapper.GraphQL.Oracle.IntegrationTest;

public class UpdateTests : IClassFixture<TestFixture>
{
    private static readonly string[] IdFirstNameColumns = ["Id", "FirstName"];

    private readonly TestFixture _fixture;

    public UpdateTests(TestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact(DisplayName = "UPDATE person succeeds")]
#pragma warning disable MA0051 // Method is too long
    public void UpdatePerson()
#pragma warning restore MA0051 // Method is too long
    {
        var person = new Person
        {
            FirstName = "Douglas",
        };

        Person? previousPerson = null;

        try
        {
            const string graphql = @"
{
    person {
        id
        firstName
    }
}";

            var selectionSet = TestFixture.BuildGraphQlSelection(graphql);
            if (selectionSet == null)
            {
                throw new XunitException("Selection set is null");
            }

            // Update the person with Id = 2 with a new FirstName
            using (var db = _fixture.GetDbConnection())
            {
                previousPerson = SqlBuilder
                    .From<Person>()
                    .Select(IdFirstNameColumns)
                    .Where("FirstName = :firstName", new { firstName = "Doug" })
                    .Execute<Person>(db, selectionSet)
                    .FirstOrDefault();

                SqlBuilder
                    .Update(person)
                    .Where("Id = :id", new { id = previousPerson?.Id ?? 0 })
                    .Execute(db);

                // Get the same person back
                person = SqlBuilder
                    .From<Person>()
                    .Select(IdFirstNameColumns)
                    .Where("Id = :id", new { id = previousPerson?.Id ?? 0 })
                    .Execute<Person>(db, selectionSet)
                    .FirstOrDefault();
            }

            // Ensure we got a person and their name was indeed changed
            person.Should().NotBeNull();
            person.FirstName.Should().Be("Douglas");
        }
        finally
        {
            if (previousPerson != null)
            {
                using (var db = _fixture.GetDbConnection())
                {
                    person = new Person
                    {
                        FirstName = previousPerson.FirstName,
                    };

                    // Put the entity back to the way it was
                    SqlBuilder
                        .Update<Person>(person)
                        .Where("Id = :id", new { id = 2 })
                        .Execute(db);
                }
            }
        }
    }

    [Fact(DisplayName = "UPDATE person asynchronously succeeds")]
#pragma warning disable MA0051 // Method is too long
    public async Task UpdatePersonAsync()
#pragma warning restore MA0051 // Method is too long
    {
        var person = new Person
        {
            FirstName = "Douglas",
        };

        Person? previousPerson = null;

        try
        {
            // Update the person with Id = 2 with a new FirstName
            using (var db = _fixture.GetDbConnection())
            {
                db.Open();

                const string graphql = @"
{
    person {
        id
        firstName
    }
}";

                var selectionSet = TestFixture.BuildGraphQlSelection(graphql);
                if (selectionSet == null)
                {
                    throw new XunitException("Selection set is null");
                }

                var previousPeople = await SqlBuilder
                    .From<Person>()
                    .Select(IdFirstNameColumns)
                    .Where("FirstName = :firstName", new { firstName = "Doug" })
                    .ExecuteAsync<Person>(db, selectionSet);

                previousPerson = previousPeople.FirstOrDefault();

                await SqlBuilder
                    .Update(person)
                    .Where("Id = :id", new { id = previousPerson?.Id ?? 0 })
                    .ExecuteAsync(db);

                // Get the same person back
                var people = await SqlBuilder
                    .From<Person>()
                    .Select(IdFirstNameColumns)
                    .Where("Id = :id", new { id = previousPerson?.Id ?? 0 })
                    .ExecuteAsync<Person>(db, selectionSet);
                person = people
                    .FirstOrDefault();
            }

            // Ensure we got a person and their name was indeed changed
            person.Should().NotBeNull();
            person.FirstName.Should().Be("Douglas");
        }
        finally
        {
            if (previousPerson != null)
            {
                using (var db = _fixture.GetDbConnection())
                {
                    db.Open();

                    person = new Person
                    {
                        FirstName = previousPerson.FirstName,
                    };

                    // Put the entity back to the way it was
                    await SqlBuilder
                        .Update<Person>(person)
                        .Where("Id = :id", new { id = 2 })
                        .ExecuteAsync(db);
                }
            }
        }
    }
}
