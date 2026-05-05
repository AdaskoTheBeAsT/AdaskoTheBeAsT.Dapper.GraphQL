using System.Linq;
using System.Threading.Tasks;
using AdaskoTheBeAsT.Dapper.GraphQL.SqlServer.Extensions;
using AdaskoTheBeAsT.Dapper.GraphQL.SqlServer.IntegrationTest.EntityMappers;
using AdaskoTheBeAsT.Dapper.GraphQL.SqlServer.IntegrationTest.Models;
using Xunit;
using Xunit.Sdk;

namespace AdaskoTheBeAsT.Dapper.GraphQL.SqlServer.IntegrationTest
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
#pragma warning disable MA0051
        public void InsertPerson()
#pragma warning restore MA0051
        {
            Person? person = null;

            var emailId = -1;
            var personId = -1;
            var phoneId = -1;

            try
            {
                using (var db = _fixture.GetDbConnection())
                {
                    db.Open();

                    person = new Person
                    {
                        FirstName = NameSteven,
                        LastName = NameRollman,
                    };

                    personId = SqlBuilder
                        .Insert(person)
                        .ExecuteWithSqlServerIdentity<int>(db);
                    Assert.True(personId > 0);

                    SqlBuilder
                        .Update(nameof(Person), new { MergedToPersonId = personId })
                        .Where("Id = @id", new { id = personId })
                        .Execute(db);

                    var email = new Email
                    {
                        Address = Email,
                    };
                    emailId = SqlBuilder
                        .Insert(email)
                        .ExecuteWithSqlServerIdentity<int>(db);

                    var personEmail = new
                    {
                        PersonId = personId,
                        EmailId = emailId,
                    };

                    var phone = new Phone
                    {
                        Number = PhoneNumber,
                        Type = PhoneType.Mobile,
                    };
                    phoneId = SqlBuilder
                        .Insert(phone)
                        .ExecuteWithSqlServerIdentity<int>(db);

                    var personPhone = new
                    {
                        PersonId = personId,
                        PhoneId = phoneId,
                    };

                    var insertedCount = SqlBuilder
                        .Insert("PersonEmail", personEmail)
                        .Insert("PersonPhone", personPhone)
                        .Execute(db);

                    Assert.Equal(2, insertedCount);

                    var personMapper = new PersonEntityMapper();

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
#pragma warning disable MA0051
        public async Task InsertPersonAsync()
#pragma warning restore MA0051
        {
            Person? person = null;

            var emailId = -1;
            var personId = -1;
            var phoneId = -1;

            try
            {
                using (var db = _fixture.GetDbConnection())
                {
                    db.Open();

                    person = new Person
                    {
                        FirstName = NameSteven,
                        LastName = NameRollman,
                    };

                    personId = await SqlBuilder
                        .Insert(person)
                        .ExecuteWithSqlServerIdentityAsync<int>(db);
                    Assert.True(personId > 0);

                    await SqlBuilder
                        .Update(nameof(Person), new { MergedToPersonId = personId })
                        .Where("Id = @id", new { id = personId })
                        .ExecuteAsync(db);

                    var email = new Email
                    {
                        Address = "srollman@landmarkhw.com",
                    };
                    emailId = await SqlBuilder
                        .Insert(email)
                        .ExecuteWithSqlServerIdentityAsync<int>(db);

                    var personEmail = new
                    {
                        PersonId = personId,
                        EmailId = emailId,
                    };

                    var phone = new Phone
                    {
                        Number = PhoneNumber,
                        Type = PhoneType.Mobile,
                    };
                    phoneId = await SqlBuilder
                        .Insert(phone)
                        .ExecuteWithSqlServerIdentityAsync<int>(db);

                    var personPhone = new
                    {
                        PersonId = personId,
                        PhoneId = phoneId,
                    };

                    var insertedCount = await SqlBuilder
                        .Insert("PersonEmail", personEmail)
                        .Insert("PersonPhone", personPhone)
                        .ExecuteAsync(db);

                    Assert.Equal(2, insertedCount);

                    var personMapper = new PersonEntityMapper();

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
