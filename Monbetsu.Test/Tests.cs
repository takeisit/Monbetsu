using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Monbetsu.Test.Utils;

namespace Monbetsu.Test
{
    public partial class Tests : Verifier
    {


        [Test]
        public void Test001()
        {
            AssertGrouping((from: 0, to: 1, group: -1));
        }

        [Test]
        public void Test002()
        {
            AssertGrouping(
                (from: 0, to: 1, group: -1),
                (from: 1, to: 2, group: -1),
                (from: 0, to: 2, group: -2, kind: Ser(-1))
                );
        }

        [Test]
        public void Test002_1()
        {
            AssertGrouping(
                (from: 0, to: 1, group: -1),
                (from: 1, to: 2, group: -1),
                (from: 2, to: 3, group: -1),
                (from: 0, to: 3, group: -2, kind: Ser(-1))
                );
        }

        [Test]
        public void Test002_2()
        {
            AssertGrouping(
                (from: 0, to: 1, group: -1),
                (from: 1, to: 2, group: -1),
                (from: 2, to: 3, group: -1),
                (from: 3, to: 4, group: -1),
                (from: 0, to: 4, group: -2, kind: Ser(-1))
                );
        }

        [Test]
        public void Test003()
        {
            AssertGrouping(
                (from: 0, to: 1, group: -1),
                (from: 0, to: 1, group: -2),
                (from: 0, to: 1, group: -3, kind: Par(-1, -2))
                );
        }

        [Test]
        public void Test004()
        {
            AssertGrouping(
                (from: 0, to: 1, group: -1),
                (from: 0, to: 1, group: -2),
                (from: 1, to: 2, group: -3),
                (from: 0, to: 1, group: -3, kind: Par(-1, -2)),
                (from: 0, to: 2, group: -4, kind: Ser(-3))
                );
        }

        [Test]
        public void Test005()
        {
            AssertGrouping(
                (from: 0, to: 1, group: -1),
                (from: 1, to: 2, group: -2),
                (from: 1, to: 2, group: -3),
                (from: 2, to: 3, group: -1),
                (from: 1, to: 2, group: -1, kind: Par(-2, -3)),
                (from: 0, to: 3, group: -4, kind: Ser(-1))
                );
        }

        [Test]
        public void Test006()
        {
            AssertGrouping(
                (from: 0, to: 1, group: -1),
                (from: 1, to: 2, group: -1),
                (from: 0, to: 3, group: -2),
                (from: 3, to: 2, group: -2),
                (from: 0, to: 2, group: -3, kind: Ser(-1)),
                (from: 0, to: 2, group: -4, kind: Ser(-2)),
                (from: 0, to: 2, group: -5, kind: Par(-3, -4))
                );
        }

        [Test]
        public void Test007()
        {
            AssertGrouping(
                (from: 0, to: 1, group: -1),
                (from: 1, to: 2, group: -1),
                (from: 0, to: 3, group: -2),
                (from: 3, to: 2, group: -3),
                (from: 3, to: 4, group: -4),
                (from: 0, to: 2, group: -5, kind: Ser(-1))
                );
        }

        [Test]
        public void Test008()
        {
            AssertGrouping(
                (from: 8, to: 9, group: -5),
                (from: 9, to: 10, group: -3),
                (from: 10, to: 11, group: -1),
                (from: 10, to: 11, group: -2),
                (from: 11, to: 12, group: -3),
                (from: 9, to: 12, group: -4),
                (from: 12, to: 13, group: -5),
                (from: 8, to: 13, group: -6),
                (from: 10, to: 11, group: -3, kind: Par(-1, -2)),
                (from: 9, to: 12, group: -7, kind: Ser(-3)),
                (from: 9, to: 12, group: -5, kind: Par(-4, -7)),
                (from: 8, to: 13, group: -8, kind: Ser(-5)),
                (from: 8, to: 13, group: -9, kind: Par(-6, -8))
                );
        }

        [Test]
        public void Test009()
        {
            AssertGrouping(
                (from: 0, to: 1, group: -1),
                (from: 0, to: 1, group: -2),
                (from: 1, to: 2, group: -3),
                (from: 0, to: 2, group: -4),
                (from: 2, to: 3, group: -5),
                (from: 0, to: 3, group: -6),
                (from: 3, to: 4, group: -7),
                (from: 0, to: 4, group: -8),
                (from: 0, to: 1, group: -3, kind: Par(-1, -2)),
                (from: 0, to: 2, group: -9, kind: Ser(-3)),
                (from: 0, to: 2, group: -5, kind: Par(-4, -9)),
                (from: 0, to: 3, group: -10, kind: Ser(-5)),
                (from: 0, to: 3, group: -7, kind: Par(-10, -6)),
                (from: 0, to: 4, group: -11, kind: Ser(-7)),
                (from: 0, to: 4, group: -12, kind: Par(-8, -11))
                );
        }

        [Test]
        public void Test010()
        {
            AssertGrouping(
                (from: 0, to: 1, group: -1),
                (from: 1, to: 2, group: -2),
                (from: 1, to: 2, group: -3),
                (from: 0, to: 2, group: -4),
                (from: 2, to: 3, group: -5),
                (from: 0, to: 3, group: -6),
                (from: 1, to: 2, group: -1, kind: Par(-2, -3)),
                (from: 0, to: 2, group: -7, kind: Ser(-1)),
                (from: 0, to: 2, group: -5, kind: Par(-4, -7)),
                (from: 0, to: 3, group: -8, kind: Ser(-5)),
                (from: 0, to: 3, group: -9, kind: Par(-6, -8))
                );
        }

        [Test]
        public void Test011()
        {
            AssertGrouping(
                (from: 0, to: 1, group: -1),
                (from: 0, to: 4, group: -2),
                (from: 1, to: 2, group: -3),
                (from: 1, to: 4, group: -4),
                (from: 2, to: 3, group: -5),
                (from: 2, to: 4, group: -6),
                (from: 3, to: 4, group: -7),
                (from: 3, to: 4, group: -8),
                (from: 3, to: 4, group: -5, kind: Par(-7, -8)),
                (from: 2, to: 4, group: -9, kind: Ser(-5)),
                (from: 2, to: 4, group: -3, kind: Par(-6, -9)),
                (from: 1, to: 4, group: -10, kind: Ser(-3)),
                (from: 1, to: 4, group: -1, kind: Par(-4, -10)),
                (from: 0, to: 4, group: -11, kind: Ser(-1)),
                (from: 0, to: 4, group: -12, kind: Par(-11, -2))
                );
        }

