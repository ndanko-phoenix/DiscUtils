using System;
using System.IO;
using DiscUtils.Fat;
using DiscUtils.Streams;
using Directory = DiscUtils.Fat.Directory;

namespace DiscUtils.Vfat
{
    internal class VfatDirectory : Directory
    {
        public VfatDirectory(Directory parent, long parentId)
            : base(parent, parentId)
        {
        }

        public VfatDirectory(FatFileSystem fileSystem, Stream fatStream)
            : base(fileSystem, fatStream)
        {
        }

        private void CreateLFNEntries(VfatFileName name) {
            for (int nPart = name.LastPart; nPart >= 0; nPart--) {
                VfatDirectoryEntry entry = new VfatDirectoryEntry(nPart, (VfatFileSystemOptions)FileSystem.FatOptions,
                    name, FileSystem.FatVariant);
                AddEntry(entry);
            }
        }

        internal override SparseStream OpenFile(FileName name, FileMode mode, FileAccess fileAccess)
        {
            long fileId = FindEntry(name);
            bool exists = fileId != -1;

            if ((mode == FileMode.OpenOrCreate || mode == FileMode.CreateNew || mode == FileMode.Create) && !exists)
            {
                CreateLFNEntries((VfatFileName)name);

                // Create new file
                var newEntry = new DirectoryEntry(FileSystem.FatOptions, name, FatAttributes.Archive, FileSystem.FatVariant);
                newEntry.FirstCluster = 0; // i.e. Zero-length
                newEntry.CreationTime = FileSystem.ConvertFromUtc(DateTime.UtcNow);
                newEntry.LastWriteTime = newEntry.CreationTime;

                fileId = AddEntry(newEntry);
            }

            return new FatFileStream(FileSystem, this, fileId, fileAccess);
        }

        internal override Directory CreateChildDirectory(FileName name) {
            CreateLFNEntries((VfatFileName)name);
            return base.CreateChildDirectory(name);
        }
    }
}
