using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Monbetsu.Test.Utils
{
    [DebuggerDisplay("{Kind}: {Group}")]
    class LabelGroup : IEquatable<LabelGroup>
    {
        public static LabelGroup Edge(int groupId = 0) => new LabelGroup(Array.Empty<int>(), GroupKind.Edge) { Group = groupId };
        public static LabelGroup Unknown(int groupId = 0) => new LabelGroup(Array.Empty<int>(), GroupKind.Unknown) { Group = groupId };

        public UnorderedNTuple<int> ToTuple() => new UnorderedNTuple<int>(Children);

        public override string ToString()
        {
            return $"{Kind.ToString()[0]}{Group}";
        }

        public override bool Equals(object? obj)
        {
            return obj is LabelGroup group && Equals(group);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Kind, Group);
        }

        public bool Equals([AllowNull] LabelGroup other)
        {
            if (other! == null!)
            {
                return false;
            }


            return Children.OrderBy(_ => _).SequenceEqual(other.Children.OrderBy(_ => _)) &&
                   Kind == other.Kind &&
                   Group == other.Group;
        }

        public IReadOnlyList<int> Children { get; }

        public GroupKind Kind { get; }

        public int Group { get; set; }

        public LabelGroup(IReadOnlyList<int> children, GroupKind kind)
        {
            Children = children ?? throw new ArgumentNullException(nameof(children));
            Kind = kind;
        }

        public static bool operator ==(LabelGroup left, LabelGroup right)
        {
            return EqualityComparer<LabelGroup>.Default.Equals(left, right);
        }

        public static bool operator !=(LabelGroup left, LabelGroup right)
        {
            return !(left == right);
        }
    }
}