        [Test]
        public void Test012()
        {
            AssertGrouping(
                (from: 0, to: 1, group: -1),
                (from: 1, to: 2, group: -2),
                (from: 1, to: 2, group: -3),
                (from: 2, to: 3, group: -1),
                (from: 0, to: 3, group: -4),
                (from: 1, to: 2, group: -1, kind: Par(-2, -3)),
                (from: 0, to: 3, group: -7, kind: Ser(-1)),
                (from: 0, to: 3, group: -5, kind: Par(-4, -7))
                );
        }

        [Test]
        public void Test013()
        {
            AssertGrouping(
                (from: 0, to: 1, group: -1),
                (from: 1, to: 2, group: -2),
                (from: 0, to: 3, group: -3),
                (from: 3, to: 2, group: -4),
                (from: 1, to: 3, group: -5),
                (from: 0, to: 2, group: -6, kind: Knot(-1, -2, -3, -4, -5))
                );
        }

        [Test]
        public void Test014()
        {
            AssertGrouping(
                (from: 0, to: 1, group: -1),
                (from: 1, to: 2, group: -1),
                (from: 3, to: 2, group: -2),
                (from: 0, to: 2, group: -3, kind: Ser(-1))
                );
        }

        [Test]
        public void Test015()
        {
            AssertGrouping(
                (from: 0, to: 1, group: -1),
                (from: 1, to: 2, group: -2),
                (from: 1, to: 3, group: -3),
                (from: 3, to: 2, group: -3),
                (from: 2, to: 4, group: -4),
                (from: 1, to: 5, group: -5),
                (from: 5, to: 2, group: -6),
                (from: 6, to: 5, group: -7),
                (from: 5, to: 7, group: -8),
                (from: 1, to: 2, group: -10, kind: Par(-2, -9)),
                (from: 1, to: 2, group: -9, kind: Ser(-3))
                );
        }

        [Test]
        public void Test016()
        {
            AssertGrouping(
                (from: 0, to: 1, group: -1),
                (from: 1, to: 2, group: -2),
                (from: 1, to: 3, group: -3),
                (from: 3, to: 2, group: -3),
                (from: 2, to: 4, group: -4),
                (from: 1, to: 5, group: -5),
                (from: 5, to: 2, group: -6),
                (from: 6, to: 5, group: -7),
                (from: 1, to: 2, group: -10, kind: Par(-2, -9)),
                (from: 1, to: 2, group: -9, kind: Ser(-3))
                );
        }

        [Test]
        public void Test017()
        {
            AssertGrouping(
                (from: 0, to: 1, group: -1),
                (from: 1, to: 2, group: -2),
                (from: 1, to: 3, group: -3),
                (from: 3, to: 2, group: -4),
                (from: 3, to: 4, group: -5),
                (from: 2, to: 5, group: -6)
                );

            AssertGrouping(
                (from: 0, to: 1, group: -1),
                (from: 1, to: 3, group: -3),
                (from: 3, to: 2, group: -4),
                (from: 3, to: 4, group: -5),
                (from: 1, to: 2, group: -2),
                (from: 2, to: 5, group: -6)
                );
        }

        [Test]
        public void Test018()
        {
            AssertGrouping(
                (from: 0, to: 1, group: -1),
                (from: 1, to: 2, group: -2),
                (from: 2, to: 3, group: -2),
                (from: 1, to: 4, group: -3),
                (from: 1, to: 3, group: -4, kind: Ser(-2))
                );

            AssertGrouping(
                (from: 0, to: 1, group: -1),
                (from: 1, to: 4, group: -3),
                (from: 1, to: 2, group: -2),
                (from: 2, to: 3, group: -2),
                (from: 1, to: 3, group: -4, kind: Ser(-2))
                );
        }

        [Test]
        public void Test019()
        {
            AssertGrouping(
                (from: 0, to: 2, group: -1),
                (from: 1, to: 2, group: -2),
                (from: 2, to: 3, group: -3)
                );

        }

        [Test]
        public void Test020()
        {
            AssertGrouping(
                (from: 0, to: 1, group: -1),
                (from: 1, to: 2, group: -2),
                (from: 2, to: 4, group: -4),
                (from: 1, to: 5, group: -5),
                (from: 5, to: 2, group: -6),
                (from: 6, to: 5, group: -7)
                );
        }

        [Test]
        public void Test021()
        {
            AssertGrouping(
                (from: 1, to: 2, group: -1),
                (from: 2, to: 4, group: -2),
                (from: 1, to: 5, group: -3),
                (from: 5, to: 2, group: -4),
                (from: 6, to: 5, group: -5)
                );
        }

        [Test]
        public void Test022()
        {
            AssertGrouping(
                (from: 1, to: 2, group: -1),
                (from: 1, to: 2, group: -2),
                (from: 3, to: 4, group: -3),
                (from: 3, to: 4, group: -4),
                (from: 1, to: 2, group: -5, kind: Par(-1, -2)),
                (from: 3, to: 4, group: -6, kind: Par(-3, -4))
                );
        }

        [Test]
        public void Test023()
        {
            AssertGrouping(
                (from: 0, to: 1, group: -1),
                (from: 0, to: 3, group: -2),
                (from: 1, to: 2, group: -3),
                (from: 1, to: 2, group: -4),
                (from: 3, to: 4, group: -5),
                (from: 3, to: 4, group: -6),
                (from: 2, to: 5, group: -1),
                (from: 4, to: 5, group: -2),
                (from: 1, to: 2, group: -1, kind: Par(-3, -4)),
                (from: 3, to: 4, group: -2, kind: Par(-5, -6)),
                (from: 0, to: 5, group: -7, kind: Ser(-1)),
                (from: 0, to: 5, group: -8, kind: Ser(-2)),
                (from: 0, to: 5, group: -9, kind: Par(-7, -8))
                );
        }

        [Test]
        public void Test024()
        {
            AssertGrouping(
                (from: 0, to: 1, group: -1),
                (from: 0, to: 3, group: -2),
                (from: 1, to: 2, group: -3),
                (from: 1, to: 4, group: -4),
                (from: 3, to: 1, group: -5),
                (from: 3, to: 4, group: -6),
                (from: 2, to: 5, group: -3),
                (from: 4, to: 5, group: -8),
                (from: 1, to: 5, group: -9, kind: Ser(-3)),
                (from: 0, to: 5, group: -10, kind: Knot(-1, -2, -4, -5, -6, -8, -9))
                );
        }

