using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using AltMstest.Core.Configuration;

namespace AltMstest.Core
{
    public static class FolderSync
    {
        public static IList<ISyncedDestination> Sync(string destination, IList<AssemblyConfigElement> assemblies, CancellationToken ct)
        {
            var dests = new List<ISyncedDestination>();

            if (destination == null)
            {
                throw new InvalidDataException("A destination is required. No destination is specified.");
            }

            if (!Directory.Exists(destination))
            {
                Directory.CreateDirectory(destination);
            }

            foreach (AssemblyConfigElement assembly in assemblies)
            {
                if (ct.IsCancellationRequested)
                    return new List<ISyncedDestination>();

                // Copy everything from folder.Folder
                var sourceDir = new DirectoryInfo(assembly.Folder);

                var destinationName = GetDestinationName(assembly.Folder);
                string destinationFullPath = Path.Combine(destination, destinationName);

                DirectoryInfo destDir = !Directory.Exists(destinationFullPath)
                                            ? Directory.CreateDirectory(destinationFullPath)
                                            : new DirectoryInfo(destinationFullPath);

                SyncDirectory(sourceDir, destDir, ct);

                var dest = new SyncedDestination();

                string ass = assembly.FileName;

                string assemblyFullPath = Path.Combine(destinationFullPath, ass);
                dest.AddAssembly(ass, assemblyFullPath, assembly.RunParallel, assembly.DegreeOfParallelism);

                dests.Add(dest);
            }

            return dests;
        }

        private static void SyncDirectory(DirectoryInfo sourceDir, DirectoryInfo destDir, CancellationToken ct)
        {
            // Sync sub folders recursively
            foreach (DirectoryInfo sourceSubFolder in sourceDir.GetDirectories())
            {
                if (ct.IsCancellationRequested)
                    return;

                var destSubFolderName = Path.Combine(destDir.FullName, sourceSubFolder.Name);

                var destSubFolder = !Directory.Exists(destSubFolderName)
                                        ? Directory.CreateDirectory(destSubFolderName)
                                        : new DirectoryInfo(destSubFolderName);

                SyncDirectory(sourceSubFolder, destSubFolder, ct);
            }

            var destFiles = destDir.GetFiles().ToDictionary(x => x.Name);
            foreach (FileInfo sourceFile in sourceDir.GetFiles())
            {
                if (ct.IsCancellationRequested)
                    return;

                FileInfo existingDestFile;
                if (destFiles.TryGetValue(sourceFile.Name, out existingDestFile))
                {
                    // Don't need to update files that are already the same.
                    if (existingDestFile.LastWriteTime == sourceFile.LastWriteTime)
                        continue;
                }

                string destFileName = Path.Combine(destDir.FullName, sourceFile.Name);
                sourceFile.CopyTo(destFileName, true);
            }
        }

        private static string GetDestinationName(string folder)
        {
            var folders = folder.Split('\\');
            return String.Join("_", folders.Skip(1));
        }

        private class SyncedDestination : ISyncedDestination
        {
            private readonly Dictionary<string, AssemblyInfo> _ass;
            
            public SyncedDestination()
            {
                _ass = new Dictionary<string, AssemblyInfo>();
            }

            public IList<AssemblyInfo> AssembliesWithFullPath
            {
                get { return _ass.Values.ToList(); }
            }

            public void AddAssembly(string fileName, string assemblyFullPath, bool parallel, int? degreeOfParallelism)
            {
                _ass.Add(fileName, new AssemblyInfo(assemblyFullPath, parallel, degreeOfParallelism));
            }
        }
    }
}