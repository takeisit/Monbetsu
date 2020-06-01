using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Monbetsu.Test.Utils;

namespace Monbetsu.Test
{
    public partial class VariationTests : Verifier
    {


        [Test]
        public void TestVariation001i()
        {
            AssertGrouping(ImplementationVersions.Latest, ClassificationVariation.Integrated, new Input[] {
                (from: 0, to: 1, group: -1),
                (from: 1, to: 2, group: -1),
                (from: 0, to: 2, group: -1, kind: Ser(-1)),
            });
        }

        [Test]
        public void TestVariation001u()
        {
            AssertGrouping(ImplementationVersions.Latest, ClassificationVariation.Unique, new Input[] {
                (from: 0, to: 1, group: -1),
                (from: 1, to: 2, group: -2),
                (from: 0, to: 2, group: -3, kind: Ser(-1, -2)),
            });
        }

        [Test]
        public void TestVariation002i()
        {
            AssertGrouping(ImplementationVersions.Latest, ClassificationVariation.Integrated, new Input[] {
                (from: 0, to: 1, group: -1),
                (from: 1, to: 2, group: -1),
                (from: 0, to: 2, group: -1, kind: Ser(-1)),
                (from: 0, to: 2, group: -2),
                (from: 0, to: 2, group: -3, kind: Par(-1, -2)),
            });
        }

        [Test]
        public void TestVariation002u()
        {
            AssertGrouping(ImplementationVersions.Latest, ClassificationVariation.Unique, new Input[] {
                (from: 0, to: 1, group: -1),
                (from: 1, to: 2, group: -2),
                (from: 0, to: 2, group: -3, kind: Ser(-1, -2)),
                (from: 0, to: 2, group: -4),
                (from: 0, to: 2, group: -5, kind: Par(-3, -4)),
            });
        }
    }
}