        [Test]
        public void Test025()
        {
            AssertGrouping(
                (from: 0, to: 1, group: -1),
                (from: 0, to: 2, group: -2),
                (from: 1, to: 3, group: -3),
                (from: 2, to: 3, group: -2),
                (from: 1, to: 4, group: -5),
                (from: 3, to: 5, group: -6),
                (from: 4, to: 5, group: -5),
                (from: 0, to: 3, group: -7, kind: Ser(-2)),
                (from: 1, to: 5, group: -8, kind: Ser(-5)),
                (from: 0, to: 5, group: -9, kind: Knot(-1, -3, -6, -7, -8))
                );
        }

        [Test]
        public void Test026()
        {
            AssertGrouping(
                (from: 0, to: 1, group: -1),
                (from: 0, to: 2, group: -2),
                (from: 1, to: 2, group: -3),
                (from: 1, to: 3, group: -4),
                (from: 2, to: 4, group: -5),
                (from: 3, to: 4, group: -6),
                (from: 3, to: 5, group: -7)
                );
        }

        [Test]
        public void Test027()
        {
            AssertGrouping(
                (from: 0, to: 1, group: -1),
                (from: 1, to: 2, group: -2),
                (from: 1, to: 3, group: -3),
                (from: 2, to: 4, group: -2),
                (from: 3, to: 4, group: -3),
                (from: 0, to: 4, group: -4),
                (from: 1, to: 4, group: -5, kind: Ser(-2)),
                (from: 1, to: 4, group: -6, kind: Ser(-3)),
                (from: 1, to: 4, group: -1, kind: Par(-5, -6)),
                (from: 0, to: 4, group: -7, kind: Ser(-1)),
                (from: 0, to: 4, group: -8, kind: Par(-4, -7))
                );
        }


        [Test]
        public void Test028()
        {
            AssertGrouping(
                (from: 0, to: 1, group: -1),
                (from: 1, to: 2, group: -2),
                (from: 1, to: 2, group: -3),
                (from: 0, to: 3, group: -4),
                (from: 3, to: 2, group: -5),
                (from: 3, to: 2, group: -6),
                (from: 3, to: 2, group: -4, kind: Par(-5, -6)),
                (from: 0, to: 2, group: -7, kind: Ser(-4)),
                (from: 1, to: 2, group: -1, kind: Par(-3, -2)),
                (from: 0, to: 2, group: -8, kind: Ser(-1)),
                (from: 0, to: 2, group: -9, kind: Par(-7, -8))
                );
        }

        [Test]
        public void Test029()
        {
            AssertGrouping(
                (from: 0, to: 1, group: -1),
                (from: 0, to: 2, group: -2),
                (from: 2, to: 3, group: -3),
                (from: 2, to: 4, group: -4),
                (from: 3, to: 5, group: -5),
                (from: 4, to: 5, group: -6),
                (from: 1, to: 3, group: -7),
                (from: 1, to: 3, group: -8),
                (from: 4, to: 6, group: -9),
                (from: 4, to: 6, group: -10),
                (from: 1, to: 3, group: -1, kind: Par(-7, -8)),
                (from: 0, to: 3, group: -11, kind: Ser(-1)),
                (from: 4, to: 6, group: -12, kind: Par(-9, -10))
                );
        }

        [Test]
        public void Test030()
        {
            AssertGrouping(
                (from: 0, to: 1, group: -1),
                (from: 0, to: 1, group: -2),
                (from: 0, to: 2, group: -3),
                (from: 0, to: 2, group: -4),
                (from: 0, to: 1, group: -5, kind: Par(-1, -2)),
                (from: 0, to: 2, group: -6, kind: Par(-3, -4))
                );
        }

        [Test]
        public void Test031()
        {
            AssertGrouping(
                (from: 0, to: 1, group: -1),
                (from: 0, to: 1, group: -2),
                (from: 0, to: 2, group: -3),
                (from: 0, to: 2, group: -4),
                (from: 1, to: 3, group: -5),
                (from: 2, to: 3, group: -6),
                (from: 0, to: 1, group: -5, kind: Par(-1, -2)),
                (from: 0, to: 2, group: -6, kind: Par(-3, -4)),
                (from: 0, to: 3, group: -7, kind: Ser(-5)),
                (from: 0, to: 3, group: -8, kind: Ser(-6)),
                (from: 0, to: 3, group: -9, kind: Par(-7, -8))
                );
        }

        [Test]
        public void Test032()
        {
            AssertGrouping(
                (from: 0, to: 1, group: -1),
                (from: 1, to: 2, group: -2),
                (from: 1, to: 3, group: -3),
                (from: 2, to: 3, group: -4),
                (from: 2, to: 4, group: -5),
                (from: 3, to: 4, group: -6),
                (from: 4, to: 5, group: -1),
                (from: 0, to: 5, group: -7),
                (from: 1, to: 4, group: -1, kind: Knot(-2, -3, -4, -5, -6)),
                (from: 0, to: 5, group: -8, kind: Ser(-1)),
                (from: 0, to: 5, group: -9, kind: Par(-8, -7))
                );
        }

        [Test]
        public void Test033()
        {
            AssertGrouping(
                (from: 0, to: 1, group: -1),
                (from: 0, to: 2, group: -2),
                (from: 1, to: 3, group: -3),
                (from: 2, to: 3, group: -4),
                (from: 1, to: 2, group: -5),

                (from: 0, to: 4, group: -6),
                (from: 0, to: 5, group: -7),
                (from: 4, to: 3, group: -8),
                (from: 5, to: 3, group: -9),
                (from: 4, to: 5, group: -10),


                (from: 0, to: 3, group: -11, kind: Knot(-1, -2, -3, -4, -5)),
                (from: 0, to: 3, group: -12, kind: Knot(-6, -7, -8, -9, -10)),
                (from: 0, to: 3, group: -13, kind: Par(-11, -12))
                );
        }

        [Test]
        public void Test034()
        {
            AssertGrouping(
                (from: 0, to: 1, group: -1),
                (from: 0, to: 2, group: -2),
                (from: 1, to: 3, group: -3),
                (from: 2, to: 3, group: -4),
                (from: 1, to: 2, group: -5),

                (from: 0, to: 4, group: -6),
                (from: 0, to: 5, group: -7),
                (from: 4, to: 3, group: -8),
                (from: 5, to: 3, group: -9),
                (from: 4, to: 5, group: -10),

                (from: 0, to: 3, group: -14),

                (from: 0, to: 3, group: -11, kind: Knot(-1, -2, -3, -4, -5)),
                (from: 0, to: 3, group: -12, kind: Knot(-6, -7, -8, -9, -10)),
                (from: 0, to: 3, group: -13, kind: Par(-11, -12, -14))
                );
        }

