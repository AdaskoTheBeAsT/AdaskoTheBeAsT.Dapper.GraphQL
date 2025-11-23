using System.Linq;
using System.Threading.Tasks;
using AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest.EntityMappers;
using AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest.Models;
using Xunit;
using Xunit.Sdk;

namespace AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest
{
    public class InsertTests : IClassFixture<TestFixture>
    {
        public const string NameSteven = "Steven";
        public const string NameRollman = "Rollman";
        public const string PhoneNumber = "8011115555";
        public const string Email = "srollman@landmarkhw.com";
        private readonly TestFixture _fixture;

        public InsertTests(TestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact(DisplayName = "INSERT person succeeds")]
#pragma warning disable MA0051 // Method is too long
        public void InsertPerson()
#pragma warning restore MA0051 // Method is too long
        {
            Person? person = null;

            // Ensure inserting a person works and we get IDs back
            var emailId = -1;
            var personId = -1;
            var phoneId = -1;

            try
            {
                using (var db = _fixture.GetDbConnection())
                {
                    db.Open();

                    // Get the next identity aggressively, as we need to assign
                    // it to both Id/MergedToPersonId
                    personId = Extensions.PostgreSql.NextIdentity(db, (Person p) => p.Id);
                    Assert.True(personId > 0);

                    person = new Person
                    {
                        Id = personId,
                        FirstName = NameSteven,
                        LastName = NameRollman,
                        MergedToPersonId = personId,
                    };

                    int insertedCount;
                    insertedCount = SqlBuilder
                        .Insert(person)
                        .Execute(db);
                    Assert.Equal(1, insertedCount);

                    emailId = Extensions.PostgreSql.NextIdentity(db, (Email e) => e.Id);
                    var email = new Email
                    {
                        Id = emailId,
                        Address = Email,
                    };

                    var personEmail = new
                    {
                        PersonId = personId,
                        EmailId = emailId,
                    };

                    phoneId = Extensions.PostgreSql.NextIdentity(db, (Phone p) => p.Id);
                    var phone = new Phone
                    {
                        Id = phoneId,
                        Number = PhoneNumber,
                        Type = PhoneType.Mobile,
                    };

                    var personPhone = new
                    {
                        PersonId = personId,
                        PhoneId = phoneId,
                    };

                    // Add email and phone number to the person
                    insertedCount = SqlBuilder
                        .Insert(email)
                        .Insert(phone)
                        .Insert("PersonEmail", personEmail)
                        .Insert("PersonPhone", personPhone)
                        .Execute(db);

                    // Ensure all were inserted properly
                    Assert.Equal(4, insertedCount);

                    // Build an entity mapper for person
                    var personMapper = new PersonEntityMapper();

                    // Query the person from the database
                    var query = SqlBuilder
                        .From<Person>(nameof(person))
                        .LeftJoin("PersonEmail personEmail on person.Id = personEmail.Id")
                        .LeftJoin("Email email on personEmail.EmailId = email.Id")
                        .LeftJoin("PersonPhone personPhone on person.Id = personPhone.PersonId")
                        .LeftJoin("Phone phone on personPhone.PhoneId = phone.Id")
                        .Select("person.*, email.*, phone.*")
                        .SplitOn<Person>("Id")
                        .SplitOn<Email>("Id")
                        .SplitOn<Phone>("Id")
                        .Where("person.Id = @id", new { id = personId });

                    var graphql = @"
{
    person {
        firstName
        lastName
        emails {
            id
            address
        }
        phones {
            id
            number
        }
    }
}";
                    var selection = _fixture.BuildGraphQlSelection(graphql);
                    if (selection == null)
                    {
                        throw new XunitException("Selection is null");
                    }

                    person = query
                        .Execute(db, selection, personMapper)
                        .Single();
                }

                // Ensure all inserted data is present
                Assert.NotNull(person);
                Assert.Equal(personId, person.Id);
                Assert.Equal(NameSteven, person.FirstName);
                Assert.Equal(NameRollman, person.LastName);
                Assert.Single(person.Emails);
                Assert.Equal(Email, person.Emails[0].Address);
                Assert.Single(person.Phones);
                Assert.Equal(PhoneNumber, person.Phones[0].Number);
            }
            finally
            {
                // Ensure the changes here don't affect other unit tests
                using (var db = _fixture.GetDbConnection())
                {
                    if (emailId != default(int))
                    {
                        SqlBuilder
                            .Delete("PersonEmail", new { EmailId = emailId })
                            .Delete(nameof(Email), new { Id = emailId })
                            .Execute(db);
                    }

                    if (phoneId != default(int))
                    {
                        SqlBuilder
                            .Delete("PersonPhone", new { PhoneId = phoneId })
                            .Delete(nameof(Phone), new { Id = phoneId })
                            .Execute(db);
                    }

                    if (personId != default(int))
                    {
                        SqlBuilder
                            .Delete<Person>(new { Id = personId })
                            .Execute(db);
                    }
                }
            }
        }

