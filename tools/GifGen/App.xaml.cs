using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GifGen
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static void Generate(IEnumerable<string> sourcePaths, string outputPath)
        {
            var encoder = new GifBitmapEncoder();
            
            foreach (var imagePath in sourcePaths)
            {
                var source = BitmapFrame.Create(new Uri(imagePath));

                var toGifEncoder = new GifBitmapEncoder();
                toGifEncoder.Frames.Add(source);

                var sourceStream = new MemoryStream();
                toGifEncoder.Save(sourceStream);
                sourceStream.Position = 0;

                var decoder = new GifBitmapDecoder(sourceStream, BitmapCreateOptions.None, BitmapCacheOption.None);

                var frame = BitmapFrame.Create(decoder.Frames[0], null, decoder.Frames[0].Metadata.Clone() as BitmapMetadata, null);

                if (frame.Metadata is BitmapMetadata meta)
                {
                    meta.SetQuery("/grctlext/Delay", (ushort)200);
                    meta.SetQuery("/appext/Application", Encoding.UTF8.GetBytes("NETSCAPE2.0"));
                    meta.SetQuery("/appext/Data", new byte[] { 3, 1, 0, 0 });

                    meta.Freeze();
                }

                frame.Freeze();

                DumpMetadata("frame", frame.Metadata as BitmapMetadata);

                encoder.Frames.Add(frame);
            }

            // hack! to save frame metadata for delays and loop of animation
            var f = typeof(BitmapEncoder).GetField("_supportsFrameMetadata", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            f!.SetValue(encoder, true);

            using (var outputStream = new FileStream(outputPath, FileMode.OpenOrCreate))
            {
                encoder.Save(outputStream);
            }
            
            var check = new GifBitmapDecoder(new Uri(outputPath), BitmapCreateOptions.None, BitmapCacheOption.None);
            DumpMetadata("check", check.Metadata as BitmapMetadata);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var args = Environment.GetCommandLineArgs();

            Generate(args.Skip(2), args[1]);

            Environment.Exit(0);
        }

        internal static void DumpMetadata(string prefix, BitmapMetadata? meta, int indent = 0)
        {
            if (meta == null)
            {
                Debug.WriteLine($"{new string(' ', indent * 2)}{prefix}: empty metadata");
                return;
            }

            var encoding = Encoding.GetEncoding("UTF-8", EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback);

            foreach (var key in meta)
            {
                var val = meta.GetQuery(key);
                Debug.WriteLine($"{new string(' ', indent * 2)}{prefix}: {key}={val}");

                if (val is BitmapMetadata child)
                {
                    DumpMetadata($"{prefix}{key}", child, indent + 1);
                }
                else if (val is byte[] bytes)
                {
                    try
                    {
                        var text = encoding.GetString(bytes);

                        var cc = text.Select(c => (char?)c).FirstOrDefault(c => c < ' ' && c != '\t' && c != '\r' && c != '\n');

                        if (text.Any(c => c < ' ' && c != '\t' && c != '\r' && c != '\n'))
                        {
                            throw new Exception("not text");
                        }
                        Debug.WriteLine($"{new string(' ', indent * 2 + 2)}text([{bytes.Length}])={text}");
                    }
                    catch
                    {
                        Debug.WriteLine($"{new string(' ', indent * 2 + 2)}binary([{bytes.Length}])={string.Join(" ", bytes.Take(16).Select(b => b.ToString("X2")))}");
                    }
                }
            }
        }
    }
}