        [Test]
        public void Test035()
        {
            AssertGrouping(
                (from: 0, to: 1, group: -1),
                (from: 0, to: 2, group: -2),
                (from: 1, to: 3, group: -3),
                (from: 2, to: 3, group: -4),
                (from: 1, to: 2, group: -5),

                (from: 4, to: 3, group: -6),
                
                (from: 0, to: 3, group: -11, kind: Knot(-1, -2, -3, -4, -5))
                
                );
        }

        [Test]
        public void Test036()
        {
            AssertGrouping(
                (from: 0, to: 1, group: -1),
                (from: 0, to: 2, group: -2),
                (from: 1, to: 3, group: -3),
                (from: 2, to: 3, group: -4),
                (from: 1, to: 2, group: -5),

                (from: 0, to: 4, group: -6),

                (from: 0, to: 3, group: -11, kind: Knot(-1, -2, -3, -4, -5))

                );
        }

        [Test]
        public void Test036a()
        {
            AssertGrouping(
                (from: 0, to: 1, group: -1),
                (from: 0, to: 2, group: -2),
                (from: 1, to: 3, group: -3),
                (from: 2, to: 3, group: -4),
                (from: 1, to: 2, group: -5),

                (from: 0, to: 4, group: -6),
                (from: 5, to: 3, group: -7),

                (from: 0, to: 3, group: -11, kind: Knot(-1, -2, -3, -4, -5))

                );
        }

        [Test]
        public void Test036b()
        {
            AssertGrouping(

                (from: 0, to: 6, group: -8),
                (from: 7, to: 3, group: -9),

                (from: 0, to: 1, group: -1),
                (from: 0, to: 2, group: -2),
                (from: 1, to: 3, group: -3),
                (from: 2, to: 3, group: -4),
                (from: 1, to: 2, group: -5),

                (from: 0, to: 4, group: -6),
                (from: 5, to: 3, group: -7),

                (from: 0, to: 3, group: -11, kind: Knot(-1, -2, -3, -4, -5))

                );
        }

        [Test]
        public void Test037()
        {
            AssertGrouping(
                (from: 0, to: 1, group: -1),
                (from: 0, to: 2, group: -2),
                (from: 1, to: 3, group: -3),
                (from: 2, to: 3, group: -4),
                (from: 1, to: 2, group: -5),

                (from: 4, to: 1, group: -6),

                Sentinel()
                );
        }

        [Test]
        public void Test038()
        {
            AssertGrouping(
                (from: 0, to: 1, group: -1),
                (from: 0, to: 2, group: -2),
                (from: 1, to: 3, group: -3),
                (from: 2, to: 3, group: -4),
                (from: 1, to: 2, group: -5),

                (from: 2, to: 4, group: -6),

                Sentinel()
                );
        }

        [Test]
        public void Test039()
        {
            AssertGrouping(
                (from: 6, to: 0, group: -13),

                (from: 0, to: 1, group: -1),
                (from: 0, to: 2, group: -2),
                (from: 1, to: 3, group: -3),
                (from: 2, to: 3, group: -4),
                (from: 1, to: 2, group: -5),

                (from: 0, to: 4, group: -6),
                (from: 0, to: 5, group: -7),
                (from: 4, to: 3, group: -8),
                (from: 5, to: 3, group: -9),
                (from: 4, to: 5, group: -10),


                (from: 0, to: 3, group: -11, kind: Knot(-1, -2, -3, -4, -5)),
                (from: 0, to: 3, group: -12, kind: Knot(-6, -7, -8, -9, -10)),
                (from: 0, to: 3, group: -13, kind: Par(-11, -12)),

                (from: 6, to: 3, group: -14, kind: Ser(-13)),

                Sentinel()
                );
        }

        [Test]
        public void Test039a()
        {
            AssertGrouping(
                
                (from: 0, to: 1, group: -1),
                (from: 0, to: 2, group: -2),
                (from: 1, to: 3, group: -3),
                (from: 2, to: 3, group: -4),
                (from: 1, to: 2, group: -5),

                (from: 0, to: 4, group: -6),
                (from: 0, to: 5, group: -7),
                (from: 4, to: 3, group: -8),
                (from: 5, to: 3, group: -9),
                (from: 4, to: 5, group: -10),

                (from: 3, to: 6, group: -13),


                (from: 0, to: 3, group: -11, kind: Knot(-1, -2, -3, -4, -5)),
                (from: 0, to: 3, group: -12, kind: Knot(-6, -7, -8, -9, -10)),
                (from: 0, to: 3, group: -13, kind: Par(-11, -12)),

                (from: 0, to: 6, group: -14, kind: Ser(-13)),

                Sentinel()
                );
        }

        [Test]
        public void Test039b()
        {
            AssertGrouping(
                (from: 7, to: 0, group: -13),

                (from: 0, to: 1, group: -1),
                (from: 0, to: 2, group: -2),
                (from: 1, to: 3, group: -3),
                (from: 2, to: 3, group: -4),
                (from: 1, to: 2, group: -5),

                (from: 0, to: 4, group: -6),
                (from: 0, to: 5, group: -7),
                (from: 4, to: 3, group: -8),
                (from: 5, to: 3, group: -9),
                (from: 4, to: 5, group: -10),

                (from: 3, to: 6, group: -13),


                (from: 0, to: 3, group: -11, kind: Knot(-1, -2, -3, -4, -5)),
                (from: 0, to: 3, group: -12, kind: Knot(-6, -7, -8, -9, -10)),
                (from: 0, to: 3, group: -13, kind: Par(-11, -12)),

                (from: 7, to: 6, group: -14, kind: Ser(-13)),

                Sentinel()
                );
        }

        [Test]
        public void Test040()
        {
            AssertGrouping(
                
                (from: 0, to: 1, group: -1),
                (from: 0, to: 2, group: -2),
                (from: 1, to: 3, group: -3),
                (from: 2, to: 3, group: -4),
                (from: 1, to: 2, group: -5),

                (from: 6, to: 4, group: -6),
                (from: 6, to: 5, group: -7),
                (from: 4, to: 3, group: -8),
                (from: 5, to: 3, group: -9),
                (from: 4, to: 5, group: -10),


                (from: 0, to: 3, group: -11, kind: Knot(-1, -2, -3, -4, -5)),
                (from: 6, to: 3, group: -12, kind: Knot(-6, -7, -8, -9, -10)),
                
                Sentinel()
                );
        }

