using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Monbetsu.Test.Utils
{
    class Visualizer
    {
        internal static Dot.RgbaColor EdgeColor = new Dot.RgbaColor(0, 0, 0, 255);
        internal static Dot.RgbaColor SeriesColor = new Dot.RgbaColor(0, 200, 0, 255);
        internal static Dot.RgbaColor ParallelColor = new Dot.RgbaColor(255, 0, 255, 255);
        internal static Dot.RgbaColor KnotColor = new Dot.RgbaColor(0, 0, 255, 255);
        internal static Dot.RgbaColor ErrorColor = new Dot.RgbaColor(255, 0, 0, 255);


        internal static bool HaveFailedToOutputDot { get; private set; } = false;
    
        [Conditional("VISUALIZED")]
        internal static void GenerateImageFromDot(string dotCode, string outputPath)
        {
            Debug.WriteLine($"// output dot image to: {outputPath}");
            Debug.WriteLine(dotCode);

#if VISUALIZED
            if (!HaveFailedToOutputDot)
            {
                try
                {
                    using var graphViz = new Togurakamiyamada.Client.GraphVizClient(new Uri("http://localhost:50001"));
                    graphViz.SendAndSave(new Togurakamiyamada.Client.GraphVizRequest
                    {
                        Layout = "dot",
                        Formats = new[]
                        {
                            new Togurakamiyamada.Client.GraphVizFormat("png")
                        },
                        DotCode = dotCode
                    }, outputPath).Wait();

                }
                catch (Togurakamiyamada.Client.GraphVizClientException exception) when(exception.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    Debug.WriteLine($"// failed to output dot image to: {outputPath}");
                }
                catch(Exception exception)
                {
                    Debug.WriteLine($"// failed to output dot image to: {outputPath}");
                    Debug.WriteLine(exception);
                    HaveFailedToOutputDot = true;
                }

                if (File.Exists(outputPath))
                {
                    TestContext.AddTestAttachment(outputPath);
                }
            }
            else
            {
                Debug.WriteLine($"// failed to output dot image to: {outputPath}");
            }
#else
            HaveFailedToOutputDot = true;
#endif
        }

        [Conditional("VISUALIZED")]
        internal static void GenerateAnimation(IEnumerable<string> inputFilePaths, string outputPath)
        {
            if (HaveFailedToOutputDot)
            {
                return;
            }

            static string Quote(string s) => $"\"{s}\"";

            if (PathUtil.TryGetGifGenPath(out var gifGenPath))
            {
                var process = Process.Start(Quote(gifGenPath!), string.Join(" ", inputFilePaths.Prepend(outputPath).Select(Quote)));

                process.WaitForExit();
            }
        }
    }
}
