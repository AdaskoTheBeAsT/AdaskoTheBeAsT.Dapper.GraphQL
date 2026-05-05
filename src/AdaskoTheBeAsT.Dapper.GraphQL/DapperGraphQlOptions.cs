using System;
using AdaskoTheBeAsT.Dapper.GraphQL.Interfaces;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;

namespace AdaskoTheBeAsT.Dapper.GraphQL
{
    public class DapperGraphQlOptions(IServiceCollection serviceCollection)
    {
        public DapperGraphQlOptions AddQueryBuilder<TModelType, TQueryBuilder>()
            where TQueryBuilder : class, IQueryBuilder<TModelType>
        {
            serviceCollection.AddSingleton<IQueryBuilder<TModelType>, TQueryBuilder>();
            return this;
        }

        public DapperGraphQlOptions AddQueryBuilder(Type modelType, Type queryBuilderType)
        {
            var queryBuilderInterface = typeof(IQueryBuilder<>).MakeGenericType(modelType);
            if (queryBuilderType.IsAbstract || queryBuilderType.IsInterface || !queryBuilderInterface.IsAssignableFrom(queryBuilderType))
            {
                throw new ArgumentException(
                    $"QueryBuilder type must be concrete and implement IQueryBuilder<{modelType.Name}>.",
                    nameof(queryBuilderType));
            }

            serviceCollection.Add(new ServiceDescriptor(queryBuilderInterface, queryBuilderType, ServiceLifetime.Singleton));
            return this;
        }

        public DapperGraphQlOptions AddSchema<TGraphSchema>()
            where TGraphSchema : class, ISchema
        {
            serviceCollection.AddSingleton<TGraphSchema>();
            return this;
        }

        public DapperGraphQlOptions AddSchema(Type graphSchemaType)
        {
            if (graphSchemaType.IsAbstract || graphSchemaType.IsInterface || !typeof(ISchema).IsAssignableFrom(graphSchemaType))
            {
                throw new ArgumentException("Type must be concrete and implement ISchema.", nameof(graphSchemaType));
            }

            serviceCollection.Add(new ServiceDescriptor(graphSchemaType, graphSchemaType, ServiceLifetime.Singleton));
            return this;
        }

        public DapperGraphQlOptions AddType<TGraphType>()
            where TGraphType : class, IGraphType
        {
            serviceCollection.AddSingleton<TGraphType>();
            return this;
        }

        public DapperGraphQlOptions AddType(Type type)
        {
            if (type.IsAbstract || type.IsInterface || !typeof(IGraphType).IsAssignableFrom(type))
            {
                throw new ArgumentException("Type must be concrete and implement IGraphType.", nameof(type));
            }

            serviceCollection.Add(new ServiceDescriptor(type, type, ServiceLifetime.Singleton));
            return this;
        }
    }
}
