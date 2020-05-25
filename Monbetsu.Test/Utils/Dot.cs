using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;

namespace Dot
{
    

    struct DotId : IEquatable<DotId>
    {
        public string Name { get; }

        public DotId(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public static implicit operator DotId(string name) => new DotId(name);
        public static implicit operator DotId(int number) => new DotId(number.ToString());

        public bool Equals([AllowNull] DotId other) => Name == other.Name;

        public override bool Equals(object? obj) => obj is DotId other ? Equals(other) : false;

        public override int GetHashCode()
        {
            return HashCode.Combine(Name);
        }

        public override string ToString()
        {
            return $"\"{Name.Replace("\"", "\\\"")}\"";
        }

        public static bool operator ==(DotId left, DotId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DotId left, DotId right)
        {
            return !(left == right);
        }
    }

    interface ILabelString : IEquatable<ILabelString>
    {

    }

    struct LabelString : IEquatable<LabelString>
    {
        public ILabelString Text { get; }

        public LabelString(ILabelString text)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
        }

        public LabelString(string text)
        {
            Text = new EscapedString(text);
        }

        public static implicit operator LabelString(string text)
        {
            return new LabelString(text);
        }

        public static implicit operator LabelString(EscapedString text)
        {
            return new LabelString(text);
        }

        public override bool Equals(object? obj)
        {
            return obj is LabelString @string ? Equals(@string) : false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Text);
        }

        public override string ToString() => Text?.ToString() ?? "";

        public bool Equals([AllowNull] LabelString other)
        {
            return Text.Equals(other.Text);
        }

        public static bool operator ==(LabelString left, LabelString right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(LabelString left, LabelString right)
        {
            return !(left == right);
        }
    }

    struct EscapedString : IEquatable<EscapedString>, ILabelString
    {
        public string Text { get; }

        public EscapedString(string text, bool isEscaped = false)
        {
            Text = isEscaped ? text : $"\"{text}\"";
        }

        public static implicit operator EscapedString(string text)
        {
            return new EscapedString(text);
        }

        public override bool Equals(object? obj)
        {
            return obj is EscapedString @string ? Equals(@string) : false;
        }

        public bool Equals([AllowNull] EscapedString other)
        {
            return Text == other.Text;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Text);
        }



        public static bool operator ==(EscapedString left, EscapedString right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(EscapedString left, EscapedString right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return Text;
        }

        public bool Equals([AllowNull] ILabelString other)
        {
            return other is EscapedString esc ? Equals(esc) : false;
        }
    }

    interface IColorOrList
    {

    }

    interface IColor : IColorOrList
    {

    }

    class RgbColor : IColor
    {
        public byte Red { get; }
        public byte Green { get; }
        public byte Blue { get; }

        public RgbColor(byte red, byte green, byte blue)
        {
            Red = red;
            Green = green;
            Blue = blue;
        }

        public override string ToString() => $"\"#{Red:X2}{Green:X2}{Blue:X2}\"";
    }

    class RgbaColor : IColor
    {
        public byte Red { get; }
        public byte Green { get; }
        public byte Blue { get; }

        public byte Alpha { get; }

        public RgbaColor(byte red, byte green, byte blue, byte alpha)
        {
            Red = red;
            Green = green;
            Blue = blue;
            Alpha = alpha;
        }

        public RgbaColor WithAlpha(byte alpha) => new RgbaColor(Red, Green, Blue, alpha);

        public override string ToString() => $"\"#{Red:X2}{Green:X2}{Blue:X2}{Alpha:X2}\"";
    }

    class NamedColor : IColor
    {
        public static readonly IColor Green = new NamedColor("green");
        public static readonly IColor Gray = new NamedColor("gray");
        public static readonly IColor Blue = new NamedColor("blue");
        public static readonly IColor Red = new NamedColor("red");
        public static readonly IColor Black = new NamedColor("black");
        public static readonly IColor White = new NamedColor("white");
        public static readonly IColor Transparent = new RgbaColor(0, 0, 0, 0);

        public string ColorName { get; }

        private NamedColor(string colorName)
        {
            ColorName = colorName ?? throw new ArgumentNullException(nameof(colorName));
        }

        public override string ToString()
        {
            return ColorName;
        }
    }

    abstract class GraphBase
    {
        public DotId? Id { get; set; }

        public List<Node> Nodes { get; set; } = new List<Node>();
        public List<Edge> Edges { get; set; } = new List<Edge>();

        public List<SubgraphBase> Subgraphs { get; set; } = new List<SubgraphBase>();

        public List<Rank> Ranks { get; set; } = new List<Rank>();

        protected virtual void WriteChildElements(RootGraph root, StringBuilder builder, int indent)
        {
            foreach (var node in Nodes)
            {
                node.WriteChildElements(root, builder, indent);
            }

            foreach (var edge in Edges)
            {
                edge.WriteChildElements(root, builder, indent);
            }

            foreach(var subgraph in Subgraphs)
            {
                subgraph.WriteChildElements(root, builder, indent);
            }

            foreach (var rank in Ranks)
            {
                rank.WriteChildElements(root, builder, indent);
            }
        }

        protected virtual void WriteAttributes(RootGraph root, StringBuilder builder, int indent)
        {

        }
    }

    abstract class RootGraph : GraphBase
    {
        public bool Strict { get; set; }

        public Size? Size { get; set; }
        public double? RankSep { get; set; } = 0.5;

        public bool? NewRank { get; set; }

        public Splines? Splines { get; set; }


        public Viewport? Viewport { get; set; }

        public RankDir? RankDir { get; set; }
        public PageDir? PageDir { get; set; }

        internal abstract string EdgeOp { get; }

        protected override void WriteAttributes(RootGraph root, StringBuilder builder, int indent)
        {
            if (NewRank != null)
            {
                WriteIndent(builder, indent).AppendLine($"newrank = {NewRank}");
            }

            if (RankSep != null)
            {
                WriteIndent(builder, indent).AppendLine($"ranksep = {RankSep}");
            }

            if (Size != null)
            {
                WriteIndent(builder, indent).AppendLine($"size = {Size}");
            }

            if (Splines != null)
            {
                WriteIndent(builder, indent).AppendLine($"splines = {Splines}");
            }

            if (Viewport!= null)
            {
                WriteIndent(builder, indent).AppendLine($"viewport = {Viewport}");
            }

            if (PageDir != null)
            {
                WriteIndent(builder, indent).AppendLine($"pagedir = {PageDir}");
            }

            if (RankDir != null)
            {
                WriteIndent(builder, indent).AppendLine($"rankdir = {RankDir}");
            }
        }

        internal StringBuilder WriteIndent(StringBuilder builder, int indent)
        {
            for (var i = 0; i < indent; i++)
            {
                builder.Append("  ");
            }
            return builder;
        }

        internal string Build()
        {
            var builder = new StringBuilder();

            if (Strict)
            {
                builder.Append("strict ");
            }

            builder.Append(GetType().Name.ToLower());

            if (Id.HasValue)
            {
                builder.Append($" {Id.Value}");
            }
            builder.AppendLine(" {");

            WriteAttributes(this, builder, 1);
            WriteChildElements(this, builder, 1);

            builder.AppendLine("}");

            return builder.ToString();
        }
    }

    class Graph : RootGraph
    {
        internal override string EdgeOp => " -- ";

    }

    class Digraph : RootGraph
    {
        internal override string EdgeOp => " -> ";
    }

    interface IStatement
    {
        void WriteChildElements(RootGraph root, StringBuilder builder, int indent);
    }

    abstract class SubgraphBase : GraphBase, IStatement
    {
        protected abstract DotId? SubgraphId { get; }

        protected override void WriteChildElements(RootGraph root, StringBuilder builder, int indent)
        {
            root.WriteIndent(builder, indent);

            builder.Append("subgraph");
            if (SubgraphId.HasValue)
            {
                builder.Append($" {SubgraphId}");
            }
            builder.AppendLine(" {");

            base.WriteChildElements(root, builder, indent + 1);

            WriteAttributes(root, builder, indent + 1);

            root.WriteIndent(builder, indent);
            builder.AppendLine("}");
        }

        

        void IStatement.WriteChildElements(RootGraph root, StringBuilder builder, int indent)
            => WriteChildElements(root, builder, indent);
    }

    class Subgraph : SubgraphBase
    {
        protected override DotId? SubgraphId => Id;
    }

    class Cluster : SubgraphBase
    {
        public ClusterStyle? Style { get; set; }

        protected override DotId? SubgraphId => Id.HasValue ? new DotId("cluster_" + Id.Value.Name) : new DotId("cluster");

        protected override void WriteAttributes(RootGraph root, StringBuilder builder, int indent)
        {
            if (Style.HasValue)
            {
                root.WriteIndent(builder, indent).AppendLine($"style = {Style}");
            }
        }
    }

    class Node : IStatement
    {
        public DotId Id { get; set; }
        public LabelString? Label { get; set; }

        public NodeStyle? Style { get; set; }

        public IColorOrList? Color { get; set; }

        public IColor? FontColor { get; set; }

        public IShape? Shape { get; set; }

        public double? Width { get; set; }
        public double? Height { get; set; }

        public void WriteChildElements(RootGraph root, StringBuilder builder, int indent)
        {
            root.WriteIndent(builder, indent);

            builder.Append(Id);

            var attrList = new List<string>();

            if (Label != null)
            {
                attrList.Add($"label={Label}");
            }
            if (Style.HasValue)
            {
                attrList.Add($"style={Style}");
            }
            if (Color != null)
            {
                attrList.Add($"color={Color}");
            }
            if (FontColor != null)
            {
                attrList.Add($"fontcolor={FontColor}");
            }
            if (Shape != null)
            {
                attrList.Add($"shape={Shape}");
            }
            if (Width != null)
            {
                attrList.Add($"width={Width}");
            }
            if (Height != null)
            {
                attrList.Add($"width={Height}");
            }

            if (attrList.Count > 0)
            {
                builder.Append(" [");
                builder.AppendJoin(", ", attrList);
                builder.Append("]");
            }

            builder.AppendLine();
        }
    }

    class Edge : IStatement
    {
        public List<DotId> Nodes { get; set; } = new List<DotId>();
        public DotId From
        {
            get => Nodes[0];
            set
            {
                if (Nodes.Count == 0)
                {
                    Nodes.Add(value);
                }
                else
                {
                    Nodes[0] = value;
                }
            }
        }

        public DotId To
        {
            get => Nodes[1];
            set
            {
                while (Nodes.Count <= 1)
                {
                    Nodes.Add(default!);
                }
                Nodes[1] = value;
            }
        }

        public LabelString? Label { get; set; }
        
        public EdgeStyle? Style { get; set; }

        public IColorOrList? Color { get; set; }

        public IColor? FontColor { get; set; }

        public ArrowType? ArrowHead { get; set; }
        public ArrowType? ArrowTail { get; set; }

        public (IReadOnlyList<Edge> edges, IReadOnlyList<Node> relayPoints) SplitByInvisibleRelay(int numberOfPoint = 1, Func<Edge, int, DotId>? pointNamer = null)
        {
            if (Nodes.Count != 2)
            {
                throw new NotSupportedException();
            }

            if (numberOfPoint < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(numberOfPoint));
            }

            pointNamer ??= (_0, _1) => new DotId($"relay-{Guid.NewGuid()}");

            var edges = Enumerable.Range(0, numberOfPoint + 1).Select(_ => new Edge()).ToList();
            var nodes = Enumerable.Range(0, numberOfPoint).Select(_ => new Node()).ToList();

            edges[0].From = From;
            edges[0].ArrowTail = ArrowTail;

            for (var i = 0; i < numberOfPoint; i++)
            {
                nodes[i].Id = pointNamer(this, i);
                nodes[i].Color = NamedColor.Transparent;
                nodes[i].Shape = new PolygonBasedShape(PolygonBasedShapeKind.point);
                nodes[i].Style = NodeStyle.invis;
                nodes[i].Width = 0;
                nodes[i].Height = 0;

                edges[i].To = nodes[i].Id;
                edges[i].ArrowHead = ArrowType.none;
                edges[i + 1].ArrowTail = ArrowType.none;
                edges[i + 1].From = nodes[i].Id;
            }

            edges.Last().To = To;
            edges.Last().ArrowHead = ArrowHead;

            foreach (var edge in edges)
            {
                edge.Style = Style;
                edge.Color = Color;
                edge.FontColor = FontColor;
                edge.Label = Label;
            }

            edges[0].ArrowHead = ArrowType.none;

            return (edges, nodes);
        }

        public void WriteChildElements(RootGraph root, StringBuilder builder, int indent)
        {
            root.WriteIndent(builder, indent);

            builder.AppendJoin(root.EdgeOp, Nodes);

            var attrList = new List<string>();

            if (Label != null)
            {
                attrList.Add($"label={Label}");
            }
            if (Style.HasValue)
            {
                attrList.Add($"style={Style}");
            }
            if (Color != null)
            {
                attrList.Add($"color={Color}");
            }
            if (FontColor != null)
            {
                attrList.Add($"fontcolor={FontColor}");
            }
            if (ArrowHead != null)
            {
                attrList.Add($"arrowhead={ArrowHead}");
            }
            if (ArrowTail != null)
            {
                attrList.Add($"arrowtail={ArrowTail}");
            }

            if (attrList.Count > 0)
            {
                builder.Append(" [");
                builder.AppendJoin(", ", attrList);
                builder.Append("]");
            }

            builder.AppendLine();
        }
    }

    enum ClusterStyle
    {
        dashed,
        dotted,
        solid,
        invis,
        bold,
        filled,
        striped,
        rounded
    }

    enum EdgeStyle
    {
        dashed,
        dotted,
        solid,
        invis,
        bold,
        tapered 
    }

    enum NodeStyle
    {
        dashed,
        dotted,
        solid,
        invis,
        bold,
        filled, striped, wedged, diagonals, rounded
    }

    enum Splines
    {
        none,
        line,
        @false,
        polyline,
        curved,
        ortho,
        spline,
        @true
    }

    interface IShape
    {

    }

    enum PolygonBasedShapeKind
    {
        box,
        polygon,
        ellipse,
        oval,
        circle, point,   egg, triangle,
        plaintext, plain,   diamond, trapezium,
        parallelogram, house,   pentagon, hexagon,
        septagon, octagon, doublecircle, doubleoctagon,
        tripleoctagon, invtriangle, invtrapezium, invhouse,
        Mdiamond, Msquare, Mcircle, rect,
        rectangle, square,  star, none,
        underline, cylinder,    note, tab,
        folder, box3d,   component, promoter,
        cds, terminator,  utr, primersite,
        restrictionsite, fivepoverhang,   threepoverhang, noverhang,
        assembly, signature,   insulator, ribosite,
        rnastab, proteasesite,    proteinstab, rpromoter,
        rarrow, larrow,  lpromoter,
    }

    class PolygonBasedShape : IShape
    {
        public static readonly IShape Box = new PolygonBasedShape(PolygonBasedShapeKind.box);

        public PolygonBasedShapeKind Kind { get; }

        public PolygonBasedShape(PolygonBasedShapeKind kind)
        {
            Kind = kind;
        }

        public static implicit operator PolygonBasedShape(PolygonBasedShapeKind kind) => new PolygonBasedShape(kind);

        public override string ToString()
        {
            return Kind.ToString();
        }
    }

    enum ArrowType
    {
        normal,
        inv,
        dot,
        invdot,
        odot,
        invodot,
        none,
        tee,
        empty,
        invempty,
        diamond,
        odiamond,
        ediamond,
        crow,
        box,
        obox,
        open,
        halfopen,
        vee
    }

    enum RankType
    {
        same, min, source, max, sink
    }

    class Rank : HashSet<DotId>, IStatement
    {
        public RankType Type { get; } = RankType.same;

        public Rank(RankType type)
        {
            Type = type;
        }

        public override string ToString()
        {
            if (Count == 0)
            {
                return "";
            }
            return $"{{rank = {Type} {string.Join(" ", this)}}}";
        }

        public void WriteChildElements(RootGraph root, StringBuilder builder, int indent)
        {
            root.WriteIndent(builder, indent).AppendLine(ToString());
        }
    }

    struct Size : IEquatable<Size>
    {
        public double Width { get; }
        public double Height { get; }

        public Size(double width, double height)
        {
            Width = width;
            Height = height;
        }

        public static implicit operator Size(double size) => new Size(size, size);
        public static implicit operator Size((double width, double height) size) => new Size(size.width, size.height);

        public bool Equals(Size other)
        {
            return Width == other.Width &&
                   Height == other.Height;
        }

        public override bool Equals(object? obj)
        {
            return obj is Size size && Equals(size);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Width, Height);
        }

        public static bool operator ==(Size left, Size right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Size left, Size right)
        {
            return !(left == right);
        }

        public override string ToString() => $"\"{Width},{Height}\"";

    }

    struct Point  : IEquatable<Point>
    {
        public double X { get; }
        public double Y { get; }

        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }

        public static implicit operator Point(double pos) => new Point(pos, pos);
        public static implicit operator Point((double x, double y) point) => new Point(point.x, point.y);

        public bool Equals(Point other)
        {
            return X == other.X &&
                   Y == other.Y;
        }

        public override bool Equals(object? obj)
        {
            return obj is Point point && Equals(point);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        public static bool operator ==(Point left, Point right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Point left, Point right)
        {
            return !(left == right);
        }

        public override string ToString() => $"\"{X},{Y}\"";

    }

    class Viewport
    {
        public double Width { get; set; }
        public double Height { get; set; }

        public double Zoom { get; set; } = 1;

        public DotId? CenterNodeId { get; set; }

        public Point? Center { get; set; }


        public override string ToString()
        {
            var center = "";

            if (CenterNodeId != null)
            {
                center = $",{CenterNodeId.Value.Name}";
            }
            else if (Center != null)
            {
                center = $",{Center.Value.X},{Center.Value.Y}";
            }

            return $"\"{Width},{Height},{Zoom}{center}\"";
        }
    }

    enum RankDir
    {
        TB,
        LR,
        BT,
        RL
    }

    enum PageDir
    {
        BL,
        BR,
        TL,
        TR,
        RB,
        RT,
        LB,
        LT
    }
}