        [Fact(DisplayName = "INSERT person asynchronously succeeds")]
#pragma warning disable MA0051 // Method is too long
        public async Task InsertPersonAsync()
#pragma warning restore MA0051 // Method is too long
        {
            Person? person = null;

            // Ensure inserting a person works and we get IDs back
            var emailId = -1;
            var personId = -1;
            var phoneId = -1;

            try
            {
                using (var db = _fixture.GetDbConnection())
                {
                    db.Open();

                    // Get the next identity aggressively, as we need to assign
                    // it to both Id/MergedToPersonId
                    personId = await Extensions.PostgreSql.NextIdentityAsync(db, (Person p) => p.Id);
                    Assert.True(personId > 0);

                    person = new Person
                    {
                        Id = personId,
                        FirstName = NameSteven,
                        LastName = NameRollman,
                        MergedToPersonId = personId,
                    };

                    int insertedCount;
                    insertedCount = await SqlBuilder
                        .Insert(person)
                        .ExecuteAsync(db);
                    Assert.Equal(1, insertedCount);

                    emailId = await Extensions.PostgreSql.NextIdentityAsync(db, (Email e) => e.Id);
                    var email = new Email
                    {
                        Id = emailId,
                        Address = "srollman@landmarkhw.com",
                    };

                    var personEmail = new
                    {
                        PersonId = personId,
                        EmailId = emailId,
                    };

                    phoneId = await Extensions.PostgreSql.NextIdentityAsync(db, (Phone p) => p.Id);
                    var phone = new Phone
                    {
                        Id = phoneId,
                        Number = PhoneNumber,
                        Type = PhoneType.Mobile,
                    };

                    var personPhone = new
                    {
                        PersonId = personId,
                        PhoneId = phoneId,
                    };

                    // Add email and phone number to the person
                    insertedCount = await SqlBuilder
                        .Insert(email)
                        .Insert(phone)
                        .Insert("PersonEmail", personEmail)
                        .Insert("PersonPhone", personPhone)
                        .ExecuteAsync(db);

                    // Ensure all were inserted properly
                    Assert.Equal(4, insertedCount);

                    // Build an entity mapper for person
                    var personMapper = new PersonEntityMapper();

                    // Query the person from the database
                    var query = SqlBuilder
                        .From<Person>(nameof(person))
                        .LeftJoin("PersonEmail personEmail on person.Id = personEmail.Id")
                        .LeftJoin("Email email on personEmail.EmailId = email.Id")
                        .LeftJoin("PersonPhone personPhone on person.Id = personPhone.PersonId")
                        .LeftJoin("Phone phone on personPhone.PhoneId = phone.Id")
                        .Select("person.*, email.*, phone.*")
                        .SplitOn<Person>("Id")
                        .SplitOn<Email>("Id")
                        .SplitOn<Phone>("Id")
                        .Where("person.Id = @id", new { id = personId });

                    var graphql = @"
{
    person {
        firstName
        lastName
        emails {
            id
            address
        }
        phones {
            id
            number
        }
    }
}";
                    var selection = _fixture.BuildGraphQlSelection(graphql);
                    if (selection == null)
                    {
                        throw new XunitException("Selection is null");
                    }

                    var people = await query.ExecuteAsync(db, selection, personMapper);
                    person = people
                        .FirstOrDefault();
                }

                // Ensure all inserted data is present
                Assert.NotNull(person);
                Assert.Equal(personId, person.Id);
                Assert.Equal(NameSteven, person.FirstName);
                Assert.Equal(NameRollman, person.LastName);
                Assert.Single(person.Emails);
                Assert.Equal(Email, person.Emails[0].Address);
                Assert.Single(person.Phones);
                Assert.Equal(PhoneNumber, person.Phones[0].Number);
            }
            finally
            {
                // Ensure the changes here don't affect other unit tests
                using (var db = _fixture.GetDbConnection())
                {
                    if (emailId != default(int))
                    {
                        await SqlBuilder
                            .Delete("PersonEmail", new { EmailId = emailId })
                            .Delete(nameof(Email), new { Id = emailId })
                            .ExecuteAsync(db);
                    }

                    if (phoneId != default(int))
                    {
                        await SqlBuilder
                            .Delete("PersonPhone", new { PhoneId = phoneId })
                            .Delete(nameof(Phone), new { Id = phoneId })
                            .ExecuteAsync(db);
                    }

                    if (personId != default(int))
                    {
                        await SqlBuilder
                            .Delete<Person>(new { Id = personId })
                            .ExecuteAsync(db);
                    }
                }
            }
        }
    }
}
