using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AltMstestGui.Configuration;

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

            foreach (FolderConfigElement folder in serviceConfigSection.Folders)
            {
                // Copy everything from folder.Folder
                var sourceDir = new DirectoryInfo(folder.Folder);

                var destinationName = GetDestinationName(folder.Folder);
                string destinationFullPath = Path.Combine(destination, destinationName);

                DirectoryInfo destDir = !Directory.Exists(destinationFullPath)
                                            ? Directory.CreateDirectory(destinationFullPath)
                                            : new DirectoryInfo(destinationFullPath);

                SyncDirectory(sourceDir, destDir);

                var dest = new SyncedDestination();

                foreach (string ass in folder.AssemblyNames)
                {
                    string assemblyFullPath = Path.Combine(destinationFullPath, ass);
                    dest.AddAssembly(assemblyFullPath);
                }

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
            private readonly List<string> _ass;

            public SyncedDestination()
            {
                _ass = new List<string>();
            }

            public IList<string> AssembliesWithFullPath
            {
                get { return _ass; }
            }

            public void AddAssembly(string assemblyFullPath)
            {
                _ass.Add(assemblyFullPath);
            }
        }
    }
}