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
#pragma warning disable MA0051 // Method is too long
        public static IEnumerable<KeyValuePair<string, object?>> GetSetFlatProperties<TType>(TType obj)
#pragma warning restore MA0051 // Method is too long
        {
            var type = obj!.GetType();
            PropertyInfo[] properties;

            lock (LockProperty)
            {
                if (!PropertyCache.TryGetValue(type, out var value))
                {
                    // Get a list of properties that are "flat" on this object, i.e. singular values
                    properties = type
                        .GetProperties()
                        .Where(p =>
                        {
                            var typeInfo = GetTypeInfo(p.PropertyType);

#if NET8_0_OR_GREATER
                            // Explicitly permit primitive, value types
                            if (typeInfo.IsPrimitive || typeInfo.IsValueType)
                            {
                                return true;
                            }

                            // Explicitly permit strings (they implement IEnumerable but should be treated as scalar values)
                            if (p.PropertyType == typeof(string))
                            {
                                return true;
                            }
#endif
#if NETSTANDARD2_0
                            // Explicitly permit primitive, value, and serializable types
                            if (typeInfo.IsSerializable || typeInfo.IsPrimitive || typeInfo.IsValueType)
                            {
                                return true;
                            }

                            // Explicitly permit strings (they implement IEnumerable but should be treated as scalar values)
                            if (p.PropertyType == typeof(string))
                            {
                                return true;
                            }
#endif

                            // Filter out list-types
                            if (typeof(IEnumerable).IsAssignableFrom(p.PropertyType))
                            {
                                return false;
                            }

                            if (p.PropertyType.IsConstructedGenericType)
                            {
                                var typeDef = p.PropertyType.GetGenericTypeDefinition();
                                if (typeof(IEnumerable<>).IsAssignableFrom(typeDef) ||
                                    typeof(ICollection<>).IsAssignableFrom(typeDef) ||
                                    typeof(IList<>).IsAssignableFrom(typeDef))
                                {
                                    return false;
                                }
                            }

                            return true;
                        })
                        .ToArray();

                    // Cache those properties
                    PropertyCache[type] = properties;
                }
                else
                {
                    properties = value;
                }
            }

            // Convert the properties to a dictionary where:
            // Key   = property name
            // Value = property value, or null if the property is set to its default value
            return properties
                .ToDictionary(
                    prop => prop.Name,
                    prop =>
                    {
                        // Ensure scalar values are properly skipped if they are set to their initial, default(type) value.
                        var value = prop.GetValue(obj);
                        if (value == null)
                        {
                            return value;
                        }

                        var valueType = value.GetType();
                        var valueTypeInfo = GetTypeInfo(valueType);
                        if (valueTypeInfo.IsValueType &&
                            Equals(value, Activator.CreateInstance(valueType)))
                        {
                            return null;
                        }

                        return value;
                    },
                    StringComparer.OrdinalIgnoreCase)

                // Then, filter out "unset" properties, or properties that are set to their default value
                .Where(kvp => kvp.Value != null);
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
