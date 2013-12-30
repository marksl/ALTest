using System;
using System.IO;
using System.Linq;
using AltMstestGui.Configuration;

namespace AltMstestGui
{
    public static class FolderSync
    {

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

        static string GetDestinationName(string folder)
        {
            var folders = folder.Split('\\');
            return String.Join("_", folders.Skip(1));
        }

        public static void Sync(AltMstestSection serviceConfigSection)
        {
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
                string fullPath = Path.Combine(destination, destinationName);

                DirectoryInfo destDir = !Directory.Exists(fullPath)
                    ? Directory.CreateDirectory(fullPath)
                    : new DirectoryInfo(fullPath);

                SyncDirectory(sourceDir, destDir);
            }
        }
    }
}
