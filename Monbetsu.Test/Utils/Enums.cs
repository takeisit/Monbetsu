using System;

namespace Monbetsu.Test.Utils
{
    [Flags]
    public enum ImplementationVersions
    {
        Latest = 1 << 0,
        V03 = 1 << 1,

        All = -1
    }

    public enum GroupKind
    {
        Unknown,
        Edge,
        Knot,
        Series,
        Parallel
    }

    public enum ClassificationVariation
    {
        Default,
        Unique,
        Integrated
    }

}