        [Test]
        public void Test041()
        {
            AssertGrouping(

                (from: 0, to: 1, group: -1),
                (from: 0, to: 2, group: -2),
                (from: 1, to: 3, group: -3),
                (from: 2, to: 3, group: -4),
                
                (from: 1, to: 6, group: -5),
                (from: 2, to: 6, group: -6),

                (from: 3, to: 4, group: -7),
                (from: 3, to: 5, group: -8),
                (from: 4, to: 6, group: -9),
                (from: 5, to: 6, group: -10),
                (from: 4, to: 5, group: -11),

                (from: 3, to: 6, group: -12, kind: Knot(-7, -8, -9, -10, -11)),
                (from: 0, to: 6, group: -13, kind: Knot(-1, -2, -3, -4, -5, -6, -12)),

                Sentinel()
                );
        }

        [Test]
        public void Test042()
        {
            AssertGrouping(

                (from: 10, to: 20, group: -1020),
                (from: 10, to: 21, group: -1021),
                (from: 10, to: 22, group: -1022),

                (from: 11, to: 20, group: -1120),
                (from: 11, to: 21, group: -1121),
                (from: 11, to: 22, group: -1122),

                (from: 12, to: 20, group: -1220),
                (from: 12, to: 21, group: -1221),
                (from: 12, to: 22, group: -1222),

                (from: 20, to: 30, group: -2030),
                (from: 20, to: 31, group: -2031),
                (from: 20, to: 32, group: -2032),

                (from: 21, to: 30, group: -2130),
                (from: 21, to: 31, group: -2131),
                (from: 21, to: 32, group: -2132),

                (from: 22, to: 30, group: -2230),
                (from: 22, to: 31, group: -2231),
                (from: 22, to: 32, group: -2232),

                (from: 30, to: 40, group: -3040),
                (from: 30, to: 41, group: -3041),
                (from: 30, to: 42, group: -3042),

                (from: 31, to: 40, group: -3140),
                (from: 31, to: 41, group: -3141),
                (from: 31, to: 42, group: -3142),

                (from: 32, to: 40, group: -3240),
                (from: 32, to: 41, group: -3241),
                (from: 32, to: 42, group: -3242),

                Sentinel()
                );
        }

        [Test]
        public void Test043()
        {
            AssertGrouping(

                (from: 0, to: 1, group: -1),
                (from: 0, to: 2, group: -2),
                (from: 1, to: 3, group: -3),
                (from: 1, to: 4, group: -4),
                (from: 2, to: 4, group: -5),
                (from: 2, to: 5, group: -6),
                (from: 3, to: 6, group: -3),
                (from: 4, to: 6, group: -8),
                (from: 4, to: 7, group: -9),
                (from: 5, to: 7, group: -6),
                (from: 6, to: 8, group: -11),
                (from: 7, to: 8, group: -12),

                (from: 1, to: 6, group: -7, kind: Ser(-3)),
                (from: 2, to: 7, group: -10, kind: Ser(-6)),
                (from: 0, to: 8, group: -13, kind: Knot(-1, -2, -4, -5, -7, -8, -9, -10, -11, -12)),

                Sentinel()
                );
        }

        [Test]
        public void Test044()
        {
            AssertGrouping(

                (from: 0, to: 6, group: -10),

                (from: 0, to: 1, group: -1),
                (from: 0, to: 2, group: -2),
                (from: 1, to: 2, group: -3),
                (from: 1, to: 3, group: -4),
                (from: 2, to: 3, group: -5),

                (from: 0, to: 7, group: -11),

                (from: 0, to: 3, group: -6, kind: Knot(-1, -2, -3, -4, -5)),

                Sentinel()
                );

            AssertGrouping(
                (from: 0, to: 1, group: -1),
                (from: 0, to: 2, group: -2),
                (from: 1, to: 2, group: -3),
                (from: 1, to: 3, group: -4),
                (from: 2, to: 3, group: -5),

                (from: 1, to: 7, group: -11),

                Sentinel()
                );

            AssertGrouping(
                (from: 1, to: 7, group: -11),

                (from: 0, to: 1, group: -1),
                (from: 0, to: 2, group: -2),
                (from: 1, to: 2, group: -3),
                (from: 1, to: 3, group: -4),
                (from: 2, to: 3, group: -5),

                Sentinel()
                );

            AssertGrouping(
                (from: 7, to: 1, group: -11),

                (from: 0, to: 1, group: -1),
                (from: 0, to: 2, group: -2),
                (from: 1, to: 2, group: -3),
                (from: 1, to: 3, group: -4),
                (from: 2, to: 3, group: -5),

                Sentinel()
                );

            AssertGrouping(
                (from: 0, to: 1, group: -1),
                (from: 0, to: 2, group: -2),
                (from: 1, to: 2, group: -3),
                (from: 1, to: 3, group: -4),
                (from: 2, to: 3, group: -5),

                (from: 7, to: 1, group: -11),

                Sentinel()
                );

            AssertGrouping(
                (from: 0, to: 1, group: -1),
                (from: 0, to: 2, group: -2),
                (from: 1, to: 2, group: -3),
                (from: 1, to: 3, group: -4),
                (from: 2, to: 3, group: -5),

                (from: 2, to: 7, group: -11),

                Sentinel()
                );

            AssertGrouping(
                (from: 2, to: 7, group: -11),

                (from: 0, to: 1, group: -1),
                (from: 0, to: 2, group: -2),
                (from: 1, to: 2, group: -3),
                (from: 1, to: 3, group: -4),
                (from: 2, to: 3, group: -5),

                Sentinel()
                );

            AssertGrouping(
                (from: 7, to: 2, group: -11),

                (from: 0, to: 1, group: -1),
                (from: 0, to: 2, group: -2),
                (from: 1, to: 2, group: -3),
                (from: 1, to: 3, group: -4),
                (from: 2, to: 3, group: -5),

                Sentinel()
                );

            AssertGrouping(
                (from: 0, to: 1, group: -1),
                (from: 0, to: 2, group: -2),
                (from: 1, to: 2, group: -3),
                (from: 1, to: 3, group: -4),
                (from: 2, to: 3, group: -5),

                (from: 7, to: 2, group: -11),

                Sentinel()
                );

        }

