using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using System.IO;
using System.Text.RegularExpressions;
using Markdig;
using Markdig.Renderers.Normalize;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Monbetsu.Test.Utils;
using System.Text;
using Monbetsu.Test;

namespace Monbetsu.Docs
{
    
    [Ignore("only for docs")]
    public class ReadMe
    {
        [Test]
        public void GenerateReadMe()
        {
            var templatePath = Path.Combine(PathUtil.SolutionDir, "docs", "README.template.md");
            var outputPath = Path.Combine(PathUtil.SolutionDir, "README.md");

            var text = File.ReadAllText(templatePath);

            var transformed = MarkdownTransformer.Transform(text,
                new TutorialEmbedder(PathUtil.GetSolutionItemPath("docs")),
                new LinkResolver(PathUtil.GetSolutionItemPath("docs"), PathUtil.SolutionDir)
                );

            transformed = Regex.Replace(transformed, "\r*\n", "\n", RegexOptions.Singleline);

            File.WriteAllText(outputPath, transformed);
        }

        class MarkdownSource
        {
            public string WholeSource { get; }
            public MarkdownDocument Document { get; }

            public MarkdownSource(string source)
            {
                WholeSource = source;
                Document = Markdown.Parse(source);
            }

            public MarkdownSource Update()
            {
                using var writer = new StringWriter();
                var renderer = new NormalizeRenderer(writer);

                renderer.Render(Document);

                return new MarkdownSource(writer.ToString());
            }
        }

        abstract class MarkdownTransformer
        {
            public static void Transform(string sourceText, TextWriter sink, params MarkdownTransformer[] transformers)
            {
                var source = new MarkdownSource(sourceText);

                foreach(var transformer in transformers)
                {
                    source = transformer.Transform(source);
                }

                sink.Write(source.WholeSource);
            }

            public static string Transform(string sourceText, params MarkdownTransformer[] transformers)
            {
                using var writer = new StringWriter();
                Transform(sourceText, writer, transformers);
                return writer.ToString();
            }

            protected abstract MarkdownSource Transform(MarkdownSource source);
        }

        
        class LinkResolver : MarkdownTransformer
        {
            private readonly string sourceDir;
            private readonly string destinationDir;

            public LinkResolver(string sourceDir, string destinationDir)
            {
                this.sourceDir = sourceDir;
                this.destinationDir = destinationDir;
            }

            protected override MarkdownSource Transform(MarkdownSource source)
            {
                foreach (var obj in source.Document.Descendants())
                {
                    switch (obj)
                    {
                        case LinkInline linkInline:
                            //Debug.WriteLine($"{linkInline.Url}");
                            linkInline.Url = ResolveRelativePath(linkInline.Url, sourceDir, destinationDir);
                            break;
                    }
                }

                return source.Update();
            }
        }

        class TutorialEmbedder : MarkdownTransformer
        {
            private static readonly Regex NamespacePlaceholderRegex = new Regex(
                @"^namespace\s+(?<namespace>.+)\s*" +
                @"^{\s*" +
                @"^}\s*$",
                RegexOptions.Multiline);


            private static readonly Regex FinderRegex = new Regex(
                @"^namespace\s+(?<namespace>.+)\s*" +
                @"^{\s*" +
                @"(.|\n)*?" +
                @"^}\s*$",
                RegexOptions.Multiline);

            private readonly string baseDir;

            public TutorialEmbedder(string baseDir)
            {
                this.baseDir = baseDir;
            }

