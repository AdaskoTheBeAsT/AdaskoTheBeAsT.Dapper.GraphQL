using System.Linq;
using System.Threading.Tasks;
using AdaskoTheBeAsT.Dapper.GraphQL.Oracle.Extensions;
using AdaskoTheBeAsT.Dapper.GraphQL.Oracle.IntegrationTest.EntityMappers;
using AdaskoTheBeAsT.Dapper.GraphQL.Oracle.IntegrationTest.Models;
using AwesomeAssertions;
using Dapper;
using Xunit;
using Xunit.Sdk;

namespace AdaskoTheBeAsT.Dapper.GraphQL.Oracle.IntegrationTest
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
            var personEmailId = -1;
            var personPhoneId = -1;

            try
            {
                using (var db = _fixture.GetDbConnection())
                {
                    db.Open();

                    personId = OracleIdentity.NextIdentity<Person, int>(db, p => p.Id);
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

                    emailId = OracleIdentity.NextIdentity<Email, int>(db, e => e.Id);
                    var email = new Email
                    {
                        Id = emailId,
                        Address = Email,
                    };
                    personEmailId = global::Dapper.SqlMapper.Query<int>(db, "SELECT PERSONEMAIL_ID_SEQ.NEXTVAL FROM DUAL").Single();
                    var personEmail = new
                    {
                        Id = personEmailId,
                        PersonId = personId,
                        EmailId = emailId,
                    };

                    phoneId = OracleIdentity.NextIdentity<Phone, int>(db, p => p.Id);
                    var phone = new Phone
                    {
                        Id = phoneId,
                        Number = PhoneNumber,
                        Type = PhoneType.Mobile,
                    };
                    personPhoneId = global::Dapper.SqlMapper.Query<int>(db, "SELECT PERSONPHONE_ID_SEQ.NEXTVAL FROM DUAL").Single();
                    var personPhone = new
                    {
                        Id = personPhoneId,
                        PersonId = personId,
                        PhoneId = phoneId,
                    };

                    insertedCount = SqlBuilder
                        .Insert(email)
                        .Insert("PersonEmail", personEmail)
                        .Execute(db);

                    insertedCount += global::Dapper.SqlMapper.Execute(
                        db,
                        "INSERT INTO Phone (Id, \"Number\", \"Type\") VALUES (:p_id, :p_number, :p_type)",
                        new { p_id = phone.Id, p_number = phone.Number, p_type = (int)phone.Type });

                    insertedCount += SqlBuilder
                        .Insert("PersonPhone", personPhone)
                        .Execute(db);

                    Assert.Equal(4, insertedCount);

                    var personMapper = new PersonEntityMapper();

                    var query = SqlBuilder
                        .From<Person>(nameof(Person))
                        .LeftJoin("PersonEmail personEmail on person.Id = personEmail.Id")
                        .LeftJoin("Email email on personEmail.EmailId = email.Id")
                        .LeftJoin("PersonPhone personPhone on person.Id = personPhone.PersonId")
                        .LeftJoin("Phone phone on personPhone.PhoneId = phone.Id")
                        .Select("person.*, email.*, phone.*")
                        .SplitOn<Person>("Id")
                        .SplitOn<Email>("Id")
                        .SplitOn<Phone>("Id")
                        .Where("person.Id = :id", new { id = personId });

                    const string graphql = @"
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

                person.Should().NotBeNull();
                Assert.Equal(personId, person.Id);
                person.FirstName.Should().Be(NameSteven);
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
            var personEmailId = -1;
            var personPhoneId = -1;

            try
            {
                using (var db = _fixture.GetDbConnection())
                {
                    db.Open();

                    personId = await OracleIdentity.NextIdentityAsync<Person, int>(db, p => p.Id);
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

                    emailId = await OracleIdentity.NextIdentityAsync<Email, int>(db, e => e.Id);
                    var email = new Email
                    {
                        Id = emailId,
                        Address = "srollman@landmarkhw.com",
                    };
                    personEmailId = (await global::Dapper.SqlMapper.QueryAsync<int>(db, "SELECT PERSONEMAIL_ID_SEQ.NEXTVAL FROM DUAL")).Single();
                    var personEmail = new
                    {
                        Id = personEmailId,
                        PersonId = personId,
                        EmailId = emailId,
                    };

                    phoneId = await OracleIdentity.NextIdentityAsync<Phone, int>(db, p => p.Id);
                    var phone = new Phone
                    {
                        Id = phoneId,
                        Number = PhoneNumber,
                        Type = PhoneType.Mobile,
                    };
                    personPhoneId = (await global::Dapper.SqlMapper.QueryAsync<int>(db, "SELECT PERSONPHONE_ID_SEQ.NEXTVAL FROM DUAL")).Single();
                    var personPhone = new
                    {
                        Id = personPhoneId,
                        PersonId = personId,
                        PhoneId = phoneId,
                    };

                    insertedCount = await SqlBuilder
                        .Insert(email)
                        .Insert("PersonEmail", personEmail)
                        .ExecuteAsync(db);

                    insertedCount += await global::Dapper.SqlMapper.ExecuteAsync(
                        db,
                        "INSERT INTO Phone (Id, \"Number\", \"Type\") VALUES (:p_id, :p_number, :p_type)",
                        new { p_id = phone.Id, p_number = phone.Number, p_type = (int)phone.Type });

                    insertedCount += await SqlBuilder
                        .Insert("PersonPhone", personPhone)
                        .ExecuteAsync(db);

                    Assert.Equal(4, insertedCount);

                    var personMapper = new PersonEntityMapper();

                    var query = SqlBuilder
                        .From<Person>(nameof(Person))
                        .LeftJoin("PersonEmail personEmail on person.Id = personEmail.Id")
                        .LeftJoin("Email email on personEmail.EmailId = email.Id")
                        .LeftJoin("PersonPhone personPhone on person.Id = personPhone.PersonId")
                        .LeftJoin("Phone phone on personPhone.PhoneId = phone.Id")
                        .Select("person.*, email.*, phone.*")
                        .SplitOn<Person>("Id")
                        .SplitOn<Email>("Id")
                        .SplitOn<Phone>("Id")
                        .Where("person.Id = :id", new { id = personId });

                    const string graphql = @"
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
