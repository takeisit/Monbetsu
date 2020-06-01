namespace Monbetsu.Test.Utils
{
    struct Input
    {
        
        public int From { get; }
        public int To { get; }
        public int Group { get; }

        public GroupKind Kind { get; }

        public LabelGroup Labels { get; }

        public Input(int from, int to, int group)
        {
            From = from;
            To = to;
            Group = group;
            Kind = GroupKind.Edge;
            Labels = LabelGroup.Edge();
            Labels.Group = group;
        }

        public Input(int from, int to, int group, LabelGroup labelGroup)
        {
            From = from;
            To = to;
            Group = group;
            Kind = labelGroup.Kind;
            Labels = labelGroup;
            Labels.Group = group;
        }

        public static implicit operator Input((int from, int to) tuple)
        {
            return new Input(tuple.from, tuple.to, 0);
        }

        public static implicit operator Input((int from, int to, int group) tuple)
        {
            return new Input(tuple.from, tuple.to, tuple.group);
        }

        public static implicit operator Input((int from, int to, int group, LabelGroup kind) tuple)
        {
            return new Input(tuple.from, tuple.to, tuple.group, tuple.kind);
        }
    }
}