        [Test]
        public void Test045()
        {
            AssertGrouping(

                (from: 10, to: 11, group: -10),
                (from: 11, to: 0, group: -10),

                (from: 0, to: 1, group: -1),
                (from: 0, to: 2, group: -2),
                (from: 1, to: 2, group: -3),
                (from: 1, to: 3, group: -4),
                (from: 2, to: 3, group: -5),

                (from: 3, to: 20, group: -10),
                (from: 20, to: 21, group: -10),

                (from: 0, to: 3, group: -10, kind: Knot(-1, -2, -3, -4, -5)),
                (from: 10, to: 21, group: -11, kind: Ser(-10)),

                Sentinel()
                );
        }

        [Test]
        public void Test046a([Values(ImplementationVersions.Latest, ImplementationVersions.V03)]ImplementationVersions ver)
        {
            AssertGrouping(
                ver, 

                (from: 0, to: 1, group: -1),
                (from: 0, to: 4, group: -2),

                (from: 1, to: 2, group: -3),
                (from: 1, to: 3, group: -4),
                (from: 2, to: 3, group: -5),
                (from: 2, to: 7, group: -6),
                (from: 3, to: 7, group: -7),

                (from: 4, to: 5, group: -8),
                (from: 4, to: 6, group: -9),
                (from: 5, to: 6, group: -10),
                (from: 5, to: 7, group: -11),
                (from: 6, to: 7, group: -12),

                (from: 3, to: 6, group: -13),

                (from: 0, to: 7, group: -14, kind: Knot(-1, -2, -3, -4, -5, -6, -7, -8, -9, -10, -11, -12, -13)),
                
                Sentinel()
                );

        }


        [Test]
        public void Test046b()
        {
            AssertGrouping(

                (from: 0, to: 1, group: -1),
                (from: 0, to: 4, group: -2),

                (from: 1, to: 2, group: -3),
                (from: 1, to: 3, group: -4),
                (from: 2, to: 3, group: -5),
                (from: 2, to: 7, group: -6),
                (from: 3, to: 7, group: -7),

                (from: 4, to: 5, group: -8),
                (from: 4, to: 6, group: -9),
                (from: 5, to: 6, group: -10),
                (from: 5, to: 7, group: -11),
                (from: 6, to: 7, group: -12),

                (from: 1, to: 4, group: -13),

                (from: 1, to: 7, group: -14, kind: Knot(-3, -4, -5, -6, -7)),
                (from: 4, to: 7, group: -15, kind: Knot(-8, -9, -10, -11, -12)),

                (from: 0, to: 7, group: -16, kind: Knot(-1, -2, -13, -14, -15)),

                Sentinel()
                );
        }

        [Test]
        public void Test046c()
        {
            AssertGrouping(

                (from: 0, to: 1, group: -1),
                (from: 0, to: 4, group: -2),

                (from: 1, to: 2, group: -3),
                (from: 1, to: 3, group: -4),
                (from: 2, to: 3, group: -5),
                (from: 2, to: 7, group: -6),
                (from: 3, to: 7, group: -7),

                (from: 4, to: 5, group: -8),
                (from: 4, to: 6, group: -9),
                (from: 5, to: 6, group: -10),
                (from: 5, to: 7, group: -11),
                (from: 6, to: 7, group: -12),

                (from: 1, to: 6, group: -13),

                (from: 1, to: 7, group: -14, kind: Knot(-3, -4, -5, -6, -7)),
                (from: 0, to: 7, group: -15, kind: Knot(-1, -2, -14, -8, -9, -10, -11, -12, -13)),

                Sentinel()
                );
        }


        [Test]
        public void Test047()
        {
            AssertGrouping(

                (from: 0, to: 2, group: -1),
                (from: 0, to: 2, group: -2),

                (from: 1, to: 2, group: -3),
                (from: 1, to: 2, group: -4),
                (from: 2, to: 3, group: -5),
                (from: 2, to: 3, group: -6),
                (from: 2, to: 4, group: -7),
                (from: 2, to: 4, group: -8),


                (from: 0, to: 2, group: -10, kind: Par(-1, -2)),
                (from: 1, to: 2, group: -11, kind: Par(-3, -4)),
                (from: 2, to: 3, group: -12, kind: Par(-5, -6)),
                (from: 2, to: 4, group: -13, kind: Par(-7, -8)),

                Sentinel()
                );
        }

        [Test, Combinatorial]
        public void Test048([Values(-1, 0, 1, 2, 3)]int additionalInflow, [Values(-1, 0, 1, 2, 3)]int additionalOutflow, [Values(false, true)]bool otherKnot)
        {
            var inputs = new List<Input>
            {
                (from: 0, to: 1, group: -1),
                (from: 0, to: 2, group: -2),
                (from: 1, to: 2, group: -3),
                (from: 1, to: 3, group: -4),
                (from: 2, to: 3, group: -5),
            };

            var start = 0;
            var end = 3;

            var serId = -10;
            var hasKnot = false;
            if (new[] { -1, 0, 3 }.Contains(additionalInflow) && new[] { -1, 0, 3 }.Contains(additionalOutflow))
            {
                inputs.Add((from: 0, to: 3, group: serId, kind: Knot(-1, -2, -3, -4, -5)));
                hasKnot = true;
            }

            if (otherKnot)
            {
                inputs.AddRange(new Input[]
                {
                    (from: 0, to: 11, group: -11),
                    (from: 0, to: 12, group: -12),
                    (from: 11, to: 12, group: -13),
                    (from: 11, to: 3, group: -14),
                    (from: 12, to: 3, group: -15),

                    (from: 0, to: 3, group: -20, kind: Knot(-11, -12, -13, -14, -15))
                });

                if (hasKnot)
                {
                    inputs.Add((from: 0, to: 3, group: -21, kind: Par(serId, -20)));
                    serId = -21;
                }
            }

            if (additionalInflow >= 0)
            {
                if (additionalInflow == 0)
                {
                    start = 30;
                    inputs.Add((from: 30, to: additionalInflow, group: serId));
                }
                else
                {
                    inputs.Add((from: 30, to: additionalInflow, group: -30));
                }
            }

            if (additionalOutflow >= 0)
            {
                if (additionalOutflow == 3)
                {
                    end = 31;
                    inputs.Add((from: additionalOutflow, to: 31, group: serId));
                }
                else
                {
                    inputs.Add((from: additionalOutflow, to: 31, group: -31));
                }
            }

            if ((start != 0 || end != 3) && hasKnot)
            {
                if (additionalOutflow == 0 || additionalInflow == 3)
                {

                }
                else
                {
                    inputs.Add((from: start, to: end, group: -22, kind: Ser(serId)));
                }
            }

            AssertGrouping(inputs);
        }

