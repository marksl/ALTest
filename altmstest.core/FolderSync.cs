using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AltMstest.Core.Configuration;

namespace AltMstest.Core
{
    public static class FolderSync
    {
        public static IList<ISyncedDestination> Sync(AltMstestSection serviceConfigSection)
        {
            var dests = new List<ISyncedDestination>();

            string destination = serviceConfigSection.Destination;
            if (destination == null)
            {
                throw new InvalidDataException("A destination is required. No destination is specified.");
            }

            if (!Directory.Exists(destination))
            {
                Directory.CreateDirectory(destination);
            }

            foreach (AssemblyConfigElement assembly in serviceConfigSection.Assemblies)
            {
                // Copy everything from folder.Folder
                var sourceDir = new DirectoryInfo(assembly.Folder);

                var destinationName = GetDestinationName(assembly.Folder);
                string destinationFullPath = Path.Combine(destination, destinationName);

                DirectoryInfo destDir = !Directory.Exists(destinationFullPath)
                                            ? Directory.CreateDirectory(destinationFullPath)
                                            : new DirectoryInfo(destinationFullPath);

                SyncDirectory(sourceDir, destDir);

                var dest = new SyncedDestination();

                string ass = assembly.FileName;

                string assemblyFullPath = Path.Combine(destinationFullPath, ass);
                dest.AddAssembly(ass, assemblyFullPath);
                

                dests.Add(dest);
            }

            return dests;
        }

        private static void SyncDirectory(DirectoryInfo sourceDir, DirectoryInfo destDir)
        {
            // Sync sub folders recursively
            foreach (DirectoryInfo sourceSubFolder in sourceDir.GetDirectories())
            {
                var destSubFolderName = Path.Combine(destDir.FullName, sourceSubFolder.Name);

                var destSubFolder = !Directory.Exists(destSubFolderName)
                                        ? Directory.CreateDirectory(destSubFolderName)
                                        : new DirectoryInfo(destSubFolderName);

                SyncDirectory(sourceSubFolder, destSubFolder);
            }

            var destFiles = destDir.GetFiles().ToDictionary(x => x.Name);
            foreach (FileInfo sourceFile in sourceDir.GetFiles())
            {
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
            private readonly Dictionary<string, string> _ass;
            
            public SyncedDestination()
            {
                _ass = new Dictionary<string, string>();
            }

            public IList<string> AssemblyNames
            {
                get { return _ass.Keys.ToList(); }
            }

            public IList<string> AssembliesWithFullPath
            {
                get { return _ass.Values.ToList(); }
            }

            public IList<string> GetAssembliesWithFullPath(IList<string> assemblyNames)
            {
                var assembliesWithFullPaths = new List<string>();
                foreach (var key in assemblyNames)
                {
                    AssembliesWithFullPath.Add(_ass[key]);
                }
                return assembliesWithFullPaths;
            }

            public void AddAssembly(string fileName, string assemblyFullPath)
            {
                _ass.Add(fileName, assemblyFullPath);
            }
        }
    }
}