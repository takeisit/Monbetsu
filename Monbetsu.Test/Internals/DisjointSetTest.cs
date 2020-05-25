using FluentAssertions;
using Monbetsu.Internals;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Monbetsu.Test
{
    public class DisjointSetTest
    {
        [Test]
        public void TestDisjointSet()
        {
            var set = new DisjointSet(10, 10);

            set.Unite(0, 1).Should().BeTrue();
            set.Unite(2, 3).Should().BeTrue();
            set.Unite(4, 5).Should().BeTrue();

            set.Unite(8, 9).Should().BeTrue();

            set.IsUnited(0, 1).Should().BeTrue();
            set.IsUnited(0, 2).Should().BeFalse();
            set.IsUnited(6, 7).Should().BeFalse();
            set.IsUnited(8, 9).Should().BeTrue();

            var result1 = set.ToLookup().Select(g => g.ToArray()).ToArray();

            result1[0].Should().Equal(new[] { 0, 1 });
            result1[1].Should().Equal(new[] { 2, 3 });
            result1[2].Should().Equal(new[] { 4, 5 });
            result1[3].Should().Equal(new[] { 6 });
            result1[4].Should().Equal(new[] { 7 });
            result1[5].Should().Equal(new[] { 8, 9 });



            set.Unite(2, 1).Should().BeTrue();

            set.IsUnited(0, 1).Should().BeTrue();
            set.IsUnited(0, 2).Should().BeTrue();
            set.IsUnited(6, 7).Should().BeFalse();
            set.IsUnited(8, 9).Should().BeTrue();

            var result2 = set.ToLookup().Select(g => g.ToArray()).ToArray();

            result2[0].Should().Equal(new[] { 0, 1, 2, 3 });
            result2[1].Should().Equal(new[] { 4, 5 });
            result2[2].Should().Equal(new[] { 6 });
            result2[3].Should().Equal(new[] { 7 });
            result2[4].Should().Equal(new[] { 8, 9 });

            set.Unite(4, 9).Should().BeTrue();

            var result3 = set.ToLookup().Select(g => g.ToArray()).ToArray();

            result3[0].Should().Equal(new[] { 0, 1, 2, 3 });
            result3[1].Should().Equal(new[] { 4, 5, 8, 9 });
            result3[2].Should().Equal(new[] { 6 });
            result3[3].Should().Equal(new[] { 7 });


            set.Unite(5, 8).Should().BeFalse();
            set.Unite(0, 0).Should().BeFalse();

            var result4 = set.ToLookup().Select(g => g.ToArray()).ToArray();

            result4[0].Should().Equal(new[] { 0, 1, 2, 3 });
            result4[1].Should().Equal(new[] { 4, 5, 8, 9 });
            result4[2].Should().Equal(new[] { 6 });
            result4[3].Should().Equal(new[] { 7 });

            set.Unite(4, 6);
            set.Unite(2, 8);
            var result5 = set.ToLookup().Select(g => g.ToArray()).ToArray();

            result5[0].Should().Equal(new[] { 0, 1, 2, 3, 4, 5, 6, 8, 9 });
            result5[1].Should().Equal(new[] { 7 });
        }

        [Test]
        public void TestDisjointSet2()
        {
            var set = new DisjointSet(4, 10);
            var result = set.Groups(1).ToArray();
            result.Length.Should().Be(4);
            result[0].Should().Equal(0);
            result[1].Should().Equal(1);
            result[2].Should().Equal(2);
            result[3].Should().Equal(3);

            set.Groups(2).Should().BeEmpty();

            set.Unite(0, 1);
            set.Unite(2, 3);
            set.Unite(2, 0);

            result = set.Groups(1).ToArray();
            result.Length.Should().Be(1);
            result[0].Should().Equal(0, 1, 2, 3);

            result = set.Groups(4).ToArray();
            result.Length.Should().Be(1);
            result[0].Should().Equal(0, 1, 2, 3);

            result = set.Groups(4).ToArray();
            result.Length.Should().Be(1);
            result[0].Should().Equal(0, 1, 2, 3);

            result = set.Groups(5).ToArray();
            result.Should().BeEmpty();
            
            set.Reset(2);

            result = set.Groups(1).ToArray();
            result.Length.Should().Be(2);
            result[0].Should().Equal(0);
            result[1].Should().Equal(1);

            set.Reset(5);
            result = set.Groups(1).ToArray();
            result.Length.Should().Be(5);
            result[0].Should().Equal(0);
            result[1].Should().Equal(1);
            result[2].Should().Equal(2);
            result[3].Should().Equal(3);
            result[4].Should().Equal(4);
        }

    }
}