        [Test, Combinatorial]
        public void Test049([Values(-1, 0, 1, 2, 3, 4, 5)]int additionalInflow, [Values(-1, 0, 1, 2, 3, 4, 5)]int additionalOutflow, [Values(false, true)]bool otherKnot)
        {
            var inputs = new List<Input>
            {
                (from: 0, to: 1, group: -1),
                (from: 0, to: 2, group: -2),
                (from: 1, to: 2, group: -3),
                (from: 1, to: 3, group: -4),
                (from: 1, to: 4, group: -5),
                (from: 2, to: 4, group: -6),
                (from: 3, to: 4, group: -7),
                (from: 3, to: 5, group: -8),
                (from: 4, to: 5, group: -9),
            };

            var start = 0;
            var end = 5;

            var serId = -10;
            var hasKnot = false;
            if (new[] { -1, 0, 5 }.Contains(additionalInflow) && new[] { -1, 0, 5 }.Contains(additionalOutflow))
            {
                inputs.Add((from: 0, to: 5, group: serId, kind: Knot(-1, -2, -3, -4, -5, -6, -7, -8, -9)));
                hasKnot = true;
            }

            if (otherKnot)
            {
                inputs.AddRange(new Input[]
                {
                    (from: 0, to: 11, group: -11),
                    (from: 0, to: 12, group: -12),
                    (from: 11, to: 12, group: -13),
                    (from: 11, to: 13, group: -14),
                    (from: 11, to: 14, group: -15),
                    (from: 12, to: 14, group: -16),
                    (from: 13, to: 14, group: -17),
                    (from: 13, to: 5, group: -18),
                    (from: 14, to: 5, group: -19),

                    (from: 0, to: 5, group: -20, kind: Knot(-11, -12, -13, -14, -15, -16, -17, -18, -19))
                });

                if (hasKnot)
                {
                    inputs.Add((from: 0, to: 5, group: -21, kind: Par(serId, -20)));
                    serId = -21;
                }
            }

            if (additionalInflow >= 0)
            {
                if (additionalInflow == 0)
                {
                    start = 30;
                    inputs.Add((from: 30, to: additionalInflow, group: serId));
                }
                else
                {
                    inputs.Add((from: 30, to: additionalInflow, group: -30));
                }
            }

            if (additionalOutflow >= 0)
            {
                if (additionalOutflow == 5)
                {
                    end = 31;
                    inputs.Add((from: additionalOutflow, to: 31, group: serId));
                }
                else
                {
                    inputs.Add((from: additionalOutflow, to: 31, group: -31));
                }
            }

            if ((start != 0 || end != 5) && hasKnot)
            {
                if (additionalOutflow == 0 || additionalInflow == 5)
                {

                }
                else
                {
                    inputs.Add((from: start, to: end, group: -22, kind: Ser(serId)));
                }
            }

            AssertGrouping(inputs);
        }

        [Test, Combinatorial]
        public void Test050([Values(-1, 0, 1, 2, 3, 4, 5, 6, 7)]int additionalInflow, [Values(-1, 0, 1, 2, 3, 4, 5, 6, 7)]int additionalOutflow, [Values(false, true)]bool otherKnot)
        {
            var inputs = new List<Input>
            {
                (from: 0, to: 1, group: -1),
                (from: 0, to: 2, group: -2),
                (from: 1, to: 2, group: -3),
                (from: 1, to: 3, group: -4),
                (from: 1, to: 4, group: -5),
                (from: 2, to: 4, group: -6),
                (from: 3, to: 4, group: -7),
                (from: 3, to: 5, group: -8),
                (from: 4, to: 5, group: -9),
                (from: 3, to: 6, group: -10),
                (from: 6, to: 5, group: -11),
                (from: 5, to: 7, group: -12),
                (from: 6, to: 7, group: -13),
            };

            var start = 0;
            var end = 7;

            var serId = -100;
            var hasKnot = false;
            if (new[] { -1, 0, 7 }.Contains(additionalInflow) && new[] { -1, 0, 7 }.Contains(additionalOutflow))
            {
                inputs.Add((from: 0, to: 7, group: serId, kind: Knot(-1, -2, -3, -4, -5, -6, -7, -8, -9, -10, -11, -12, -13)));
                hasKnot = true;
            }

            if (otherKnot)
            {
                inputs.AddRange(new Input[]
                {
                    (from: 0, to: 11, group: -21),
                    (from: 0, to: 12, group: -22),
                    (from: 11, to: 12, group: -23),
                    (from: 11, to: 13, group: -24),
                    (from: 11, to: 14, group: -25),
                    (from: 12, to: 14, group: -26),
                    (from: 13, to: 14, group: -27),
                    (from: 13, to: 15, group: -28),
                    (from: 14, to: 15, group: -29),
                    (from: 13, to: 16, group: -30),
                    (from: 16, to: 15, group: -31),
                    (from: 15, to: 7, group: -32),
                    (from: 16, to: 7, group: -33),

                    (from: 0, to: 7, group: -200, kind: Knot(-21, -22, -23, -24, -25, -26, -27, -28, -29, -30, -31, -32, -33))
                });

                if (hasKnot)
                {
                    inputs.Add((from: 0, to: 7, group: -201, kind: Par(serId, -200)));
                    serId = -201;
                }
            }

            if (additionalInflow >= 0)
            {
                if (additionalInflow == 0)
                {
                    start = 50;
                    inputs.Add((from: 50, to: additionalInflow, group: serId));
                }
                else
                {
                    inputs.Add((from: 50, to: additionalInflow, group: -300));
                }
            }

            if (additionalOutflow >= 0)
            {
                if (additionalOutflow == 7)
                {
                    end = 51;
                    inputs.Add((from: additionalOutflow, to: 51, group: serId));
                }
                else
                {
                    inputs.Add((from: additionalOutflow, to: 51, group: -301));
                }
            }

            if ((start != 0 || end != 7) && hasKnot)
            {
                if (additionalOutflow == 0 || additionalInflow == 7)
                {

                }
                else
                {
                    inputs.Add((from: start, to: end, group: -202, kind: Ser(serId)));
                }
            }

            AssertGrouping(inputs);
        }

