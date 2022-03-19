namespace ZetaLongPaths
{
    using Native;

    /// <summary>
    /// The goal of this class is to provide more error-tolerant functions
    /// for basic file operations. Especially when you have a larger project
    /// and ask yourself "why is this file being deleted?" this class helps
    /// by logging each operation and doing it in a more error-tolerant way,
    /// too. So do all file operations through this class and you get a more
    /// determinable system, hopefully.
    /// </summary>
    /// <remarks>
    /// 2007-03-08: Initially created class.
    /// </remarks>
    public static class ZlpSafeFileOperations
    {
        public static void SafeDeleteFile(
            ZlpFileInfo filePath)
        {
            if (filePath != null)
            {
                SafeDeleteFile(filePath.FullName);
            }
        }

        [PublicAPI]
        public static void SafeDeleteFile(
            string filePath)
        {
#if WANT_TRACE
            Trace.TraceInformation(@"About to safe-delete file '{0}'.", filePath);
#endif

            if (!string.IsNullOrEmpty(filePath) && SafeFileExists(filePath))
            {
                try
                {
                    var attributes = ZlpIOHelper.GetFileAttributes(filePath);

                    // Remove read-only attributes.
                    if ((attributes & FileAttributes.Readonly) != 0)
                    {
                        ZlpIOHelper.SetFileAttributes(filePath, attributes & ~FileAttributes.Readonly);
                    }

                    ZlpIOHelper.DeleteFile(filePath);
                }
                catch (UnauthorizedAccessException x)
                {
                    var newFilePath =
                        $@"{filePath}.{Guid.NewGuid():N}.deleted";

#if WANT_TRACE
                    Trace.TraceWarning(@"Caught UnauthorizedAccessException while deleting file '{0}'. " +
                                       @"Renaming now to '{1}'. {2}", filePath, newFilePath, x.Message);
#endif

                    try
                    {
                        ZlpIOHelper.MoveFile(
                            filePath,
                            newFilePath,
                            true );
                    }
                    catch (Win32Exception x2)
                    {
#if WANT_TRACE
                        Trace.TraceWarning(@"Caught IOException while renaming upon failed deleting file '{0}'. " +
                                           @"Renaming now to '{1}'. {2}", filePath, newFilePath, x2.Message);
#endif
                    }
                }
                catch (Win32Exception x)
                {
                    var newFilePath =
                        $@"{filePath}.{Guid.NewGuid():N}.deleted";

#if WANT_TRACE
                    Trace.TraceWarning(@"Caught IOException while deleting file '{0}'. " +
                                       @"Renaming now to '{1}'. {2}", filePath, newFilePath, x.Message);
#endif

                    try
                    {
                        ZlpIOHelper.MoveFile(filePath, newFilePath, true);
                    }
                    catch (Win32Exception x2)
                    {
#if WANT_TRACE
                        Trace.TraceWarning(@"Caught IOException while renaming upon failed deleting file '{0}'. " +
                                           @"Renaming now to '{1}'. {2}", filePath, newFilePath, x2.Message);
#endif
                    }
                }
            }
            else
            {
#if WANT_TRACE
                Trace.TraceInformation(@"Not safe-deleting file '{0}', " +
                                       @"because the file does not exist.", filePath);
#endif
            }
        }

        public static bool SafeFileExists(
            ZlpFileInfo filePath)
        {
            return filePath != null && SafeFileExists(filePath.FullName);
        }

        [PublicAPI]
        public static bool SafeFileExists(
            string filePath)
        {
            return !string.IsNullOrEmpty(filePath) && ZlpIOHelper.FileExists(filePath);
        }

        /// <summary>
        /// Deep-deletes the contents, as well as the folder itself.
        /// </summary>
        public static void SafeDeleteDirectory(
            ZlpDirectoryInfo folderPath)
        {
            if (folderPath != null)
            {
                SafeDeleteDirectory(folderPath.FullName);
            }
        }

        /// <summary>
        /// Deep-deletes the contents, as well as the folder itself.
        /// </summary>
        [PublicAPI]
        public static void SafeDeleteDirectory(
            string folderPath)
        {
#if WANT_TRACE
            Trace.TraceInformation(@"About to safe-delete directory '{0}'.", folderPath);
#endif

            if (!string.IsNullOrEmpty(folderPath) && SafeDirectoryExists(folderPath))
            {
                try
                {
                    ZlpIOHelper.DeleteDirectory(folderPath, true);
                }
                catch (Win32Exception x)
                {
                    var newFilePath = $@"{folderPath}.{Guid.NewGuid():B}.deleted";

#if WANT_TRACE
                    Trace.TraceWarning(@"Caught IOException while deleting directory '{0}'. " +
                                       @"Renaming now to '{1}'. {2}", folderPath, newFilePath, x.Message);
#endif

                    try
                    {
                        ZlpIOHelper.MoveDirectory(
                            folderPath,
                            newFilePath);
                    }
                    catch (Win32Exception x2)
                    {
#if WANT_TRACE
                        Trace.TraceWarning(@"Caught IOException while renaming upon failed deleting directory '{0}'. " +
                                           @"Renaming now to '{1}'. {2}", folderPath, newFilePath, x2.Message);
#endif
                    }
                }
            }
            else
            {
#if WANT_TRACE
                Trace.TraceInformation(@"Not safe-deleting directory '{0}', " +
                                       @"because the directory does not exist.", folderPath);
#endif
            }
        }

        public static bool SafeDirectoryExists(
            ZlpDirectoryInfo folderPath)
        {
            return folderPath != null && SafeDirectoryExists(folderPath.FullName);
        }

        [PublicAPI]
        public static bool SafeDirectoryExists(
            string folderPath)
        {
            return !string.IsNullOrEmpty(folderPath) && ZlpIOHelper.DirectoryExists(folderPath);
        }

        public static void SafeMoveFile(
            ZlpFileInfo sourcePath,
            string dstFilePath)
        {
            SafeMoveFile(
                sourcePath?.FullName,
                dstFilePath);
        }

        [PublicAPI]
        public static void SafeMoveFile(
            string sourcePath,
            ZlpFileInfo dstFilePath)
        {
            SafeMoveFile(
                sourcePath,
                dstFilePath?.FullName);
        }

        public static void SafeMoveFile(
            ZlpFileInfo sourcePath,
            ZlpFileInfo dstFilePath)
        {
            SafeMoveFile(
                sourcePath?.FullName,
                dstFilePath?.FullName);
        }

        [PublicAPI]
        public static void SafeMoveFile(
            string sourcePath,
            string dstFilePath)
        {
            Trace.TraceInformation(@"About to safe-move file from '{0}' to '{1}'.", sourcePath, dstFilePath);

            if (sourcePath == null || dstFilePath == null)
            {
                Trace.TraceInformation(
                    string.Format(
                        @"Source file path or destination file path does not exist. " +
                        @"Not moving."
                    ));
            }
            else
            {
                if (SafeFileExists(sourcePath))
                {
                    SafeDeleteFile(dstFilePath);

                    var d = ZlpPathHelper.GetDirectoryPathNameFromFilePath(dstFilePath);

                    if (!ZlpIOHelper.DirectoryExists(d))
                    {
                        Trace.TraceInformation(@"Creating non-existing folder '{0}'.", d);
                        ZlpIOHelper.CreateDirectory(d);
                    }

                    ZlpIOHelper.MoveFile(sourcePath, dstFilePath, true);
                }
                else
                {
                    Trace.TraceInformation(@"Source file path to move does not exist: '{0}'.", sourcePath);
                }
            }
        }

        public static void SafeCopyFile(
            ZlpFileInfo sourcePath,
            string dstFilePath,
            bool overwrite = true)
        {
            SafeCopyFile(sourcePath?.FullName, dstFilePath, overwrite);
        }

        [PublicAPI]
        public static void SafeCopyFile(
            string sourcePath,
            ZlpFileInfo dstFilePath,
            bool overwrite = true)
        {
            SafeCopyFile(sourcePath, dstFilePath?.FullName, overwrite);
        }

        public static void SafeCopyFile(
            ZlpFileInfo sourcePath,
            ZlpFileInfo dstFilePath,
            bool overwrite = true)
        {
            SafeCopyFile(
                sourcePath?.FullName,
                dstFilePath?.FullName,
                overwrite);
        }

        [PublicAPI]
        public static void SafeCopyFile(
            string sourcePath,
            string dstFilePath,
            bool overwrite = true)
        {
            Trace.TraceInformation(@"About to safe-copy file from '{0}' to '{1}' " +
                                   @"with overwrite = '{2}'.", sourcePath, dstFilePath, overwrite);

            if (sourcePath == null || dstFilePath == null)
            {
                Trace.TraceInformation(
                    string.Format(
                        @"Source file path or destination file path does not exist. " +
                        @"Not copying."
                    ));
            }
            else
            {
                if (string.Compare(sourcePath, dstFilePath, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    Trace.TraceInformation(@"Source path and destination path are the same: " +
                                           @"'{0}' is '{1}'. Not copying.", sourcePath, dstFilePath);
                }
                else
                {
                    if (SafeFileExists(sourcePath))
                    {
                        if (overwrite)
                        {
                            SafeDeleteFile(dstFilePath);
                        }

                        var d = ZlpPathHelper.GetDirectoryPathNameFromFilePath(dstFilePath);

                        if (!ZlpIOHelper.DirectoryExists(d))
                        {
                            Trace.TraceInformation(@"Creating non-existing folder '{0}'.", d);
                            ZlpIOHelper.CreateDirectory(d);
                        }

                        ZlpIOHelper.CopyFile(sourcePath, dstFilePath, overwrite);
                    }
                    else
                    {
                        Trace.TraceInformation(@"Source file path to copy does not exist: '{0}'.", sourcePath);
                    }
                }
            }
        }

        /// <summary>
        /// Deep-deletes the contents, but not the folder itself.
        /// </summary>
        [PublicAPI]
        public static void SafeDeleteDirectoryContents(
            string folderPath)
        {
            var info = new ZlpDirectoryInfo(folderPath);
            SafeDeleteDirectoryContents(info);
        }

        /// <summary>
        /// Deep-deletes the contents, but not the folder itself.
        /// </summary>
        public static void SafeDeleteDirectoryContents(
            ZlpDirectoryInfo folderPath)
        {
            if (folderPath is {Exists: true})
            {
                foreach (var filePath in folderPath.GetFiles())
                {
                    SafeDeleteFile(filePath);
                }

                foreach (var childFolderPath in
                         folderPath.GetDirectories())
                {
                    SafeDeleteDirectoryContents(childFolderPath);

                    // If empty now, remove.
                    // Only for childs, not for the root.
                    if (childFolderPath.GetFiles().Length <= 0 &&
                        childFolderPath.GetDirectories().Length <= 0)
                    {
                        childFolderPath.Delete(true);
                    }
                }
            }
        }

        public static void SafeCheckCreateDirectory(
            ZlpDirectoryInfo folderPath)
        {
            SafeCheckCreateDirectory(folderPath?.FullName);
        }

        [PublicAPI]
        public static void SafeCheckCreateDirectory(
            string folderPath)
        {
            Trace.TraceInformation(@"About to safe check-create folder '{0}'.", folderPath);

            if (!string.IsNullOrEmpty(folderPath) && !SafeDirectoryExists(folderPath))
            {
                try
                {
                    ZlpIOHelper.CreateDirectory(folderPath);
                }
                catch (UnauthorizedAccessException x)
                {
                    Trace.TraceWarning(
                        @"Caught UnauthorizedAccessException while safe check-creating folder '{0}'. {1}", folderPath,
                        x.Message);
                }
                catch (Win32Exception x)
                {
                    Trace.TraceWarning(@"Caught IOException while safe check-creating folder '{0}'. {1}", folderPath,
                        x.Message);
                }
            }
            else
            {
                Trace.TraceInformation(
                    @"Not safe check-creating folder '{0}', because the folder is null or already exists.", folderPath);
            }
        }
    }
}