namespace ZetaLongPaths
{
    using Native;

    public interface IZlpFileSystemInfo
    {
        [PublicAPI] bool Exists { get; }
        [PublicAPI] string OriginalPath { get; }
        [PublicAPI] string FullName { get; }
        [PublicAPI] string Extension { get; }
        [PublicAPI] string Name { get; }
        [PublicAPI] DateTime LastWriteTime { get; set; }
        [PublicAPI] DateTime LastAccessTime { get; set; }
        [PublicAPI] DateTime CreationTime { get; set; }
        [PublicAPI] FileAttributes Attributes { get; set; }

        [PublicAPI]
        void MoveToRecycleBin();

        [PublicAPI]
        string ToString();

        [PublicAPI]
        void Refresh();

        [PublicAPI]
        void Delete();

        [PublicAPI]
        void MoveTo(string destinationDirectoryPath);
    }
}