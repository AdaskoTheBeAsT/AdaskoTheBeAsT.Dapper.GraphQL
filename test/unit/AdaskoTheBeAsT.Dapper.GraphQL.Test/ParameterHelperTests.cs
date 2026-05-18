using System;
using System.Collections.Generic;
using System.Linq;
using AwesomeAssertions;
using Xunit;

namespace AdaskoTheBeAsT.Dapper.GraphQL.Test;

public class ParameterHelperTests
{
    [Fact(DisplayName = "GetSetFlatProperties returns set string and primitive properties")]
    public void ReturnsSetStringAndPrimitiveProperties()
    {
        var input = new FlatEntity { Id = 7, Name = "Bob", Active = true };

        var result = ParameterHelper.GetSetFlatProperties(input).ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.Ordinal);

        result.Should().ContainKey(nameof(FlatEntity.Id)).WhoseValue.Should().Be(7);
        result.Should().ContainKey(nameof(FlatEntity.Name)).WhoseValue.Should().Be("Bob");
        result.Should().ContainKey(nameof(FlatEntity.Active)).WhoseValue.Should().Be(true);
    }

    [Fact(DisplayName = "GetSetFlatProperties skips default value types")]
    public void SkipsDefaultValueTypes()
    {
        var input = new FlatEntity { Id = 0, Name = "John", Active = false };

        var result = ParameterHelper.GetSetFlatProperties(input).ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.Ordinal);

        result.Should().NotContainKey(nameof(FlatEntity.Id));
        result.Should().NotContainKey(nameof(FlatEntity.Active));
        result.Should().ContainKey(nameof(FlatEntity.Name));
    }

    [Fact(DisplayName = "GetSetFlatProperties skips null reference type properties")]
    public void SkipsNullProperties()
    {
        var input = new FlatEntity { Id = 1, Name = null };

        var result = ParameterHelper.GetSetFlatProperties(input).ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.Ordinal);

        result.Should().NotContainKey(nameof(FlatEntity.Name));
        result.Should().ContainKey(nameof(FlatEntity.Id));
    }

    [Fact(DisplayName = "GetSetFlatProperties excludes generic collection properties")]
    public void ExcludesGenericCollections()
    {
        var input = new EntityWithCollections
        {
            Id = 1,
            Tags = new List<string> { "a", "b" },
            Map = new Dictionary<string, int>(StringComparer.Ordinal) { ["x"] = 1 },
        };

        var result = ParameterHelper.GetSetFlatProperties(input).ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.Ordinal);

        result.Should().NotContainKey(nameof(EntityWithCollections.Tags));
        result.Should().NotContainKey(nameof(EntityWithCollections.Map));
        result.Should().ContainKey(nameof(EntityWithCollections.Id));
    }

    [Fact(DisplayName = "GetSetFlatProperties excludes non-generic IEnumerable properties (arrays)")]
    public void ExcludesArrays()
    {
        var input = new EntityWithArray { Id = 1, Values = new[] { 1, 2, 3 } };

        var result = ParameterHelper.GetSetFlatProperties(input).ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.Ordinal);

        result.Should().NotContainKey(nameof(EntityWithArray.Values));
        result.Should().ContainKey(nameof(EntityWithArray.Id));
    }

    [Fact(DisplayName = "GetSetFlatProperties caches reflection metadata across invocations")]
    public void CachesReflectionMetadata()
    {
        var first = ParameterHelper.GetSetFlatProperties(new FlatEntity { Id = 1 }).ToList();
        var second = ParameterHelper.GetSetFlatProperties(new FlatEntity { Id = 2 }).ToList();

        first.Should().ContainSingle(kv => kv.Key == nameof(FlatEntity.Id) && (int)kv.Value! == 1);
        second.Should().ContainSingle(kv => kv.Key == nameof(FlatEntity.Id) && (int)kv.Value! == 2);
    }

    public class FlatEntity
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public bool Active { get; set; }
    }

    public class EntityWithCollections
    {
        public int Id { get; set; }

        public IList<string>? Tags { get; set; }

        public IDictionary<string, int>? Map { get; set; }
    }

    public class EntityWithArray
    {
        public int Id { get; set; }

        public int[]? Values { get; set; }
    }
}
