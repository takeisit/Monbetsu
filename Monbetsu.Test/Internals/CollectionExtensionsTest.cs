using FluentAssertions;
using Monbetsu.Internals;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Monbetsu.Test
{
    public class CollectionExtensionsTest
    {
        [Test]
        public void TestHasIntarsection()
        {
            var setA = new HashSet<int>();
            var setB = new HashSet<int>();
            var setA012 = new HashSet<int> { 0,1,2 };
            var setA01 = new HashSet<int> { 0, 1 };
            var setA02 = new HashSet<int> { 0, 2 };
            var setA12 = new HashSet<int> { 1, 2 };
            var setA0 = new HashSet<int> { 0 };
            var setA1 = new HashSet<int> { 1 };
            var setA2 = new HashSet<int> { 2 };
            var setA3 = new HashSet<int> { 3 };


            void Test(HashSet<int> h0, HashSet<int> h1, bool expected)
            {
                h0.HasIntarsection(h1).Should().Be(expected);
                h1.HasIntarsection(h0).Should().Be(expected);
            }

            Test(setA, setB, false);
            Test(setA, setA0, false);
            Test(setA, setA012, false);

            Test(setA012, setA0, true);
            Test(setA012, setA3, false);
            Test(setA012, setA12, true);
            Test(setA012, setA012, true);

            Test(setA01, setA2, false);
            Test(setA01, setA1, true);

            Test(setA02, setA2, true);
            Test(setA02, setA3, false);
        }

        [Test]
        public void TestRemoveWhereContainsUnorderedly()
        {
            void Test(IEnumerable<int> init, IEnumerable<int> targets, IEnumerable<int> expected)
            {
                var list = init.ToList();
                list.RemoveWhereContainsUnorderedly(new HashSet<int>(targets));
                list.Should().BeEquivalentTo(expected);
            }

            Test(new[] { 0, 1, 2 }, new[] { 3 }, new[] { 0, 1, 2 });
            Test(new[] { 0, 1, 2 }, new[] { 3, 0 }, new[] { 1, 2 });
            Test(new[] { 0, 1, 2 }, new[] { 1 }, new[] { 0, 2 });
            Test(new[] { 0, 1, 2 }, new[] { 2 }, new[] { 0, 1 });
            Test(new[] { 0, 1, 2 }, new int[] { }, new[] { 1, 0, 2 });
            Test(new[] { 1, 1 }, new[] { 1 }, new int[] { });
            Test(new[] { 0, 1, 1, 2 }, new[] { 1 }, new int[] { 0, 2 });
            Test(new[] { 0, 1, 2 }, new[] { 0, 1, 2 }, new int[] { });
            Test(new[] { 0, 0, 1, 1, 2, 2 }, new[] { 0, 2 }, new int[] { 1, 1 });
        }

        class Box<T> : IEquatable<Box<T>?>
        {
            public T Value { get; }
            public Box(T value) { Value = value; }

            public static implicit operator T(Box<T> b) => b.Value;
            public static implicit operator Box<T>(T v) => new Box<T>(v);

            public static bool operator ==(Box<T>? left, Box<T>? right)
            {
                return EqualityComparer<Box<T>>.Default.Equals(left, right);
            }

            public static bool operator !=(Box<T>? left, Box<T>? right)
            {
                return !(left == right);
            }

            public override bool Equals(object? obj)
            {
                return Equals(obj as Box<T>);
            }

            public bool Equals(Box<T>? other)
            {
                return other != null &&
                       EqualityComparer<T>.Default.Equals(Value, other.Value);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Value);
            }
        }

        [Test]
        public void TestRemoveAllUnorderedly()
        {
            void Test(IEnumerable<int> init, int target, IEnumerable<int> expected)
            {
                var list = new List<Box<int>>();
                var dic = new Dictionary<int, Box<int>>();
                var t = default(Box<int>?);
                foreach(var n in init)
                {
                    if(!dic.TryGetValue(n, out var box))
                    {
                        dic[n] = box = new Box<int>(n);
                    }
                    list.Add(box);
                    if (n == target)
                    {
                        t = box;
                    }
                }

                list.RemoveAllUnorderedly(t ?? new Box<int>(-1));
                list.Select(_ => _.Value).Should().BeEquivalentTo(expected);
            }

            Test(new[] { 0, 1, 2 }, 3, new[] { 0, 1, 2 });
            Test(new[] { 0, 1, 2 }, 0, new[] { 1, 2 });
            Test(new[] { 0, 1, 2 }, 1, new[] { 0, 2 });
            Test(new[] { 0, 1, 2 }, 2, new[] { 0, 1 });
            Test(new[] { 1, 1 }, 1, new int[] { });
            Test(new[] { 0, 1, 1, 2 }, 1, new int[] { 0, 2 });
        }

        [Test]
        public void TestRemoveWhereContainsIndex()
        {
            void Test(IEnumerable<int> init, params int[] targets)
            {
                var list = init.ToList();
                var expected = list.Where((_, i) => !targets.Contains(i)).ToList();

                Monbetsu.v03.Internals.CollectionsExtensions.RemoveWhereContainsIndex(list, targets);
                list.Should().Equal(expected);
            }

            Test(Enumerable.Range(0, 5));
            Test(Enumerable.Range(0, 5), 0, 1, 2, 3, 4);
            Test(Enumerable.Range(0, 5), 0, 4);
            Test(Enumerable.Range(0, 5), 0, 1);
            Test(Enumerable.Range(0, 5), 1, 2, 3);
            Test(Enumerable.Range(0, 5), 3, 4);
            Test(Enumerable.Range(0, 5), 1, 3);

        }

        [Test]
        public void TestRemoveWhereContainsIndexUnorderedly()
        {
            void Test(IEnumerable<int> init, params int[] targets)
            {
                var list = init.ToList();
                var expected = list.Where((_, i) => !targets.Contains(i)).ToList();


                var targetSet = new IndexSet();
                foreach(var t in targets)
                {
                    targetSet.Add(t);
                }

                list.RemoveWhereContainsIndexUnorderedly(targetSet);
                list.Should().BeEquivalentTo(expected);
            }

            Test(Enumerable.Range(0, 5));
            Test(Enumerable.Range(0, 5), 0, 1, 2, 3, 4);
            Test(Enumerable.Range(0, 5), 0, 4);
            Test(Enumerable.Range(0, 5), 0, 1);
            Test(Enumerable.Range(0, 5), 1, 2, 3);
            Test(Enumerable.Range(0, 5), 3, 4);
            Test(Enumerable.Range(0, 5), 1, 3);
        }
    }
}
