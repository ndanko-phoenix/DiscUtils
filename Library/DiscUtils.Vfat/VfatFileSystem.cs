using System;
using System.IO;
using DiscUtils.Fat;
using DiscUtils.Streams;
using System.Collections.Generic;
using Directory = DiscUtils.Fat.Directory;

namespace DiscUtils.Vfat
{
    public class VfatFileSystem : FatFileSystem
    {
        private IDictionary<string, int> _dicPathToIndex;
        private IDictionary<string, ISet<string>> _dicUsedShortNamesPerDir;

        public VfatFileSystem(Stream data)
            : base(data, new VfatFileSystemOptions())
        {
            _dicPathToIndex = new Dictionary<string, int>();
            _dicUsedShortNamesPerDir = new Dictionary<string, ISet<string>>();
        }

        public override SparseStream OpenFile(string path, FileMode mode, FileAccess access)
        {
            VfatDirectory parent;
            long entryId;
            try
            {
                entryId = GetDirectoryEntry(RootDir, path, out Directory p);
                parent = (VfatDirectory)p;
            }
            catch (ArgumentException ex)
            {
                throw new IOException("Invalid path: " + path, ex);
            }

            if (parent == null)
            {
                throw new FileNotFoundException("Could not locate file", path);
            }

            if (entryId < 0)
            {
                return parent.OpenFile(VfatFileName.FromPath(path, this), mode, access);
            }

            DirectoryEntry dirEntry = parent.GetEntry(entryId);

            if ((dirEntry.Attributes & FatAttributes.Directory) != 0)
            {
                throw new IOException("Attempted to open directory \"" + path + "\" as a file");
            }
            return parent.OpenFile(dirEntry.Name, mode, access);
        }

        internal override Directory DirectoryFactory(Directory parent, long parentId)
        {
            return new VfatDirectory(parent, parentId);
        }

        internal override Directory DirectoryFactory(Stream fatStream)
        {
            return new VfatDirectory(this, fatStream);
        }

        internal override FileName FileNameFactory(string name, string strParentDirPath)
        {
            return VfatFileName.FromName(name, this, GetUniqueIndex(name, strParentDirPath));
        }

        public int GetUniqueIndex(string name, string strParentDirPath)
        {
            string strFullPath = strParentDirPath + @"\" + name;
            
            if (_dicPathToIndex.ContainsKey(strFullPath))
                return _dicPathToIndex[strFullPath];

            if (!_dicUsedShortNamesPerDir.ContainsKey(strParentDirPath))
                _dicUsedShortNamesPerDir.Add(strParentDirPath, new HashSet<string>());

            for (int nIndex = 1; nIndex < VfatFileName.INDEX_MAX; nIndex++) {
                string strShortName = VfatFileName.GetShortName(name, nIndex);
                if (!_dicUsedShortNamesPerDir[strParentDirPath].Contains(strShortName)) {
                    _dicUsedShortNamesPerDir[strParentDirPath].Add(strShortName);
                    _dicPathToIndex.Add(strFullPath, nIndex);
                    return nIndex;
                }
            }

            throw new IOException(string.Format("There are over {0} files with names similar to \"{1}\" in \"{2}\"",
                VfatFileName.INDEX_MAX, name, strParentDirPath));
        }
    }
}
