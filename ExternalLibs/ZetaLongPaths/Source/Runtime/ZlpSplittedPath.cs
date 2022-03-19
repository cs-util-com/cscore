namespace ZetaLongPaths
{
    public sealed class ZlpSplittedPath
    {
        [PublicAPI]
        public ZlpSplittedPath(
            string path)
        {
            Info = new ZlpFileOrDirectoryInfo(path);
        }

        public ZlpSplittedPath(
            ZlpFileOrDirectoryInfo path)
        {
            Info = new ZlpFileOrDirectoryInfo(path);
        }

        [PublicAPI] public string FullPath => Info.FullName;

        [PublicAPI] public ZlpFileOrDirectoryInfo Info { get; }

        [PublicAPI] public string Drive => ZlpPathHelper.GetDrive(Info.FullName);

        [PublicAPI] public string Share => ZlpPathHelper.GetShare(Info.FullName);

        [PublicAPI] public string DriveOrShare => ZlpPathHelper.GetDriveOrShare(Info.FullName);

        [PublicAPI] public string Directory => ZlpPathHelper.GetDirectory(Info.FullName);

        [PublicAPI] public string NameWithoutExtension => ZlpPathHelper.GetNameWithoutExtension(Info.FullName);

        [PublicAPI] public string NameWithExtension => ZlpPathHelper.GetNameWithExtension(Info.FullName);

        [PublicAPI] public string Extension => ZlpPathHelper.GetExtension(Info.FullName);

        [PublicAPI] public string DriveOrShareAndDirectory => ZlpPathHelper.Combine(DriveOrShare, Directory);

        [PublicAPI]
        public string DriveOrShareAndDirectoryAndNameWithoutExtension =>
            ZlpPathHelper.Combine(ZlpPathHelper.Combine(DriveOrShare, Directory), NameWithoutExtension);

        [PublicAPI] public string DirectoryAndNameWithExtension => ZlpPathHelper.Combine(Directory, NameWithExtension);
    }
}