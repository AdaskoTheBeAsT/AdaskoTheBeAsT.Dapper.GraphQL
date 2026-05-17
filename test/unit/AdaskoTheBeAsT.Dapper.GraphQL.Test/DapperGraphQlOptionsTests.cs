using System;
using AdaskoTheBeAsT.Dapper.GraphQL.Contexts;
using AdaskoTheBeAsT.Dapper.GraphQL.Interfaces;
using AwesomeAssertions;
using GraphQL.Types;
using GraphQLParser.AST;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AdaskoTheBeAsT.Dapper.GraphQL.Test;

public class DapperGraphQlOptionsTests
{
    [Fact(DisplayName = "AddQueryBuilder<TModel, TBuilder> registers IQueryBuilder<TModel>")]
    public void AddQueryBuilderGenericRegistersService()
    {
        var services = new ServiceCollection();
        var sut = new DapperGraphQlOptions(services);

        var result = sut.AddQueryBuilder<DummyModel, DummyQueryBuilder>();

        result.Should().BeSameAs(sut);
        using var provider = services.BuildServiceProvider();
        provider.GetService<IQueryBuilder<DummyModel>>().Should().BeOfType<DummyQueryBuilder>();
    }

    [Fact(DisplayName = "AddQueryBuilder(Type,Type) registers concrete IQueryBuilder<TModel>")]
    public void AddQueryBuilderTypeRegistersService()
    {
        var services = new ServiceCollection();
        var sut = new DapperGraphQlOptions(services);

        var result = sut.AddQueryBuilder(typeof(DummyModel), typeof(DummyQueryBuilder));

        result.Should().BeSameAs(sut);
        using var provider = services.BuildServiceProvider();
        provider.GetService<IQueryBuilder<DummyModel>>().Should().BeOfType<DummyQueryBuilder>();
    }

    [Fact(DisplayName = "AddQueryBuilder(Type,Type) throws when builder is abstract")]
    public void AddQueryBuilderTypeThrowsForAbstract()
    {
        var sut = new DapperGraphQlOptions(new ServiceCollection());

        var act = () => sut.AddQueryBuilder(typeof(DummyModel), typeof(AbstractQueryBuilder));

        act.Should().Throw<ArgumentException>();
    }

    [Fact(DisplayName = "AddQueryBuilder(Type,Type) throws when builder does not implement IQueryBuilder")]
    public void AddQueryBuilderTypeThrowsForNonImplementer()
    {
        var sut = new DapperGraphQlOptions(new ServiceCollection());

        var act = () => sut.AddQueryBuilder(typeof(DummyModel), typeof(DummyModel));

        act.Should().Throw<ArgumentException>();
    }

    [Fact(DisplayName = "AddType<TGraphType> registers type")]
    public void AddTypeGenericRegistersType()
    {
        var services = new ServiceCollection();
        var sut = new DapperGraphQlOptions(services);

        var result = sut.AddType<DummyGraphType>();

        result.Should().BeSameAs(sut);
        using var provider = services.BuildServiceProvider();
        provider.GetService<DummyGraphType>().Should().NotBeNull();
    }

    [Fact(DisplayName = "AddType(Type) registers concrete IGraphType")]
    public void AddTypeTypeRegistersType()
    {
        var services = new ServiceCollection();
        var sut = new DapperGraphQlOptions(services);

        var result = sut.AddType(typeof(DummyGraphType));

        result.Should().BeSameAs(sut);
        using var provider = services.BuildServiceProvider();
        provider.GetService(typeof(DummyGraphType)).Should().NotBeNull();
    }

    [Fact(DisplayName = "AddType(Type) throws when type is abstract")]
    public void AddTypeTypeThrowsForAbstract()
    {
        var sut = new DapperGraphQlOptions(new ServiceCollection());

        var act = () => sut.AddType(typeof(AbstractGraphType));

        act.Should().Throw<ArgumentException>();
    }

    [Fact(DisplayName = "AddType(Type) throws when type does not implement IGraphType")]
    public void AddTypeTypeThrowsForNonImplementer()
    {
        var sut = new DapperGraphQlOptions(new ServiceCollection());

        var act = () => sut.AddType(typeof(DummyModel));

        act.Should().Throw<ArgumentException>();
    }

    [Fact(DisplayName = "AddSchema<TSchema> registers schema")]
    public void AddSchemaGenericRegistersSchema()
    {
        var services = new ServiceCollection();
        var sut = new DapperGraphQlOptions(services);

        var result = sut.AddSchema<DummySchema>();

        result.Should().BeSameAs(sut);
        using var provider = services.BuildServiceProvider();
        provider.GetService<DummySchema>().Should().NotBeNull();
    }

    [Fact(DisplayName = "AddSchema(Type) registers concrete ISchema")]
    public void AddSchemaTypeRegistersSchema()
    {
        var services = new ServiceCollection();
        var sut = new DapperGraphQlOptions(services);

        var result = sut.AddSchema(typeof(DummySchema));

        result.Should().BeSameAs(sut);
        using var provider = services.BuildServiceProvider();
        provider.GetService(typeof(DummySchema)).Should().NotBeNull();
    }

    [Fact(DisplayName = "AddSchema(Type) throws when schema is abstract")]
    public void AddSchemaTypeThrowsForAbstract()
    {
        var sut = new DapperGraphQlOptions(new ServiceCollection());

        var act = () => sut.AddSchema(typeof(AbstractSchema));

        act.Should().Throw<ArgumentException>();
    }

    [Fact(DisplayName = "AddSchema(Type) throws when type does not implement ISchema")]
    public void AddSchemaTypeThrowsForNonImplementer()
    {
        var sut = new DapperGraphQlOptions(new ServiceCollection());

        var act = () => sut.AddSchema(typeof(DummyModel));

        act.Should().Throw<ArgumentException>();
    }

    public class DummyModel
    {
        public int Id { get; set; }
    }

    public class DummyQueryBuilder : IQueryBuilder<DummyModel>
    {
        public SqlQueryContext Build(
            SqlQueryContext query,
            IHasSelectionSetNode context,
            string alias) => query;
    }

    public abstract class AbstractQueryBuilder : IQueryBuilder<DummyModel>
    {
        public int Marker { get; protected set; }

        public abstract SqlQueryContext Build(
            SqlQueryContext query,
            IHasSelectionSetNode context,
            string alias);
    }

    public class DummyGraphType : ObjectGraphType<DummyModel>
    {
        public DummyGraphType()
        {
            Name = "DummyGraph";
        }
    }

    public abstract class AbstractGraphType : ObjectGraphType<DummyModel>
    {
        public int Marker { get; protected set; }
    }

    public class DummySchema : Schema
    {
        public int Marker { get; }
    }

    public abstract class AbstractSchema : Schema
    {
        public int Marker { get; protected set; }
    }
}
