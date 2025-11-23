using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
#if NET9_0_OR_GREATER
using System.Threading;
#endif

namespace AdaskoTheBeAsT.Dapper.GraphQL
{
    public static class ParameterHelper
    {
#if NET9_0_OR_GREATER
        private static readonly Lock LockProperty = new();
        private static readonly Lock LockType = new();
#endif
#if NET8_0 || NETSTANDARD2_0
        private static readonly object LockProperty = new();
        private static readonly object LockType = new();
#endif

        private static readonly Dictionary<Type, PropertyInfo[]> PropertyCache = [];
        private static readonly Dictionary<Type, TypeInfo> TypeInfoCache = [];

        /// <summary>
        /// Gets a list of flat properties that have been set on the object.
        /// </summary>
        /// <typeparam name="TType">The type to get properties from.</typeparam>
        /// <param name="obj">The object to get properties from.</param>
        /// <returns>A list of key-value pairs of property names and values.</returns>
        public static IEnumerable<KeyValuePair<string, object?>> GetSetFlatProperties<TType>(TType obj)
        {
            var type = obj!.GetType();
            var properties = GetCachedFlatProperties(type);

            return properties
                .ToDictionary(
                    prop => prop.Name,
                    prop => GetPropertyValueOrDefault(prop, obj),
                    StringComparer.OrdinalIgnoreCase)
                .Where(kvp => kvp.Value != null);
        }

        private static PropertyInfo[] GetCachedFlatProperties(Type type)
        {
            lock (LockProperty)
            {
                if (!PropertyCache.TryGetValue(type, out var cachedProperties))
                {
                    cachedProperties = type
                        .GetProperties()
                        .Where(IsPropertyFlat)
                        .ToArray();

                    PropertyCache[type] = cachedProperties;
                }

                return cachedProperties;
            }
        }

        private static bool IsPropertyFlat(PropertyInfo property)
        {
            var typeInfo = GetTypeInfo(property.PropertyType);

            if (IsPrimitiveOrValueTypeOrString(property.PropertyType, typeInfo))
            {
                return true;
            }

            return !IsExcludedEnumerableType(property.PropertyType);
        }

        private static bool IsPrimitiveOrValueTypeOrString(Type propertyType, TypeInfo typeInfo)
        {
#if NET8_0_OR_GREATER
            // Explicitly permit primitive, value types, and strings
            if (typeInfo.IsPrimitive || typeInfo.IsValueType || propertyType == typeof(string))
            {
                return true;
            }
#endif
#if NETSTANDARD2_0
            // Explicitly permit primitive, value, serializable types, and strings
            if (typeInfo.IsSerializable || typeInfo.IsPrimitive || typeInfo.IsValueType || propertyType == typeof(string))
            {
                return true;
            }
#endif

            return false;
        }

        private static bool IsExcludedEnumerableType(Type propertyType)
        {
            // Filter out IEnumerable types (but not strings, which are handled above)
            if (typeof(IEnumerable).IsAssignableFrom(propertyType))
            {
                return true;
            }

            // Filter out generic collection types
            if (propertyType.IsConstructedGenericType)
            {
                var typeDef = propertyType.GetGenericTypeDefinition();
                if (typeof(IEnumerable<>).IsAssignableFrom(typeDef) ||
                    typeof(ICollection<>).IsAssignableFrom(typeDef) ||
                    typeof(IList<>).IsAssignableFrom(typeDef))
                {
                    return true;
                }
            }

            return false;
        }

        private static object? GetPropertyValueOrDefault<TType>(PropertyInfo property, TType obj)
        {
            var value = property.GetValue(obj);
            if (value == null)
            {
                return null;
            }

            if (IsValueSetToDefault(value))
            {
                return null;
            }

            return value;
        }

        private static bool IsValueSetToDefault(object value)
        {
            var valueType = value.GetType();
            var valueTypeInfo = GetTypeInfo(valueType);

            return valueTypeInfo.IsValueType &&
                   Equals(value, Activator.CreateInstance(valueType));
        }

        private static TypeInfo GetTypeInfo(Type type)
        {
            TypeInfo typeInfo;
            lock (LockType)
            {
                if (!TypeInfoCache.TryGetValue(type, out var value))
                {
                    typeInfo = type.GetTypeInfo();
                    TypeInfoCache[type] = typeInfo;
                }
                else
                {
                    typeInfo = value;
                }
            }

            return typeInfo;
        }
    }
}
