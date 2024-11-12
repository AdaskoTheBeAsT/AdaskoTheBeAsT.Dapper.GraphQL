using System;
using System.Collections.Generic;
using System.Linq;
using AdaskoTheBeAsT.Dapper.GraphQL.Extensions;
using AdaskoTheBeAsT.Dapper.GraphQL.Interfaces;
using GraphQLParser.AST;

namespace AdaskoTheBeAsT.Dapper.GraphQL.Contexts
{
    public sealed class EntityMapContext : IDisposable
    {
        private readonly object _lockObject = new();
        private bool _isDisposing;
        private IDictionary<GraphQLName, GraphQLField>? _currentSelectionSet;
        private IEnumerator<object?>? _itemEnumerator;
        private IEnumerator<Type>? _splitOnEnumerator;

        /// <summary>
        /// A list of objects to be mapped.
        /// </summary>
        public IEnumerable<object?>? Items { get; set; }

        /// <summary>
        /// The count of objects that have been mapped.
        /// </summary>
        public int MappedCount { get; private set; } = 0;

        /// <summary>
        /// The GraphQL selection criteria.
        /// </summary>
        public IHasSelectionSetNode? SelectionSet { get; set; }

        /// <summary>
        /// The types used to split the GraphQL query.
        /// </summary>
        public IEnumerable<Type>? SplitOn { get; set; }

        public void Dispose()
        {
            lock (_lockObject)
            {
                if (!_isDisposing)
                {
                    _isDisposing = true;

                    if (_itemEnumerator != null &&
                        _splitOnEnumerator != null)
                    {
                        _itemEnumerator.Dispose();
                        _itemEnumerator = null;
                        _splitOnEnumerator.Dispose();
                        _splitOnEnumerator = null;
                    }
                }
            }
        }

        /// <summary>
        /// Returns a map of selected GraphQL fields.
        /// </summary>
        public IDictionary<GraphQLName, GraphQLField>? GetSelectedFields()
        {
            return SelectionSet.GetSelectedFields();
        }

        /// <summary>
        /// Maps the next object from Dapper.
        /// </summary>
        /// <param name="fieldNames">The names of one or more GraphQL fields associated with the item.</param>
        /// <param name="getSelectionSet">Gets information if node is selected.</param>
        /// <param name="entityMapper">An optional entity mapper.  This is used to map complex objects from Dapper mapping results.</param>
        /// <typeparam name="TItemType">The item type to be mapped.</typeparam>
        /// <returns>The mapped item.</returns>
#pragma warning disable MA0051 // Method is too long
        public TItemType? Next<TItemType>(
            IEnumerable<string> fieldNames,
            Func<IDictionary<GraphQLName, GraphQLField>?, IHasSelectionSetNode?, IHasSelectionSetNode?> getSelectionSet,
            IEntityMapper<TItemType>? entityMapper = null)
            where TItemType : class
#pragma warning restore MA0051 // Method is too long
        {
            if (fieldNames == null)
            {
                throw new ArgumentNullException(nameof(fieldNames));
            }

            if (getSelectionSet == null)
            {
                throw new ArgumentNullException(nameof(getSelectionSet));
            }

            lock (_lockObject)
            {
                if (_itemEnumerator == null ||
                _splitOnEnumerator == null)
                {
                    throw new NotSupportedException("Cannot call Next() before calling Start()");
                }

                var keys = fieldNames.Intersect(
                    _currentSelectionSet?.Keys.Select(k => k.StringValue) ?? Enumerable.Empty<string>(),
                    StringComparer.OrdinalIgnoreCase);
                if (keys.Any())
                {
                    var item = default(TItemType);
                    while (
                        _itemEnumerator.MoveNext() &&
                        _splitOnEnumerator.MoveNext())
                    {
                        // Whether a non-null object exists at this position or not,
                        // the SplitOn is expecting this type here, so we will yield it.
                        if (_splitOnEnumerator.Current == typeof(TItemType))
                        {
                            item = _itemEnumerator.Current as TItemType;
                            break;
                        }
                    }

                    if (entityMapper != null)
                    {
                        // Determine where the next entity mapper will get its selection set from
                        var selectionSet = getSelectionSet(_currentSelectionSet, SelectionSet);

                        var nextContext = new EntityMapContext
                        {
                            Items = Items.Skip(MappedCount),
                            SelectionSet = selectionSet,
                            SplitOn = SplitOn.Skip(MappedCount),
                        };
                        using (nextContext)
                        {
                            item = entityMapper.Map(nextContext);

                            // Update enumerators to skip past items already mapped
                            var mappedCount = nextContext.MappedCount;
                            MappedCount += nextContext.MappedCount;
                            var i = 0;
                            while (

                                // Less 1, the next time we iterate we
                                // will advance by 1 as part of the iteration.
                                i < mappedCount - 1 &&
                                _itemEnumerator.MoveNext() &&
                                _splitOnEnumerator.MoveNext())
                            {
                                i++;
                            }
                        }
                    }
                    else
                    {
                        MappedCount++;
                    }

                    return item;
                }
            }

            return default;
        }

        /// <summary>
        /// Begins mapping objects from Dapper.
        /// </summary>
        /// <typeparam name="TEntityType">The entity type to be mapped.</typeparam>
        /// <returns>The mapped entity.</returns>
        public TEntityType? Start<TEntityType>()
            where TEntityType : class
        {
            lock (_lockObject)
            {
                _itemEnumerator?.Dispose();
                _itemEnumerator = Items?.GetEnumerator();
                _splitOnEnumerator?.Dispose();
                _splitOnEnumerator = SplitOn?.GetEnumerator();
                _currentSelectionSet = SelectionSet.GetSelectedFields();
                MappedCount = 0;

                if ((_itemEnumerator?.MoveNext() ?? false) &&
                    (_splitOnEnumerator?.MoveNext() ?? false))
                {
                    var entity = _itemEnumerator.Current as TEntityType;
                    MappedCount++;
                    return entity;
                }

                return default;
            }
        }
    }
}
