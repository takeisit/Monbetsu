using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Monbetsu.Test
{
    class UnorderedNTuple<T> : IEquatable<UnorderedNTuple<T>>, IReadOnlyCollection<T>
    {
        private readonly List<(IGrouping<T, T> group, int count)> list;

        public UnorderedNTuple(params T[] list)
            : this(list.AsEnumerable())
        {
            
        }

        public UnorderedNTuple(IEnumerable<T> list)
        {
            this.list = (list ?? throw new ArgumentNullException(nameof(list)))
                .ToLookup(_ => _)
                .Select(g => (group: g, count: g.Count()))
                .OrderByDescending(t => t.count)
                .ToList()
            ;

            Count = this.list.Sum(t => t.count);
        }

        public int Count { get; }

        public bool Equals([AllowNull] UnorderedNTuple<T> other)
        {
            if (Count == other?.Count)
            {
                var r = other.list.ToDictionary(t => t.group.Key);
                return list.All(t => r.TryGetValue(t.group.Key, out var s) && t.count == s.count);
            }
            return false;
        }

        public override bool Equals(object? obj)
        {
            return obj is UnorderedNTuple<T> other ? Equals(other) : false;
        }

        private IEnumerable<T> GetEnumerable() => list.SelectMany(t => t.group);
        public IEnumerator<T> GetEnumerator() => GetEnumerable().GetEnumerator();

        public override int GetHashCode()
        {
            
            return list.Count switch
            {
                0 => 0,
                1 => HashCode.Combine(Count, list[0].count),
                2 => HashCode.Combine(Count, list[0].count, list[1].count),
                3 => HashCode.Combine(Count, list[0].count, list[1].count, list[2].count),
                4 => HashCode.Combine(Count, list[0].count, list[1].count, list[2].count, list[3].count),
                _ => HashCode.Combine(Count, list[0].count, list[1].count, list[2].count, list[3].count, list[4].count)
            };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerable().GetEnumerator();

        public static bool operator ==(UnorderedNTuple<T> left, UnorderedNTuple<T> right)
        {
            return EqualityComparer<UnorderedNTuple<T>>.Default.Equals(left, right);
        }

        public static bool operator !=(UnorderedNTuple<T> left, UnorderedNTuple<T> right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return $"Count = {Count}, {{{string.Join(", ", GetEnumerable().Take(5))}{ (Count > 5 ? ", ..." : "")}}}";
        }
    }
}
