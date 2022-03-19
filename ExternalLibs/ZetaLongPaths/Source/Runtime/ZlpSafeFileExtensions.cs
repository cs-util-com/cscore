namespace ZetaLongPaths
{
    public static class ZlpSafeFileExtensions
    {
        [PublicAPI]
        public static ZlpDirectoryInfo SafeDelete(this ZlpDirectoryInfo folderPath)
        {
            ZlpSafeFileOperations.SafeDeleteDirectory(folderPath);
            return folderPath;
        }

        [PublicAPI]
        public static ZlpDirectoryInfo SafeDeleteContents(this ZlpDirectoryInfo folderPath)
        {
            ZlpSafeFileOperations.SafeDeleteDirectoryContents(folderPath);
            return folderPath;
        }

        [PublicAPI]
        public static ZlpFileInfo SafeDelete(this ZlpFileInfo filePath)
        {
            ZlpSafeFileOperations.SafeDeleteFile(filePath);
            return filePath;
        }

        [PublicAPI]
        public static bool SafeExists(this ZlpDirectoryInfo folderPath)
        {
            return ZlpSafeFileOperations.SafeDirectoryExists(folderPath);
        }

        [PublicAPI]
        public static bool SafeExists(this ZlpFileInfo filePath)
        {
            return ZlpSafeFileOperations.SafeFileExists(filePath);
        }

        [PublicAPI]
        public static ZlpDirectoryInfo SafeCheckCreate(this ZlpDirectoryInfo folderPath)
        {
            ZlpSafeFileOperations.SafeCheckCreateDirectory(folderPath);
            return folderPath;
        }

        [PublicAPI]
        public static ZlpFileInfo SafeMove(this ZlpFileInfo sourcePath, string dstFilePath)
        {
            ZlpSafeFileOperations.SafeMoveFile(sourcePath, dstFilePath);
            return sourcePath;
        }

        [PublicAPI]
        public static ZlpFileInfo SafeMove(this ZlpFileInfo sourcePath, ZlpFileInfo dstFilePath)
        {
            ZlpSafeFileOperations.SafeMoveFile(sourcePath, dstFilePath);
            return sourcePath;
        }

        [PublicAPI]
        public static ZlpFileInfo SafeCopy(this ZlpFileInfo sourcePath, string dstFilePath, bool overwrite = true)
        {
            ZlpSafeFileOperations.SafeCopyFile(sourcePath, dstFilePath, overwrite);
            return sourcePath;
        }

        [PublicAPI]
        public static ZlpFileInfo SafeCopy(this ZlpFileInfo sourcePath, ZlpFileInfo dstFilePath, bool overwrite = true)
        {
            ZlpSafeFileOperations.SafeCopyFile(sourcePath, dstFilePath, overwrite);
            return sourcePath;
        }
    }
}