        [Test]
        public void Test051a()
        {
            AssertGrouping(
                (from: 0, to: 1, group: -1),
                (from: 1, to: 2, group: -2),
                (from: 0, to: 3, group: -3),
                (from: 3, to: 2, group: -4),
                (from: 1, to: 3, group: -5),
                (from: 0, to: 2, group: -6, kind: Knot(-1, -2, -3, -4, -5)),

                (from: 0, to: 10, group: -10),
                (from: 10, to: 11, group: -11),
                (from: 11, to: 2, group: -12),
                (from: 10, to: 13, group: -13),
                (from: 13, to: 12, group: -14),
                (from: 11, to: 13, group: -15),
                //(from: 10, to: 2, group: -10, kind: Knot(-11, -12, -13, -14, -15)),

                //(from: 0, to: 2, group: -20, kind: Ser(-10)),
                //(from: 0, to: 2, group: -21, kind: Par(-6, -20)),

                Sentinel()
                );
        }

        [Test]
        public void Test051b()
        {
            AssertGrouping(
                (from: 0, to: 1, group: -1),
                (from: 1, to: 2, group: -2),
                (from: 0, to: 3, group: -3),
                (from: 3, to: 2, group: -4),
                (from: 1, to: 3, group: -5),
                (from: 0, to: 2, group: -6, kind: Knot(-1, -2, -3, -4, -5)),

                (from: 0, to: 11, group: -11),
                (from: 11, to: 10, group: -12),
                (from: 10, to: 13, group: -13),
                (from: 13, to: 12, group: -14),
                (from: 11, to: 13, group: -15),
                //(from: 0, to: 10, group: -10, kind: Knot(-11, -12, -13, -14, -15)),

                (from: 10, to: 2, group: -10),
                
                //(from: 0, to: 2, group: -20, kind: Ser(-10)),
                //(from: 0, to: 2, group: -21, kind: Par(-6, -20)),

                Sentinel()
                );
        }

        [Test]
        public void Test052a()
        {
            AssertGrouping(
                (from: 0, to: 1, group: -1),
                (from: 1, to: 2, group: -2),
                (from: 0, to: 3, group: -3),
                (from: 3, to: 2, group: -4),
                (from: 1, to: 3, group: -5),
                (from: 0, to: 2, group: -6, kind: Knot(-1, -2, -3, -4, -5)),

                (from: 0, to: 11, group: -11),
                (from: 11, to: 10, group: -12),
                (from: 10, to: 13, group: -13),
                (from: 13, to: 12, group: -14),
                (from: 11, to: 13, group: -15),
                (from: 10, to: 12, group: -16),
                (from: 11, to: 12, group: -11, kind: Knot(-12, -13, -14, -15, -16)),
                (from: 12, to: 2, group: -11),


                (from: 0, to: 2, group: -20, kind: Ser(-11)),
                (from: 0, to: 2, group: -21, kind: Par(-6, -20)),

                Sentinel()
                );
        }

        [Test]
        public void Test052b()
        {
            AssertGrouping(
                (from: 0, to: 1, group: -1),
                (from: 1, to: 2, group: -2),
                (from: 0, to: 3, group: -3),
                (from: 3, to: 2, group: -4),
                (from: 1, to: 3, group: -5),
                (from: 0, to: 2, group: -6, kind: Knot(-1, -2, -3, -4, -5)),

                (from: 0, to: 11, group: -11),
                (from: 11, to: 10, group: -12),
                (from: 10, to: 13, group: -13),
                (from: 13, to: 2, group: -14),
                (from: 11, to: 13, group: -15),
                (from: 10, to: 2, group: -16),
                (from: 11, to: 2, group: -11, kind: Knot(-12, -13, -14, -15, -16)),
                

                (from: 0, to: 2, group: -20, kind: Ser(-11)),
                (from: 0, to: 2, group: -21, kind: Par(-6, -20)),

                Sentinel()
                );
        }

        [Test]
        public void Test052c()
        {
            AssertGrouping(
                (from: 0, to: 1, group: -1),
                (from: 1, to: 2, group: -2),
                (from: 0, to: 3, group: -3),
                (from: 3, to: 2, group: -4),
                (from: 1, to: 3, group: -5),
                (from: 0, to: 2, group: -6, kind: Knot(-1, -2, -3, -4, -5)),

                (from: 0, to: 11, group: -11),
                (from: 11, to: 10, group: -12),
                (from: 10, to: 13, group: -13),
                (from: 13, to: 2, group: -14),
                (from: 11, to: 13, group: -15),
                (from: 10, to: 2, group: -16),
                (from: 11, to: 2, group: -17, kind: Knot(-12, -13, -14, -15, -16)),

                (from: 11, to: 30, group: -31),
                (from: 30, to: 33, group: -32),
                (from: 33, to: 2, group: -33),
                (from: 11, to: 33, group: -34),
                (from: 30, to: 2, group: -35),
                (from: 11, to: 2, group: -36, kind: Knot(-31, -32, -33, -34, -35)),

                (from: 11, to: 2, group: -11, kind: Par(-17, -36)),

                (from: 0, to: 2, group: -20, kind: Ser(-11)),
                (from: 0, to: 2, group: -21, kind: Par(-6, -20)),

                Sentinel()
                );
        }

        [Test]
        public void Test053()
        {
            AssertGrouping(
                (from: 0, to: 2, group: -13),

                (from: 0, to: 1, group: -1),
                (from: 1, to: 2, group: -2),
                (from: 0, to: 3, group: -3),
                (from: 3, to: 2, group: -4),
                (from: 1, to: 3, group: -5),
                (from: 0, to: 2, group: -6, kind: Knot(-1, -2, -3, -4, -5)),

                (from: 0, to: 2, group: -11),
                (from: 0, to: 2, group: -12),
                
                (from: 0, to: 2, group: -31, kind: Par(-6, -11, -12, -13)),

                Sentinel()
                );
        }

        [Test]
        public void Test054()
        {
            AssertGrouping(
                (from: 10, to: 11, group: -11),
                (from: 11, to: 0, group: -11),

                (from: 0, to: 1, group: -1),
                (from: 1, to: 2, group: -2),
                (from: 0, to: 3, group: -3),
                (from: 3, to: 2, group: -4),
                (from: 1, to: 3, group: -5),
                (from: 0, to: 2, group: -11, kind: Knot(-1, -2, -3, -4, -5)),

                (from: 2, to: 12, group: -11),
                (from: 12, to: 13, group: -11),

                (from: 10, to: 13, group: -31, kind: Ser(-11)),

                Sentinel()
                );
        }

    }
}