            protected override MarkdownSource Transform(MarkdownSource source)
            {
                var newSource = new StringBuilder();
                var anchor = 0;

                foreach (var obj in source.Document.Descendants())
                {
                    switch (obj)
                    {
                        case FencedCodeBlock fencedCodeBlock:
                            if (fencedCodeBlock.Info == "csharp" && fencedCodeBlock.Arguments != null)
                            {
                                var targetPath = Path.Combine(baseDir, fencedCodeBlock.Arguments);
                                if (File.Exists(targetPath))
                                {
                                    if (TryParseFencedCode(fencedCodeBlock, source.WholeSource, out var result))
                                    {
                                        var placeholderMatch = NamespacePlaceholderRegex.Match(result.code);
                                        if (placeholderMatch.Success)
                                        {
                                            var ns = placeholderMatch.Groups["namespace"].Value.Trim();
                                            var csCode = File.ReadAllText(targetPath);

                                            foreach (var match2 in FinderRegex.Matches(csCode).OfType<Match>())
                                            {
                                                var ns2 = match2.Groups["namespace"].Value.Trim();
                                                if (ns == ns2)
                                                {
                                                    var csCodePart = Regex.Replace(match2.Value, @"^\s*\[Test\]\s*$", "", RegexOptions.Multiline);
                                                    csCodePart = Regex.Replace(csCodePart, @"\bTestContext[.]WriteLine\b", "Debug.WriteLine", RegexOptions.Multiline);

                                                    newSource.Append(source.WholeSource, anchor, fencedCodeBlock.Span.Start - anchor);
                                                    newSource.Append(result.fence);
                                                    newSource.AppendLine("csharp");
                                                    newSource.Append(csCodePart);
                                                    newSource.AppendLine(result.fence);
                                                    anchor = fencedCodeBlock.Span.End + 1;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                    }
                }

                newSource.Append(source.WholeSource, anchor, source.WholeSource.Length - anchor);

                return new MarkdownSource(newSource.ToString());
            }
        }

        private static string? ResolveRelativePath(string? path, string sourceDir, string destinationDir)
        {
            if (path == null)
            {
                return null;
            }
            else if (path.StartsWith("http://") || path.StartsWith("https://"))
            {
                return path;
            }
            else if (Path.IsPathRooted(path))
            {
                return path;
            }

            var sourcePath = Path.Combine(sourceDir, path);

            if (Path.GetRelativePath(PathUtil.SolutionDir, sourcePath).Replace(Path.DirectorySeparatorChar, '/').StartsWith("../"))
            {
                return path;
            }

            var destinationPath = Path.GetRelativePath(destinationDir, sourcePath).Replace(Path.DirectorySeparatorChar, '/');
            
            return destinationPath;
        }

        private static bool TryParseFencedCode(FencedCodeBlock fencedCodeBlock, string wholeSource, out (string code, int index, int length, string fence) result)
        {
            var pattern = $@"[{fencedCodeBlock.FencedChar}]{{{fencedCodeBlock.FencedCharCount}}}.*?\n(.*)[{fencedCodeBlock.FencedChar}]{{{fencedCodeBlock.FencedCharCount}}}";
            var regex = new Regex(pattern, RegexOptions.Singleline);
            var match = regex.Match(wholeSource, fencedCodeBlock.Span.Start, fencedCodeBlock.Span.Length);
            if (match.Success)
            {
                result = (match.Groups[1].Value, match.Groups[1].Index, match.Groups[1].Length, new string(fencedCodeBlock.FencedChar, fencedCodeBlock.FencedCharCount));
                return true;
            }
            else
            {
                result = ("", 0, 0, "");
                return false;
            }
        }
    }

    [Ignore("only for docs")]
    public class Demo
    {
        [Test]
        public void PublishToDocs()
        {
            var docsPath = PathUtil.GetSolutionItemPath("docs");

            PublishDemo(docsPath);
        }

        [Test]
        public void PublishToPagesTest()
        {
            // docker run --rm --volume="$(PWD):/srv/jekyll" -it -p 4000:4000 jekyll/jekyll jekyll serve -b /Monbetsu

            File.Copy(PathUtil.GetSolutionItemPath("docs", "_config.yml"), Path.Combine(PathUtil.LocalJekyllPath, "_config.yml"), true);

            PublishDemo(PathUtil.LocalJekyllPath);

            File.Copy(PathUtil.GetSolutionItemPath("README.md"), Path.Combine(PathUtil.LocalJekyllPath, "README.md"), true);

            Directory.CreateDirectory(Path.Combine(PathUtil.LocalJekyllPath, "docs", "images"));
            PathUtil.CopyDicrectory(PathUtil.GetSolutionItemPath("docs", "images"), Path.Combine(PathUtil.LocalJekyllPath, "docs", "images")); ;
        }

        private void PublishDemo(string destinationPath)
        {
            var demoProjDirPath = PathUtil.GetSolutionItemPath("Monbetsu.BlazorDemo");
            var publishDirPath = Path.Combine(demoProjDirPath, "temp-publish");

            Directory.CreateDirectory(publishDirPath);

            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = PathUtil.Arguments("publish", "-o", publishDirPath, "-c", "Release"),
                WorkingDirectory = demoProjDirPath,
            };

            Process.Start(startInfo).WaitForExit();

            PathUtil.CopyDicrectory(Path.Combine(publishDirPath, "wwwroot"), destinationPath);

            Directory.Delete(publishDirPath, true);
        }

    }

    [Ignore("only for docs")]
    public class Image
    {
        class RecOptions : Recorder<Graph, Node, Edge, RecordLabel>.RecordOptions
        {
            public RecOptions()
            {
                EdgeDecorator = (dotEdge, nodes, label, flags, options) =>
                {
                    dotEdge.Label = label?.LabelText ?? "";
                };
                SeriesSubgraphDecorator = (dotEdge, nodes, label, flags, options) =>
                {
                    dotEdge.Label = $"{label.LabelText}\nSeries\n{{{string.Join(", ", nodes.Via.OrderBy(l => l.Order))}}}";
                };
                ParallelSubgraphDecorator = (dotEdge, nodes, label, flags, options) =>
                {
                    dotEdge.Label = $"{label.LabelText}\nParallel\n{{{string.Join(", ", nodes.Via.OrderBy(l => l.Order))}}}";
                };
                KnotSubgraphDecorator = (dotEdge, nodes, label, flags, options) =>
                {
                    dotEdge.Label = $"{label.LabelText}\nKnot\n{{{string.Join(", ", nodes.Via.OrderBy(l => l.Order))}}}";
                };
            }

            public ClassificationVariation Variation { get; set; }
        }


        [Test]
        public void Example01()
        {
            var graphOption = new Action<Dot.RootGraph>(graph =>
            {
                graph.RankSep = 0.02;
                graph.NewRank = true;
                graph.Viewport = new Dot.Viewport
                {
                    Width = 500,
                    Height = 500,
                    Zoom = 0.7
                };

                graph.Ranks.Add(new Dot.Rank(Dot.RankType.source) { 0 });
                graph.Ranks.Add(new Dot.Rank(Dot.RankType.sink) { 6 });
                graph.Ranks.Add(new Dot.Rank(Dot.RankType.same) { 2, 5 });
            });

            var fileses = RecordGrouping(new[]
                {
                    new RecOptions
                    {
                        FitToLast = true,
                        HighlightLatests = true,
                        HighlightBelongingEdges = true,
                        NameFactory = (test, pattern, step) => $"animation-step-{step:D4}",
                        OptionConfigurer = graphOption,
                    },
                    new RecOptions
                    {
                        FitToLast = true,
                        SnapshotFlags = SnapshotFlags.Initial | SnapshotFlags.Last,
                        NameFactory = (test, pattern, step) => $"original-step-{step:D4}",
                        OptionConfigurer = graphOption
                    },
                    new RecOptions
                    {
                        FitToLast = true,
                        SnapshotFlags = SnapshotFlags.Last,
                        NameFactory = (test, pattern, step) => $"integrated-step-{step:D4}",
                        OptionConfigurer = graphOption,
                        Variation = ClassificationVariation.Integrated
                    },
                    new RecOptions
                    {
                        FitToLast = true,
                        SnapshotFlags = SnapshotFlags.Last,
                        NameFactory = (test, pattern, step) => $"unique-step-{step:D4}",
                        OptionConfigurer = graphOption,
                        Variation = ClassificationVariation.Unique
                    },
                },
                (from: 0, to: 1),
                (from: 1, to: 2),
                (from: 1, to: 3),
                (from: 2, to: 3),
                (from: 2, to: 4),
                (from: 3, to: 4),
                (from: 4, to: 6),
                (from: 0, to: 5),
                (from: 5, to: 6),
                (from: 0, to: 6)
                );

            File.Copy(fileses.ElementAt(1).First(), Path.Combine(PathUtil.SolutionDir, "docs", "images", "example01-original-graph.png"), true);
            File.Copy(fileses.ElementAt(1).Last(), Path.Combine(PathUtil.SolutionDir, "docs", "images", "example01-labeled-graph.png"), true);

            File.Copy(fileses.ElementAt(2).Last(), Path.Combine(PathUtil.SolutionDir, "docs", "images", "example01-labeled-graph-integrated.png"), true);
            File.Copy(fileses.ElementAt(3).Last(), Path.Combine(PathUtil.SolutionDir, "docs", "images", "example01-labeled-graph-unique.png"), true);

            var gifPath = Path.Combine(PathUtil.SolutionDir, "docs", "images", "example01-steps.gif");
            Visualizer.GenerateAnimation(fileses.ElementAt(0), gifPath);
        }


        [Test]
        public void Series01()
        {
            var files = RecordGrouping(new[]{
                    new RecOptions
                    {
                        //HideLabels = true,
                        NameFactory = (test, pattern, step) => $"default-step-{step:D4}",
                        OptionConfigurer = graph =>
                        {
                            graph.RankDir = Dot.RankDir.LR;
                        },
                    },
                    new RecOptions
                    {
                        //HideLabels = true,
                        NameFactory = (test, pattern, step) => $"unique-step-{step:D4}",
                        OptionConfigurer = graph =>
                        {
                            graph.RankDir = Dot.RankDir.LR;
                        },
                        Variation = ClassificationVariation.Unique
                    },
                    new RecOptions
                    {
                        //HideLabels = true,
                        NameFactory = (test, pattern, step) => $"integrated-step-{step:D4}",
                        OptionConfigurer = graph =>
                        {
                            graph.RankDir = Dot.RankDir.LR;
                        },
                        Variation = ClassificationVariation.Integrated
                    }
                },
                (from: 0, to: 1),
                (from: 1, to: 2),
                (from: 2, to: 3)
                );

            File.Copy(files.ElementAt(0).Last(), Path.Combine(PathUtil.SolutionDir, "docs", "images", "subgraph-series.png"), true);
            File.Copy(files.ElementAt(1).Last(), Path.Combine(PathUtil.SolutionDir, "docs", "images", "subgraph-series-unique.png"), true);
            File.Copy(files.ElementAt(2).Last(), Path.Combine(PathUtil.SolutionDir, "docs", "images", "subgraph-series-integrated.png"), true);
        }

        [Test]
        public void Parallel01()
        {
            var files = RecordGrouping(new RecOptions
            {
                //HideLabels = true,
                OptionConfigurer = graph =>
                {
                    graph.RankSep = 0.02;
                    graph.NewRank = true;
                },
            },
                (from: 0, to: 1),
                (from: 0, to: 1),
                (from: 0, to: 1),
                (from: 0, to: 1)
                );

            File.Copy(files.Last(), Path.Combine(PathUtil.SolutionDir, "docs", "images", "subgraph-parallel.png"), true);
        }

        [Test]
        public void Knot01()
        {
            var files = RecordGrouping(new RecOptions
            {
                //HideLabels = true,
                OptionConfigurer = graph =>
                {
                    graph.RankSep = 0.02;
                    graph.NewRank = true;
                    //graph.Splines = Dot.Splines.polyline;
                    graph.Ranks.Add(new Dot.Rank(Dot.RankType.same) { 1, 2 });
                },
            },
                (from: 0, to: 1),
                (from: 0, to: 2),
                (from: 1, to: 2),
                (from: 1, to: 3),
                (from: 2, to: 3)
                );

            File.Copy(files.Last(), Path.Combine(PathUtil.SolutionDir, "docs", "images", "subgraph-knot.png"), true);
        }

        private IEnumerable<string> RecordGrouping(params Input[] inputs)
            => RecordGrouping(new RecOptions(), inputs.AsEnumerable());


        private IEnumerable<string> RecordGrouping(IEnumerable<Input> inputs)
            => RecordGrouping(new RecOptions(), inputs.AsEnumerable());


        private IEnumerable<IEnumerable<string>> RecordGrouping(IEnumerable<RecOptions> options, params Input[] inputs)
            => options.Select(opt => RecordGrouping(opt, inputs)).ToList();

        private IEnumerable<string> RecordGrouping(RecOptions options, params Input[] inputs)
            => RecordGrouping(options, inputs.AsEnumerable());

        private IEnumerable<string> RecordGrouping(RecOptions options, IEnumerable<Input> inputs)
        {
            var outputPaths = new List<string>();

            foreach (var pattern in Pattern.Generate(inputs, 0, options.Versions))
            {
                var env = pattern.Version switch
                {
                    ImplementationVersions.Latest => new RecordGraphEnvironment() as ITestGraphEnvironment,
                    ImplementationVersions.V03 => new Test.v03.RecordGraphEnvironment(),
                    _ => throw new Exception()
                };

                var graph = new Graph(pattern.Nodes, pattern.Edges);

                var outputDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "record-images", TestContext.CurrentContext.Test.Name);

                Directory.CreateDirectory(outputDir);

                var recorder = new Recorder<Graph, Node, Edge, RecordLabel>(node => node.Id.ToString(), options, outputDir);

                var locals = recorder.Record(pattern.Id, pattern.EdgeWithNodePair, rec =>
                {
                    switch (options.Variation)
                    {
                        case ClassificationVariation.Default:
                            env.Classify(graph, pattern.StartNodes,
                                (g, t, e, f, l) => rec.LabelEdge(g, t, e, f, (RecordLabel)l),
                                (g, s, sl, e, l) => rec.LabelSeries(g, s, (RecordLabel)sl, e, (RecordLabel)l),
                                (g, s, sls, e, l) => rec.LabelParallel(g, s, sls.OfType<RecordLabel>(), e, (RecordLabel)l),
                                (g, s, sls, e, l) => rec.LabelKnot(g, s, sls.OfType<RecordLabel>(), e, (RecordLabel)l));
                            break;
                        case ClassificationVariation.Integrated:
                            env.ClassifyIntegratedly(graph, pattern.StartNodes,
                                (g, t, e, f, l) => rec.LabelEdge(g, t, e, f, (RecordLabel)l),
                                (g, s, sl, e, l) => rec.LabelSeries(g, s, (RecordLabel)sl, e, (RecordLabel)l),
                                (g, s, sls, e, l) => rec.LabelParallel(g, s, sls.OfType<RecordLabel>(), e, (RecordLabel)l),
                                (g, s, sls, e, l) => rec.LabelKnot(g, s, sls.OfType<RecordLabel>(), e, (RecordLabel)l));
                            break;
                        case ClassificationVariation.Unique:
                            env.ClassifyUniquely(graph, pattern.StartNodes,
                                (g, t, e, f, l) => rec.LabelEdge(g, t, e, f, (RecordLabel)l),
                                (g, s, sls, e, l) => rec.LabelSeries(g, s, sls.OfType<RecordLabel>(), e, (RecordLabel)l),
                                (g, s, sls, e, l) => rec.LabelParallel(g, s, sls.OfType<RecordLabel>(), e, (RecordLabel)l),
                                (g, s, sls, e, l) => rec.LabelKnot(g, s, sls.OfType<RecordLabel>(), e, (RecordLabel)l));
                            break;
                    }
                });

                outputPaths.AddRange(locals);
            }
            return outputPaths;
        }


    }
}
