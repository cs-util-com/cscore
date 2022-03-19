namespace ZetaLongPaths
{
    using FileAttributes = Native.FileAttributes;

// ReSharper disable once UseNameofExpression
    [DebuggerDisplay(@"{FullName}")]
    public class ZlpDirectoryInfo : IZlpFileSystemInfo
    {
        public static ZlpDirectoryInfo GetTemp()
        {
            return new(ZlpPathHelper.GetTempDirectoryPath());
        }

        [PublicAPI]
        public static ZlpDirectoryInfo GetFolderPath(
            Environment.SpecialFolder specialFolder)
        {
            return new(Environment.GetFolderPath(specialFolder));
        }

        [PublicAPI]
        public static ZlpDirectoryInfo GetFolderPath(
            Environment.SpecialFolder specialFolder,
            Environment.SpecialFolderOption option)
        {
            return new(Environment.GetFolderPath(specialFolder, option));
        }

        public ZlpDirectoryInfo(string path)
        {
            FullName = path;
        }

        [PublicAPI]
        public ZlpDirectoryInfo(DirectoryInfo path)
        {
            FullName = path?.FullName;
        }

        [PublicAPI]
        public ZlpDirectoryInfo(ZlpDirectoryInfo path)
        {
            FullName = path?.FullName;
        }

        [PublicAPI]
        public static ZlpDirectoryInfo FromOther(ZlpDirectoryInfo path)
        {
            return new(path);
        }

        [PublicAPI]
        public static ZlpDirectoryInfo FromString(string path)
        {
            return new(path);
        }

        [PublicAPI]
        public static ZlpDirectoryInfo FromBuiltIn(DirectoryInfo path)
        {
            return new(path);
        }

        [PublicAPI]
        public DirectoryInfo ToBuiltIn()
        {
            return new(FullName);
        }

        [PublicAPI]
        public ZlpDirectoryInfo ToOther()
        {
            return Clone();
        }

        [PublicAPI]
        public void Refresh()
        {
        }

        public string Extension => ZlpPathHelper.GetExtension(FullName);

        [PublicAPI]
        public ZlpDirectoryInfo Clone()
        {
            return new(FullName);
        }

        [PublicAPI]
        public ZlpDirectoryInfo GetFullPath()
        {
            return new(ZlpPathHelper.GetFullPath(FullName));
        }

        public bool Exists => ZlpIOHelper.DirectoryExists(FullName);

        [PublicAPI] public bool IsEmpty => ZlpIOHelper.DirectoryIsEmpty(FullName);

        public void MoveToRecycleBin()
        {
            ZlpIOHelper.MoveDirectoryToRecycleBin(FullName);
        }

        public string OriginalPath => FullName;

        public override string ToString()
        {
            return FullName;
        }

        public void Delete()
        {
            ZlpIOHelper.DeleteDirectory(FullName, false);
        }

        public void Delete(bool recursive)
        {
            ZlpIOHelper.DeleteDirectory(FullName, recursive);
        }

        [PublicAPI]
        public void DeleteContents(bool recursive)
        {
            ZlpIOHelper.DeleteDirectoryContents(FullName, recursive);
        }

        public void Create()
        {
            ZlpIOHelper.CreateDirectory(FullName);
        }

        /// <summary>
        /// The destination folder may not exists yet, otherwise an error 183 will be thrown.
        /// </summary>
        public void MoveTo(string destinationDirectoryPath)
        {
            ZlpIOHelper.MoveDirectory(FullName, destinationDirectoryPath);
        }

        /// <summary>
        /// The destination folder may not exists yet, otherwise an error 183 will be thrown.
        /// </summary>
        public void MoveTo(string destinationDirectoryPath, bool writeThrough)
        {
            ZlpIOHelper.MoveDirectory(FullName, destinationDirectoryPath, writeThrough);
        }

        /// <summary>
        /// The destination folder may not exists yet, otherwise an error 183 will be thrown.
        /// </summary>
        public void MoveTo(ZlpDirectoryInfo destinationDirectoryPath, bool writeThrough = false)
        {
            ZlpIOHelper.MoveDirectory(FullName, destinationDirectoryPath.FullName, writeThrough);
        }

        public string FullName { get; }

        public string Name => ZlpPathHelper.GetDirectoryNameOnlyFromFilePath(FullName);

        public ZlpFileInfo[] GetFiles()
        {
            return ZlpIOHelper.GetFiles(FullName);
        }

        [PublicAPI]
        public ZlpFileInfo[] GetFiles(string pattern)
        {
            return ZlpIOHelper.GetFiles(FullName, pattern);
        }

        [PublicAPI]
        public ZlpFileInfo[] GetFiles(string pattern, SearchOption searchOption)
        {
            return ZlpIOHelper.GetFiles(FullName, pattern, searchOption);
        }

        [PublicAPI]
        public ZlpFileInfo[] GetFiles(SearchOption searchOption)
        {
            return ZlpIOHelper.GetFiles(FullName, searchOption);
        }

        public IZlpFileSystemInfo[] GetFileSystemInfos()
        {
            return ZlpIOHelper.GetFileSystemInfos(FullName);
        }

        [PublicAPI]
        public IZlpFileSystemInfo[] GetFileSystemInfos(string pattern)
        {
            return ZlpIOHelper.GetFileSystemInfos(FullName, pattern);
        }

        [PublicAPI]
        public IZlpFileSystemInfo[] GetFileSystemInfos(string pattern, SearchOption searchOption)
        {
            return ZlpIOHelper.GetFileSystemInfos(FullName, pattern, searchOption);
        }

        public IZlpFileSystemInfo[] GetFileSystemInfos(SearchOption searchOption)
        {
            return ZlpIOHelper.GetFileSystemInfos(FullName, searchOption);
        }

        public ZlpDirectoryInfo[] GetDirectories()
        {
            return ZlpIOHelper.GetDirectories(FullName);
        }

        [PublicAPI]
        public ZlpDirectoryInfo[] GetDirectories(string pattern)
        {
            return ZlpIOHelper.GetDirectories(FullName, pattern);
        }

        [PublicAPI]
        public ZlpDirectoryInfo[] GetDirectories(SearchOption searchOption)
        {
            return ZlpIOHelper.GetDirectories(FullName, searchOption);
        }

        [PublicAPI]
        public ZlpDirectoryInfo[] GetDirectories(string pattern, SearchOption searchOption)
        {
            return ZlpIOHelper.GetDirectories(FullName, pattern, searchOption);
        }

        [PublicAPI]
        public ZlpDirectoryInfo CreateSubdirectory(string name)
        {
            var path = ZlpPathHelper.Combine(FullName, name);
            ZlpIOHelper.CreateDirectory(path);
            return new ZlpDirectoryInfo(path);
        }

        public ZlpDirectoryInfo Parent =>
            new(
                ZlpPathHelper.GetDirectoryPathNameFromFilePath(FullName.TrimEnd(
                    Path.DirectorySeparatorChar,
                    Path.AltDirectorySeparatorChar)));

        public DateTime LastWriteTime
        {
            get => ZlpIOHelper.GetFileLastWriteTime(FullName);
            set => ZlpIOHelper.SetFileLastWriteTime(FullName, value);
        }

        public DateTime LastAccessTime
        {
            get => ZlpIOHelper.GetFileLastAccessTime(FullName);
            set => ZlpIOHelper.SetFileLastAccessTime(FullName, value);
        }

        public DateTime CreationTime
        {
            get => ZlpIOHelper.GetFileCreationTime(FullName);
            set => ZlpIOHelper.SetFileCreationTime(FullName, value);
        }

        public ZlpFileDateInfos DateInfos
        {
            get => ZlpIOHelper.GetFileDateInfos(FullName);
            set => ZlpIOHelper.SetFileDateInfos(FullName, value);
        }

        public FileAttributes Attributes
        {
            get => ZlpIOHelper.GetFileAttributes(FullName);
            set => ZlpIOHelper.SetFileAttributes(FullName, value);
        }
    }
}