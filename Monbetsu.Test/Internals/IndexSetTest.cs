using FluentAssertions;
using Monbetsu.Internals;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Monbetsu.Test
{
    public class IndexSetTest
    {
        [Test]
        public void TestIndexSet()
        {
            var set = new IndexSet(70);
            var set2 = new IndexSet();
            var example = new SortedSet<int>();

            void AssertEquavalent()
            {
                set.Count.Should().Be(example.Count);
                set.Should().Equal(example);
                set2.Count.Should().Be(example.Count);
                set2.Should().Equal(example);

                for (var i = 0; i < 70; i++)
                {
                    set.Contains(i).Should().Be(example.Contains(i));
                    set2.Contains(i).Should().Be(example.Contains(i));
                }
            }

            AssertEquavalent();


            void Set(int index, bool value)
            {
                if (value)
                {
                    set.Add(index).Should().Be(example.Add(index));
                    set2.Add(index);
                }
                else
                {
                    set.Remove(index).Should().Be(example.Remove(index));
                    set2.Remove(index);
                }
                
                AssertEquavalent();
            }

            void Clear()
            {
                set.Clear();
                set2.Clear();
                example.Clear();

                AssertEquavalent();
            }


            AssertEquavalent();


            Set(0, true);
            Set(0, true);
            Set(0, false);
            Set(1, true);
            Set(12, true);
            Set(31, true);
            Set(31, false);
            Set(32, true);
            Set(32, false);
            Set(50, true);
            Set(64, true);
            Set(64, false);

            Set(65, false);
            Set(66, true);
            Set(69, true);
            Set(69, false);


            Clear();

            Set(69, true);
            Set(68, true);

            Set(3, true);
            Set(4, true);

            for(var i = 0; i < 70; i++)
            {
                Set(i, true);
            }
        }

    }
}
