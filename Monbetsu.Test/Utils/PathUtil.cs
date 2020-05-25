using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Monbetsu.Test
{
    class PathUtil
    {
        private static string? solutionDir;

        public static string SolutionDir => solutionDir ??= GetSolutionDir();

        public static string LocalJekyllPath
        {
            get
            {
                var path = @"F:\monbetsu-pages";
                if (!Directory.Exists(path))
                {
                    throw new DirectoryNotFoundException(path);
                }
                return path;
            }
        }

        public static string GetSolutionItemPath(params string[] tree)
        {
            return Path.Combine(tree.Prepend(SolutionDir).ToArray());
        }

        private static string GetSolutionDir([CallerFilePath]string? path = null)
        {
            var dir = Path.GetDirectoryName(path);

            while (dir != Path.GetPathRoot(dir))
            {
                if (File.Exists(Path.Combine(dir!, "Monbetsu.sln")))
                {
                    return dir!;
                }

                dir = Path.GetDirectoryName(dir);
            }

            throw new FileNotFoundException("can not find the solution file.");
        }

        internal static bool TryGetGifGenPath(out string? path)
        {
            var gifGenDir = Path.Combine(SolutionDir, "tools", "GifGen", "bin");
            
            path = null;

            if (Directory.Exists(gifGenDir))
            {
                path = Directory.EnumerateFiles(gifGenDir, "GifGen.exe", SearchOption.AllDirectories).FirstOrDefault();
            }

            if (path == null)
            {
                var gifGenProjDir = Path.Combine(SolutionDir, "tools", "GifGen");
                if (Directory.Exists(gifGenProjDir))
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = "dotnet",
                        Arguments = Arguments("build"),
                        WorkingDirectory = gifGenProjDir,
                    };
                    Process.Start(startInfo).WaitForExit();

                    if (Directory.Exists(gifGenDir))
                    {
                        path = Directory.EnumerateFiles(gifGenDir, "GifGen.exe", SearchOption.AllDirectories).FirstOrDefault();
                    }
                }
            }

            return path != null;
        }

        internal static string Arguments(params string[] args)
        {
            return string.Join(" ", args.Select(arg => arg.Contains(' ') ? $"\"{arg}\"" : arg));
        }

        internal static void CopyDicrectory(string sourceDirPath, string destinationDirPath, Func<string[], int, bool>? filter = null)
        {
            if (!Directory.Exists(sourceDirPath))
            {
                throw new DirectoryNotFoundException($"{sourceDirPath} was not found.");
            }
            if (!Directory.Exists(destinationDirPath))
            {
                throw new DirectoryNotFoundException($"{destinationDirPath} was not found.");
            }

            sourceDirPath = new DirectoryInfo(sourceDirPath).FullName;
            sourceDirPath = Path.TrimEndingDirectorySeparator(sourceDirPath);
            destinationDirPath = new DirectoryInfo(destinationDirPath).FullName;
            destinationDirPath = Path.TrimEndingDirectorySeparator(destinationDirPath);

            Span<byte> srcBuf = stackalloc byte[1024 * 8];
            Span<byte> destBuf = stackalloc byte[1024 * 8];

            var baseSourceParts = sourceDirPath.Split(Path.DirectorySeparatorChar).Length;

            foreach (var fs in Directory.EnumerateFileSystemEntries(sourceDirPath, "*", SearchOption.AllDirectories).OrderBy(_ => _))
            {
                var relPath = Path.GetRelativePath(sourceDirPath, fs);

                if (relPath == fs || relPath.StartsWith(".."))
                {
                    continue;
                }

                if (filter != null)
                {
                    var sourceParts = fs.Split(Path.DirectorySeparatorChar);
                    if (!filter(sourceParts, baseSourceParts))
                    {
                        continue;
                    }
                }

                var destPath = Path.Combine(destinationDirPath, relPath);

                if (Directory.Exists(fs))
                {
                    if (!Directory.Exists(destPath))
                    {
                        Directory.CreateDirectory(destPath);
                    }
                }
                else if (File.Exists(fs))
                {
                    if (File.Exists(destPath))
                    {
                        using var src = File.OpenRead(fs);
                        using var dest = File.OpenRead(destPath);

                        if (src.Length == dest.Length)
                        {
                            var match = true;
                            while(src.Position < src.Length)
                            {
                                var readSize = src.Read(srcBuf);
                                dest.Read(destBuf);

                                if (!srcBuf.SequenceEqual(destBuf))
                                {
                                    match = false;
                                    break;
                                }
                            }
                            if (match)
                            {
                                continue;
                            }
                        }
                    }

                    File.Copy(fs, destPath, true);
                }
            }
        }
    }
}
