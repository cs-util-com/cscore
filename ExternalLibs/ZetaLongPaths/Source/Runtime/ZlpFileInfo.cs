namespace ZetaLongPaths
{
    using Native;
    using FileAccess = Native.FileAccess;
    using FileAttributes = Native.FileAttributes;
    using FileShare = Native.FileShare;

// ReSharper disable once UseNameofExpression
    [DebuggerDisplay(@"{FullName}")]
    public class ZlpFileInfo :
        IZlpFileSystemInfo
    {
        [PublicAPI]
        public static ZlpFileInfo GetTemp() => new(ZlpPathHelper.GetTempFilePath());

        [PublicAPI]
        public static ZlpFileInfo GetTemp(string extension) => new(ZlpPathHelper.GetTempFilePath(extension));

        public ZlpFileInfo(string path)
        {
            FullName = path;
        }

        [PublicAPI]
        public ZlpFileInfo(FileInfo path)
        {
            FullName = path?.FullName;
        }

        [PublicAPI]
        public ZlpFileInfo(ZlpFileInfo path)
        {
            FullName = path?.FullName;
        }

        [PublicAPI]
        public static ZlpFileInfo FromOther(ZlpFileInfo path)
        {
            return new(path);
        }

        [PublicAPI]
        public static ZlpFileInfo FromString(string path)
        {
            return new(path);
        }

        [PublicAPI]
        public static ZlpFileInfo FromBuiltIn(FileInfo path)
        {
            return new(path);
        }

        [PublicAPI]
        public FileInfo ToBuiltIn()
        {
            return new(FullName);
        }

        [PublicAPI]
        public ZlpFileInfo ToOther()
        {
            return Clone();
        }

        [PublicAPI]
        public void Refresh()
        {
        }

        [PublicAPI]
        public ZlpFileInfo Clone()
        {
            return new(FullName);
        }

        [PublicAPI]
        public ZlpFileInfo GetFullPath()
        {
            return new(ZlpPathHelper.GetFullPath(FullName));
        }

        [PublicAPI]
        public bool IsReadOnly
        {
            get => (Attributes & FileAttributes.Readonly) != 0;
            set
            {
                if (value)
                    Attributes |= FileAttributes.Readonly;
                else
                    Attributes &= ~FileAttributes.Readonly;
            }
        }

        public void MoveToRecycleBin() => ZlpIOHelper.MoveFileToRecycleBin(FullName);

        public string OriginalPath => FullName;

        public override string ToString() => FullName;

        public void MoveTo(
            string destinationFilePath) => ZlpIOHelper.MoveFile(FullName, destinationFilePath);

        [PublicAPI]
        public void MoveTo(
            ZlpFileInfo destinationFilePath)
            => ZlpIOHelper.MoveFile(FullName, destinationFilePath.FullName);

        public void MoveTo(
            string destinationFilePath,
            bool overwriteExisting) => ZlpIOHelper.MoveFile(FullName, destinationFilePath, overwriteExisting);

        [PublicAPI]
        public void MoveTo(
            ZlpFileInfo destinationFilePath,
            bool overwriteExisting)
            => ZlpIOHelper.MoveFile(FullName, destinationFilePath.FullName, overwriteExisting);

        /// <summary>
        /// Pass the file handle to the <see cref="System.IO.FileStream"/> constructor. 
        /// The <see cref="System.IO.FileStream"/> will close the handle.
        /// </summary>
        [PublicAPI]
        public SafeFileHandle CreateHandle(
            CreationDisposition creationDisposition,
            FileAccess fileAccess,
            FileShare fileShare)
        {
            return ZlpIOHelper.CreateFileHandle(FullName, creationDisposition, fileAccess, fileShare);
        }

        [PublicAPI]
        public void CopyTo(
            string destinationFilePath,
            bool overwriteExisting)
        {
            ZlpIOHelper.CopyFile(FullName, destinationFilePath, overwriteExisting);
        }

        [PublicAPI]
        public void CopyTo(
            ZlpFileInfo destinationFilePath,
            bool overwriteExisting)
        {
            ZlpIOHelper.CopyFile(FullName, destinationFilePath.FullName, overwriteExisting);
        }

        [PublicAPI]
        public void CopyToExact(
            string destinationFilePath,
            bool overwriteExisting)
        {
            ZlpIOHelper.CopyFileExact(FullName, destinationFilePath, overwriteExisting);
        }

        [PublicAPI]
        public void CopyToExact(
            ZlpFileInfo destinationFilePath,
            bool overwriteExisting)
        {
            ZlpIOHelper.CopyFileExact(FullName, destinationFilePath.FullName, overwriteExisting);
        }

        [PublicAPI]
        public void Delete() => ZlpIOHelper.DeleteFile(FullName);

        [PublicAPI]
        public void DeleteFileAfterReboot() => ZlpIOHelper.DeleteFileAfterReboot(FullName);

        [PublicAPI]
        public void Touch() => ZlpIOHelper.Touch(FullName);

        [PublicAPI]
        public string Owner => ZlpIOHelper.GetFileOwner(FullName);

        public bool Exists => ZlpIOHelper.FileExists(FullName);

        [PublicAPI]
        public byte[] ReadAllBytes()
        {
            return ZlpIOHelper.ReadAllBytes(FullName);
        }

        [PublicAPI]
        public FileStream OpenRead()
        {
            return new(
                ZlpIOHelper.CreateFileHandle(
                    FullName,
                    CreationDisposition.OpenAlways,
                    FileAccess.GenericRead,
                    FileShare.Read),
                System.IO.FileAccess.Read);
        }

        [PublicAPI]
        public FileStream OpenWrite()
        {
            return new(
                ZlpIOHelper.CreateFileHandle(
                    FullName,
                    CreationDisposition.OpenAlways,
                    FileAccess.GenericRead | FileAccess.GenericWrite,
                    FileShare.Read | FileShare.Write),
                System.IO.FileAccess.ReadWrite);
        }

        [PublicAPI]
        public FileStream OpenCreate()
        {
            return new(
                ZlpIOHelper.CreateFileHandle(
                    FullName,
                    CreationDisposition.CreateAlways,
                    FileAccess.GenericRead | FileAccess.GenericWrite,
                    FileShare.Read | FileShare.Write),
                System.IO.FileAccess.ReadWrite);
        }

        [PublicAPI]
        public string ReadAllText() => ZlpIOHelper.ReadAllText(FullName);

        [PublicAPI]
        public string ReadAllText(Encoding encoding) => ZlpIOHelper.ReadAllText(FullName, encoding);

        [PublicAPI]
        public string[] ReadAllLines() => ZlpIOHelper.ReadAllLines(FullName);

        [PublicAPI]
        public string[] ReadAllLines(Encoding encoding) => ZlpIOHelper.ReadAllLines(FullName, encoding);

        [PublicAPI]
        public void WriteAllText(string text, Encoding encoding = null)
            => ZlpIOHelper.WriteAllText(FullName, text, encoding);

        [PublicAPI]
        public void WriteAllLines(string[] lines, Encoding encoding = null)
            => ZlpIOHelper.WriteAllLines(FullName, lines, encoding);

        [PublicAPI]
        public void AppendText(string text, Encoding encoding = null)
            => ZlpIOHelper.AppendText(FullName, text, encoding);

        [PublicAPI]
        public void WriteAllBytes(byte[] content) => ZlpIOHelper.WriteAllBytes(FullName, content);

        [PublicAPI]
        public void AppendBytes(byte[] content) => ZlpIOHelper.AppendBytes(FullName, content);

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

        public string FullName { get; }

        public string Name => ZlpPathHelper.GetFileNameFromFilePath(FullName);

        /// <summary>
        /// Returns the same MD5 hash as the PHP function call http://php.net/manual/de/function.hash-file.php 
        /// with 'md5' as the first parameter.
        /// </summary>
        public string MD5Hash => ZlpIOHelper.CalculateMD5Hash(FullName);

        [PublicAPI]
        public string NameWithoutExtension => ZlpPathHelper.GetFileNameWithoutExtension(Name);

        public ZlpDirectoryInfo Directory => new(DirectoryName);

        public string DirectoryName => ZlpPathHelper.GetDirectoryPathNameFromFilePath(FullName);

        public string Extension => ZlpPathHelper.GetExtension(FullName);

        public long Length => ZlpIOHelper.GetFileLength(FullName);

        public FileAttributes Attributes
        {
            get => ZlpIOHelper.GetFileAttributes(FullName);
            set => ZlpIOHelper.SetFileAttributes(FullName, value);
        }
    }
}