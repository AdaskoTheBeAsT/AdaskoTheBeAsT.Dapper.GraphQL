using System;
using System.Collections.Generic;
using System.Linq;
#if NET9_0_OR_GREATER
using System.Threading;
#endif
using AdaskoTheBeAsT.Dapper.GraphQL.Extensions;
using AdaskoTheBeAsT.Dapper.GraphQL.Interfaces;
using GraphQLParser.AST;

namespace AdaskoTheBeAsT.Dapper.GraphQL.Contexts
{
    public sealed class EntityMapContext : IDisposable
    {
#if NET9_0_OR_GREATER
        private readonly Lock _lockObject = new();
#endif
#if NET8_0 || NETSTANDARD2_0
        private readonly object _lockObject = new();
#endif
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

                    if (_itemEnumerator != null)
                    {
                        _itemEnumerator.Dispose();
                        _itemEnumerator = null;
                    }

                    if (_splitOnEnumerator != null)
                    {
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
        public TItemType? Next<TItemType>(
            IEnumerable<string> fieldNames,
            Func<IDictionary<GraphQLName, GraphQLField>?, IHasSelectionSetNode?, IHasSelectionSetNode?> getSelectionSet,
            IEntityMapper<TItemType>? entityMapper = null)
            where TItemType : class
        {
            ValidateNextParameters(fieldNames, getSelectionSet);

            lock (_lockObject)
            {
                EnsureEnumeratorsInitialized();

                if (!IsFieldSelected(fieldNames))
                {
                    return default;
                }

                var item = FindNextItemOfType<TItemType>();

                return entityMapper != null
                    ? MapEntityWithMapper(item, getSelectionSet, entityMapper)
                    : IncrementCountAndReturn(item);
            }
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

        private static void ValidateNextParameters(
            IEnumerable<string> fieldNames,
            Func<IDictionary<GraphQLName, GraphQLField>?, IHasSelectionSetNode?, IHasSelectionSetNode?> getSelectionSet)
        {
            if (fieldNames == null)
            {
                throw new ArgumentNullException(nameof(fieldNames));
            }

            if (getSelectionSet == null)
            {
                throw new ArgumentNullException(nameof(getSelectionSet));
            }
        }

        private void EnsureEnumeratorsInitialized()
        {
            if (_itemEnumerator == null || _splitOnEnumerator == null)
            {
                throw new NotSupportedException("Cannot call Next() before calling Start()");
            }
        }

        private bool IsFieldSelected(IEnumerable<string> fieldNames)
        {
            var selectedFieldNames = _currentSelectionSet?.Keys.Select(k => k.StringValue) ?? Enumerable.Empty<string>();
            var matchingKeys = fieldNames.Intersect(selectedFieldNames, StringComparer.OrdinalIgnoreCase);
            return matchingKeys.Any();
        }

        private TItemType? FindNextItemOfType<TItemType>()
            where TItemType : class
        {
            while (_itemEnumerator!.MoveNext() && _splitOnEnumerator!.MoveNext())
            {
                // Whether a non-null object exists at this position or not,
                // the SplitOn is expecting this type here, so we will yield it.
                if (_splitOnEnumerator.Current == typeof(TItemType))
                {
                    return _itemEnumerator.Current as TItemType;
                }
            }

            return default;
        }

        private TItemType? MapEntityWithMapper<TItemType>(
            TItemType? item,
            Func<IDictionary<GraphQLName, GraphQLField>?, IHasSelectionSetNode?, IHasSelectionSetNode?> getSelectionSet,
            IEntityMapper<TItemType> entityMapper)
            where TItemType : class
        {
#pragma warning disable CC0031 // Verify if delegate 'getSelectionSet' is null before invoking it - validated in ValidateNextParameters
            var selectionSet = getSelectionSet(_currentSelectionSet, SelectionSet);
#pragma warning restore CC0031

            var nextContext = new EntityMapContext
            {
                Items = Items!.Skip(MappedCount),
                SelectionSet = selectionSet,
                SplitOn = SplitOn!.Skip(MappedCount),
            };

            using (nextContext)
            {
                item = entityMapper.Map(nextContext);
                AdvanceEnumeratorsPastMappedItems(nextContext.MappedCount);
                MappedCount += nextContext.MappedCount;
            }

            return item;
        }

        private void AdvanceEnumeratorsPastMappedItems(int mappedCount)
        {
            // Less 1, the next time we iterate we will advance by 1 as part of the iteration.
            var advanceCount = mappedCount - 1;

            for (var i = 0; i < advanceCount; i++)
            {
                if (!_itemEnumerator!.MoveNext() || !_splitOnEnumerator!.MoveNext())
                {
                    break;
                }
            }
        }

        private TItemType? IncrementCountAndReturn<TItemType>(TItemType? item)
            where TItemType : class
        {
            MappedCount++;
            return item;
        }
    }
}
