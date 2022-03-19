namespace ZetaLongPaths
{
    using Native;
    using Native.FileOperations;
    using Native.Interop;
    using Properties;
    using FileAccess = Native.FileAccess;
    using FileAttributes = Native.FileAttributes;
    using FileShare = Native.FileShare;

    public static class ZlpIOHelper
    {
        private const FileOperationFlags FileOperationDeleteFlags =
            FileOperationFlags.FOF_ALLOWUNDO |
            FileOperationFlags.FOF_NOCONFIRMATION |
            FileOperationFlags.FOF_NOCONFIRMMKDIR |
            FileOperationFlags.FOF_NOERRORUI |
            FileOperationFlags.FOF_SILENT |
            FileOperationFlags.FOF_WANTNUKEWARNING;

        public static void MoveFileToRecycleBin(
            string filePath)
        {
            using var fo = new FileOperation(new FileOperationProgressSink());
            fo.SetOperationFlags(FileOperationDeleteFlags);
            fo.DeleteItem(ZlpPathHelper.GetFullPath(filePath));
            fo.PerformOperations();
        }

        public static void MoveDirectoryToRecycleBin(
            string directoryPath)
        {
            using var fo = new FileOperation(new FileOperationProgressSink());
            fo.SetOperationFlags(FileOperationDeleteFlags);
            fo.DeleteItem(ZlpPathHelper.GetFullPath(directoryPath));
            fo.PerformOperations();
        }

        public static byte[] ReadAllBytes(
            string path)
        {
            using var fs =
                new FileStream(
                    CreateFileHandle(
                        path,
                        CreationDisposition.OpenExisting,
                        FileAccess.GenericRead,
                        FileShare.Read),
                    System.IO.FileAccess.Read);
            var buf = new byte[fs.Length];
            fs.Read(buf, 0, buf.Length);

            return buf;
        }

        public static string ReadAllText(
            string path)
        {
            var encoding = new UTF8Encoding(false, true);
            return ReadAllText(path, encoding);
        }

        public static string[] ReadAllLines(
            string path)
        {
            var encoding = new UTF8Encoding(false, true);
            return ReadAllLines(path, encoding);
        }

        [PublicAPI]
        public static bool IsDirectoryEmpty(
            string path)
        {
            return GetFiles(path).Length <= 0 && GetDirectories(path).Length <= 0;
        }

        /// <summary>
        /// Returns the same MD5 hash as the PHP function call http://php.net/manual/de/function.hash-file.php 
        /// with 'md5' as the first parameter.
        /// </summary>
        public static string CalculateMD5Hash(
            string path)
        {
            // https://stackoverflow.com/a/10520086/107625

            using var fs =
                new FileStream(
                    CreateFileHandle(
                        path,
                        CreationDisposition.OpenExisting,
                        FileAccess.GenericRead,
                        FileShare.Read),
                    System.IO.FileAccess.Read);
            using var md5 = MD5.Create();
            var hash = md5.ComputeHash(fs);
            return BitConverter.ToString(hash).Replace(@"-", string.Empty).ToLowerInvariant();
        }

        public static string ReadAllText(
            string path,
            Encoding encoding)
        {
            using var fs =
                new FileStream(
                    CreateFileHandle(
                        path,
                        CreationDisposition.OpenExisting,
                        FileAccess.GenericRead,
                        FileShare.Read),
                    System.IO.FileAccess.Read);
            using var sr = new StreamReader(fs, encoding);
            return sr.ReadToEnd();
        }

        public static string[] ReadAllLines(
            string path,
            Encoding encoding)
        {
            var lines = new List<string>();

            using var fs =
                new FileStream(
                    CreateFileHandle(
                        path,
                        CreationDisposition.OpenExisting,
                        FileAccess.GenericRead,
                        FileShare.Read),
                    System.IO.FileAccess.Read);
            using var sr = new StreamReader(fs, encoding);

            string line;
            while ((line = sr.ReadLine()) != null)
                lines.Add(line);

            return lines.ToArray();
        }

        public static void WriteAllLines(
            string path,
            string[] lines,
            Encoding encoding = null)
        {
            WriteAllText(path, string.Join(Environment.NewLine, lines), encoding);
        }

        public static void WriteAllText(
            string path,
            string contents,
            Encoding encoding = null)
        {
            encoding ??= new UTF8Encoding(false, true);

            using var fs =
                new FileStream(
                    CreateFileHandle(
                        path,
                        CreationDisposition.CreateAlways,
                        FileAccess.GenericWrite,
                        FileShare.Read),
                    System.IO.FileAccess.Write);
            using var streamWriter = new StreamWriter(fs, encoding);
            streamWriter.Write(contents);
        }

        public static void AppendText(
            string path,
            string contents,
            Encoding encoding = null)
        {
            encoding ??= new UTF8Encoding(false, true);

            using var fs =
                new FileStream(
                    CreateFileHandle(
                        path,
                        FileExists(path) ? CreationDisposition.OpenExisting : CreationDisposition.CreateAlways,
                        FileAccess.GenericWrite,
                        FileShare.Read),
                    System.IO.FileAccess.Write);
            using var streamWriter = new StreamWriter(fs, encoding);
            fs.Seek(0, SeekOrigin.End);

            streamWriter.Write(contents);
        }

        public static void AppendBytes(
            string path,
            byte[] contents)
        {
            using var fs =
                new FileStream(
                    CreateFileHandle(
                        path,
                        CreationDisposition.OpenAlways,
                        FileAccess.FileAppendData,
                        FileShare.Read),
                    System.IO.FileAccess.Write);
            fs.Write(contents, 0, contents.Length);
        }

        public static void WriteAllBytes(
            string path,
            byte[] contents)
        {
            using var fs =
                new FileStream(
                    CreateFileHandle(
                        path,
                        CreationDisposition.CreateAlways,
                        FileAccess.GenericWrite,
                        FileShare.Read),
                    System.IO.FileAccess.Write);
            fs.Write(contents, 0, contents.Length);
        }

        /// <summary>
        /// Pass the file handle to the <see cref="System.IO.FileStream"/> constructor.
        /// The <see cref="System.IO.FileStream"/> will close the handle.
        /// </summary>
        public static SafeFileHandle CreateFileHandle(
            string filePath,
            CreationDisposition creationDisposition,
            FileAccess fileAccess,
            FileShare fileShare,
            bool useAsync = false)
        {
            filePath = CheckAddLongPathPrefix(filePath);

            // Create a file with generic write access
            var fileHandle =
                PInvokeHelper.CreateFile(
                    filePath,
                    fileAccess,
                    fileShare,
                    IntPtr.Zero,
                    creationDisposition,
                    // Fix by Richard, 2015-04-14.
                    // See https://msdn.microsoft.com/en-us/library/aa363858(VS.85).aspx#DIRECTORIES,
                    // See http://stackoverflow.com/q/4998814/107625
                    FileAttributes.BackupSemantics | (useAsync ? FileAttributes.Overlapped : 0),
                    IntPtr.Zero);

            // Check for errors.
            var lastWin32Error = Marshal.GetLastWin32Error();
            if (fileHandle.IsInvalid)
            {
                var x = new Win32Exception(
                    lastWin32Error,
                    string.Format(
                        Resources.ErrorCreatingFileHandle,
                        lastWin32Error,
                        filePath,
                        CheckAddDotEnd(new Win32Exception(lastWin32Error).Message)))
                {
                    Data =
                    {
                        [nameof(filePath)] = filePath,
                        [nameof(creationDisposition)] = creationDisposition,
                        [nameof(fileAccess)] = fileAccess,
                        [nameof(fileShare)] = fileShare,
                        [nameof(useAsync)] = useAsync
                    }
                };

                throw x;
            }

            // Pass the file handle to FileStream. FileStream will close the handle.
            return fileHandle;
        }

        [PublicAPI]
        public static int ReadFile(
            SafeFileHandle handle,
            byte[] buffer,
            int offset,
            int count)
        {
            var gCHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);

            bool flag;
            uint result;

            try
            {
                var q = new IntPtr(gCHandle.AddrOfPinnedObject().ToInt64() + offset);
                flag = PInvokeHelper.ReadFile(handle, q, (uint)count, out result, IntPtr.Zero);
            }
            finally
            {
                gCHandle.Free();
            }

            var lastWin32Error = Marshal.GetLastWin32Error();
            if (!flag)
            {
                var x = new Win32Exception(
                    lastWin32Error,
                    string.Format(
                        Resources.ErrorReadFile,
                        lastWin32Error,
                        CheckAddDotEnd(new Win32Exception(lastWin32Error).Message)))
                {
                    Data =
                    {
                        [nameof(offset)] = offset,
                        [nameof(count)] = count
                    }
                };

                throw x;
            }

            return (int)result;
        }

        [PublicAPI]
        public static int WriteFile(
            SafeFileHandle handle,
            byte[] buffer,
            int offset,
            int count)
        {
            var gCHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);

            bool flag;
            uint result;

            try
            {
                var q = new IntPtr(gCHandle.AddrOfPinnedObject().ToInt64() + offset);
                flag = PInvokeHelper.WriteFile(handle, q, (uint)count, out result, IntPtr.Zero);
            }
            finally
            {
                gCHandle.Free();
            }

            var lastWin32Error = Marshal.GetLastWin32Error();
            if (!flag)
            {
                var x = new Win32Exception(
                    lastWin32Error,
                    string.Format(
                        Resources.ErrorWriteFile,
                        lastWin32Error,
                        CheckAddDotEnd(new Win32Exception(lastWin32Error).Message)))
                {
                    Data =
                    {
                        [nameof(offset)] = offset,
                        [nameof(count)] = count
                    }
                };

                throw x;
            }

            return (int)result;
        }

        [PublicAPI]
        public static long Seek(
            SafeFileHandle handle,
            long distance,
            FileSeekOrigin origin)
        {
            var flag = PInvokeHelper.Seek(handle, distance, out var result, origin);

            var lastWin32Error = Marshal.GetLastWin32Error();
            if (!flag)
            {
                var x = new Win32Exception(
                    lastWin32Error,
                    string.Format(
                        Resources.ErrorSeek,
                        lastWin32Error,
                        CheckAddDotEnd(new Win32Exception(lastWin32Error).Message)))
                {
                    Data =
                    {
                        [nameof(distance)] = distance,
                        [nameof(origin)] = origin
                    }
                };

                throw x;
            }

            return result;
        }

        public static void CopyFileExact(
            string sourceFilePath,
            string destinationFilePath,
            bool overwriteExisting)
        {
            CopyFile(sourceFilePath, destinationFilePath, overwriteExisting);
            CloneDates(sourceFilePath, destinationFilePath);
        }

        private static void CloneDates(string sourceFilePath, string destinationFilePath)
        {
            var s = new ZlpFileInfo(sourceFilePath);
            var d = new ZlpFileInfo(destinationFilePath);

            var sc = s.CreationTime;
            var sa = s.LastAccessTime;
            var sw = s.LastWriteTime;

            if (sc > DateTime.MinValue) d.CreationTime = sc;
            if (sa > DateTime.MinValue) d.LastAccessTime = sa;
            if (sw > DateTime.MinValue) d.LastWriteTime = sw;
        }

        public static bool DriveExists(char driveLetter)
        {
            return DriveInfo.GetDrives().Any(di =>
                di.Name.StartsWith($@"{driveLetter}:", StringComparison.InvariantCultureIgnoreCase));
        }

        public static void Touch(string filePath)
        {
            var now = DateTime.Now;

            SetFileCreationTime(filePath, now);
            SetFileLastAccessTime(filePath, now);
            SetFileLastWriteTime(filePath, now);
        }

        public static void CopyFile(
            string sourceFilePath,
            string destinationFilePath,
            bool overwriteExisting)
        {
            sourceFilePath = CheckAddLongPathPrefix(sourceFilePath);
            destinationFilePath = CheckAddLongPathPrefix(destinationFilePath);

            if (!PInvokeHelper.CopyFile(sourceFilePath, destinationFilePath, !overwriteExisting))
            {
                // http://msdn.microsoft.com/en-us/library/ms681382(VS.85).aspx.

                var lastWin32Error = Marshal.GetLastWin32Error();
                var x = new Win32Exception(
                    lastWin32Error,
                    string.Format(
                        Resources.ErrorCopyingFile,
                        lastWin32Error,
                        sourceFilePath,
                        destinationFilePath,
                        CheckAddDotEnd(new Win32Exception(lastWin32Error).Message)))
                {
                    Data =
                    {
                        [nameof(sourceFilePath)] = sourceFilePath,
                        [nameof(destinationFilePath)] = destinationFilePath,
                        [nameof(overwriteExisting)] = overwriteExisting
                    }
                };

                throw x;
            }
        }

        public static void MoveFile(
            string sourceFilePath,
            string destinationFilePath,
            bool overwriteExisting = false)
        {
            sourceFilePath = CheckAddLongPathPrefix(sourceFilePath);
            destinationFilePath = CheckAddLongPathPrefix(destinationFilePath);
            var flags = overwriteExisting ? MoveFileExFlags.ReplaceExisting : MoveFileExFlags.None;

            if (!PInvokeHelper.MoveFileEx(sourceFilePath, destinationFilePath, flags | MoveFileExFlags.CopyAllowed))
            {
                // http://msdn.microsoft.com/en-us/library/ms681382(VS.85).aspx.

                var lastWin32Error = Marshal.GetLastWin32Error();
                var x = new Win32Exception(
                    lastWin32Error,
                    string.Format(
                        Resources.ErrorMovingFile,
                        lastWin32Error,
                        sourceFilePath,
                        destinationFilePath,
                        CheckAddDotEnd(new Win32Exception(lastWin32Error).Message)))
                {
                    Data =
                    {
                        [nameof(sourceFilePath)] = sourceFilePath,
                        [nameof(destinationFilePath)] = destinationFilePath,
                        [nameof(overwriteExisting)] = overwriteExisting
                    }
                };

                throw x;
            }
        }

        /// <summary>
        /// This does only succeed if the process is in the context of a user who
        /// belongs to the administrators group or the LocalSystem account.
        /// </summary>
        public static void DeleteFileAfterReboot(
            string sourceFilePath,
            bool throwIfFails = false)
        {
            // http://stackoverflow.com/questions/6077869/movefile-function-in-c-sharp-delete-file-after-reboot-c-sharp

            sourceFilePath = CheckAddLongPathPrefix(sourceFilePath);

            // Aus der Doku:
            // "...This value can be used only if the process is in the context of a user who belongs to the administrators group or the LocalSystem account..."
            if (!PInvokeHelper.MoveFileEx(sourceFilePath, null, MoveFileExFlags.DelayUntilReboot))
            {
                var lastWin32Error = Marshal.GetLastWin32Error();

                if (throwIfFails)
                {
                    // http://msdn.microsoft.com/en-us/library/ms681382(VS.85).aspx.

                    var x = new Win32Exception(
                        lastWin32Error,
                        string.Format(
                            Resources.ErrorMarkingFileForDeletion,
                            lastWin32Error,
                            sourceFilePath,
                            CheckAddDotEnd(new Win32Exception(lastWin32Error).Message)))
                    {
                        Data =
                        {
                            [nameof(sourceFilePath)] = sourceFilePath,
                            [nameof(throwIfFails)] = true
                        }
                    };

                    throw x;
                }
                else
                {
#if WANT_TRACE
                    // ReSharper disable once InvocationIsSkipped
                    Trace.TraceWarning(@"Error {0} marking file '{1}' for deletion after reboot: {2}",
                        lastWin32Error,
                        sourceFilePath,
                        CheckAddDotEnd(new Win32Exception(lastWin32Error).Message));
#endif
                }
            }
        }

        /// <summary>
        /// The destination folder may not exists yet, otherwise an error 183 will be thrown.
        /// </summary>
        public static void MoveDirectory(
            string sourceFolderPath,
            string destinationFolderPath,
            bool writeThrough = false)
        {
            sourceFolderPath = CheckAddLongPathPrefix(sourceFolderPath);
            destinationFolderPath = CheckAddLongPathPrefix(destinationFolderPath);

            if (writeThrough) // https://docs.microsoft.com/en-us/windows/win32/fileio/moving-directories
            {
                if (!PInvokeHelper.MoveFileEx(sourceFolderPath, destinationFolderPath,
                        // https://docs.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-movefileexa#parameters
                        MoveFileExFlags.WriteThrough))
                {
                    // http://msdn.microsoft.com/en-us/library/ms681382(VS.85).aspx.

                    var lastWin32Error = Marshal.GetLastWin32Error();
                    throw new Win32Exception(
                        lastWin32Error,
                        string.Format(
                            Resources.ErrorMovingFolder,
                            lastWin32Error,
                            sourceFolderPath,
                            destinationFolderPath,
                            CheckAddDotEnd(new Win32Exception(lastWin32Error).Message)));
                }
            }
            else
            {
                if (!PInvokeHelper.MoveFile(sourceFolderPath, destinationFolderPath))
                {
                    // http://msdn.microsoft.com/en-us/library/ms681382(VS.85).aspx.

                    var lastWin32Error = Marshal.GetLastWin32Error();
                    throw new Win32Exception(
                        lastWin32Error,
                        string.Format(
                            Resources.ErrorMovingFolder,
                            lastWin32Error,
                            sourceFolderPath,
                            destinationFolderPath,
                            CheckAddDotEnd(new Win32Exception(lastWin32Error).Message)));
                }
            }
        }

        // http://www.dotnet247.com/247reference/msgs/21/108780.aspx
        public static string GetFileOwner(
            string filePath)
        {
            filePath = CheckAddLongPathPrefix(filePath);
            // Not used here

            var errorReturn =
                PInvokeHelper.GetNamedSecurityInfo(
                    filePath,
                    PInvokeHelper.SeFileObject,
                    PInvokeHelper.OwnerSecurityInformation,
                    out var pSid,
                    out _,
                    out _,
                    out _,
                    out _);

            if (errorReturn == 0)
            {
                const int bufferSize = 64;
                var buffer = new StringBuilder();
                var accounLength = bufferSize;
                var domainLength = bufferSize;
                var account = new StringBuilder(bufferSize);
                var domain = new StringBuilder(bufferSize);

                errorReturn =
                    PInvokeHelper.LookupAccountSid(
                        null,
                        pSid,
                        account,
                        ref accounLength,
                        domain,
                        ref domainLength,
                        out _);

                if (errorReturn == 0)
                {
                    // http://msdn.microsoft.com/en-us/library/ms681382(VS.85).aspx.

                    var lastWin32Error = Marshal.GetLastWin32Error();
                    throw new Win32Exception(
                        lastWin32Error,
                        string.Format(
                            Resources.ErrorLookingUpSid,
                            lastWin32Error,
                            filePath,
                            CheckAddDotEnd(new Win32Exception(lastWin32Error).Message)));
                }
                else
                {
                    buffer.Append(domain);
                    buffer.Append(@"\");
                    buffer.Append(account);
                    return buffer.ToString();
                }
            }
            else
            {
                // http://msdn.microsoft.com/en-us/library/ms681382(VS.85).aspx.

                var lastWin32Error = Marshal.GetLastWin32Error();
                throw new Win32Exception(
                    lastWin32Error,
                    string.Format(
                        Resources.ErrorGettingSecurityInfo,
                        lastWin32Error,
                        filePath,
                        CheckAddDotEnd(new Win32Exception(lastWin32Error).Message)));
            }
        }

        public static void SetFileAttributes(string filePath, FileAttributes attributes)
        {
            if (!FileExists(filePath) && !DirectoryExists(filePath))
            {
                throw new FileNotFoundException(Resources.ErrorFileDoesNotExist, filePath);
            }

            filePath = CheckAddLongPathPrefix(filePath);

            if (!PInvokeHelper.SetFileAttributes(filePath, attributes))
            {
                // http://msdn.microsoft.com/en-us/library/ms681382(VS.85).aspx.

                var lastWin32Error = Marshal.GetLastWin32Error();
                throw new Win32Exception(
                    lastWin32Error,
                    string.Format(
                        Resources.ErrorSettingAttributes,
                        lastWin32Error,
                        filePath,
                        attributes,
                        CheckAddDotEnd(new Win32Exception(lastWin32Error).Message)));
            }
        }

        public static FileAttributes GetFileAttributes(string filePath)
        {
            if (!FileExists(filePath) && !DirectoryExists(filePath))
            {
                throw new FileNotFoundException(Resources.ErrorFileDoesNotExist, filePath);
            }

            filePath = CheckAddLongPathPrefix(filePath);

            return (FileAttributes)PInvokeHelper.GetFileAttributes(filePath);
        }

        public static void DeleteFile(string filePath)
        {
            filePath = CheckAddLongPathPrefix(filePath);

            if (!PInvokeHelper.DeleteFile(filePath))
            {
                // http://msdn.microsoft.com/en-us/library/ms681382(VS.85).aspx.

                var lastWin32Error = Marshal.GetLastWin32Error();
                if (lastWin32Error != PInvokeHelper.ERROR_NO_MORE_FILES &&
                    lastWin32Error != PInvokeHelper.ERROR_FILE_NOT_FOUND)
                {
                    // Sometimes it returns "ERROR_SUCCESS" and stil deletes the file.
                    var t = lastWin32Error != PInvokeHelper.ERROR_SUCCESS || FileExists(filePath);

                    // --

                    if (t)
                    {
                        throw new Win32Exception(
                            lastWin32Error,
                            string.Format(
                                Resources.ErrorDeletingFile,
                                lastWin32Error,
                                filePath,
                                CheckAddDotEnd(new Win32Exception(lastWin32Error).Message)));
                    }
                }
            }
        }

        public static void DeleteDirectory(string folderPath, bool recursive)
        {
            folderPath = CheckAddLongPathPrefix(folderPath);

            if (DirectoryExists(folderPath))
            {
                if (recursive)
                {
                    var files = GetFiles(folderPath);
                    var dirs = GetDirectories(folderPath);

                    foreach (var file in files)
                    {
                        DeleteFile(file.FullName);
                    }

                    foreach (var dir in dirs)
                    {
                        DeleteDirectory(dir.FullName, true);
                    }
                }

                if (!PInvokeHelper.RemoveDirectory(folderPath))
                {
                    {
                        // http://msdn.microsoft.com/en-us/library/ms681382(VS.85).aspx.

                        var lastWin32Error = Marshal.GetLastWin32Error();
                        if (lastWin32Error != PInvokeHelper.ERROR_NO_MORE_FILES)
                        {
                            throw new Win32Exception(
                                lastWin32Error,
                                string.Format(
                                    Resources.ErrorDeletingFolder,
                                    lastWin32Error,
                                    folderPath,
                                    CheckAddDotEnd(new Win32Exception(lastWin32Error).Message)));
                        }
                    }
                }
            }
        }

        public static void DeleteDirectoryContents(string folderPath, bool recursive)
        {
            folderPath = CheckAddLongPathPrefix(folderPath);

            if (DirectoryExists(folderPath))
            {
                if (recursive)
                {
                    var files = GetFiles(folderPath);
                    var dirs = GetDirectories(folderPath);

                    foreach (var file in files)
                    {
                        DeleteFile(file.FullName);
                    }

                    foreach (var dir in dirs)
                    {
                        DeleteDirectory(dir.FullName, true);
                    }
                }
            }
        }

        public static bool FileExists(string filePath)
        {
            filePath = CheckAddLongPathPrefix(filePath);

            var wIn32FileAttributeData = default(PInvokeHelper.WIN32_FILE_ATTRIBUTE_DATA);

            var b = PInvokeHelper.GetFileAttributesEx(filePath, 0, ref wIn32FileAttributeData);
            return b &&
                   wIn32FileAttributeData.dwFileAttributes != -1 &&
                   (wIn32FileAttributeData.dwFileAttributes & 16) == 0;

            // --

            //var a = PInvokeHelper.GetFileAttributes(filePath);
            //if ((a & PInvokeHelper.INVALID_FILE_ATTRIBUTES) == PInvokeHelper.INVALID_FILE_ATTRIBUTES)
            //{
            //    return false;
            //}
            //else
            //{
            //    return (a & PInvokeHelper.FILE_ATTRIBUTE_DIRECTORY) == 0;
            //}

            // --

            //filePath = CheckAddLongPathPrefix(filePath);

            //PInvokeHelper.WIN32_FIND_DATA fd;
            //var result = PInvokeHelper.FindFirstFile(filePath.TrimEnd('\\'), out fd);

            //if (result.ToInt32() == PInvokeHelper.ERROR_FILE_NOT_FOUND || result == PInvokeHelper.INVALID_HANDLE_VALUE)
            //{
            //    return false;
            //}
            //else
            //{
            //    return ((int)fd.dwFileAttributes & PInvokeHelper.FILE_ATTRIBUTE_DIRECTORY) == 0;
            //}
        }

        public static void CreateDirectory(string directoryPath)
        {
            // https://referencesource.microsoft.com/#mscorlib/system/io/directory.cs,214

            if (string.IsNullOrEmpty(directoryPath))
            {
                throw new ArgumentNullException(nameof(directoryPath));
            }

            // Wenn schon vorhanden, direkt fertig.
            if (DirectoryExists(directoryPath))
            {
                return;
            }

            splitFolderPath(directoryPath, out var basePart, out var childParts);

            var path = basePart;

            var index = 0;
            foreach (var childPart in childParts)
            {
                path = combine(path, childPart);

                if (!DirectoryExists(path))
                {
                    doCreateDirectory(path, index >= childParts.Length - 1);
                }

                index++;
            }
        }

        private static string combine(
            string path1,
            string path2)
        {
            if (string.IsNullOrEmpty(path1))
            {
                return path2;
            }
            else if (string.IsNullOrEmpty(path2))
            {
                return path1;
            }
            else
            {
                path1 = path1.TrimEnd('\\', '/').Replace('/', '\\');
                path2 = path2.TrimStart('\\', '/').Replace('/', '\\');

                return path1 + @"\" + path2;
            }
        }

        private static void splitFolderPath(
            string directoryPath,
            out string basePart,
            out string[] childParts)
        {
            directoryPath = ForceRemoveLongPathPrefix(directoryPath);

            basePart = getDriveOrShare(directoryPath);

            var remaining = directoryPath.Substring(basePart.Length);
            childParts = remaining.Trim('\\').Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
        }

        private static string getDrive(
            string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }
            else
            {
                var colonPos = path.IndexOf(':');
                var slashPos = path.IndexOf('\\');

                if (colonPos <= 0)
                {
                    return string.Empty;
                }
                else
                {
                    if (slashPos < 0 || slashPos > colonPos)
                    {
                        return path.Substring(0, colonPos + 1);
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
            }
        }

        private static string getShare(
            string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }
            else
            {
                var str = path;

                // Nach Doppel-Slash suchen.
                // Kann z.B. "\\server\share\" sein,
                // aber auch "http:\\www.xyz.com\".
                const string dblslsh = @"\\";
                var n = str.IndexOf(dblslsh, StringComparison.Ordinal);
                if (n < 0)
                {
                    return string.Empty;
                }
                else
                {
                    // Übernehme links von Doppel-Slash alles in Rückgabe
                    // (inkl. Doppel-Slash selbst).
                    var ret = str.Substring(0, n + dblslsh.Length);
                    str = str.Remove(0, n + dblslsh.Length);

                    // Jetzt nach Slash nach Server-Name suchen.
                    // Dieser Slash darf nicht unmittelbar nach den 2 Anfangsslash stehen.
                    n = str.IndexOf('\\');
                    if (n <= 0)
                    {
                        return string.Empty;
                    }
                    else
                    {
                        // Wiederum übernehmen in Rückgabestring.
                        ret += str.Substring(0, n + 1);
                        str = str.Remove(0, n + 1);

                        // Jetzt nach Slash nach Share-Name suchen.
                        // Dieser Slash darf ebenfalls nicht unmittelbar
                        // nach dem jetzigen Slash stehen.
                        n = str.IndexOf('\\');
                        switch (n)
                        {
                            case < 0:
                                n = str.Length;
                                break;
                            case 0:
                                return string.Empty;
                        }

                        // Wiederum übernehmen in Rückgabestring,
                        // aber ohne letzten Slash.
                        ret += str.Substring(0, n);

                        // The last item must not be a slash.
                        return ret[ret.Length - 1] == '\\' ? string.Empty : ret;
                    }
                }
            }
        }

        private static string getDriveOrShare(
            string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }
            else
            {
                if (!string.IsNullOrEmpty(getDrive(path)))
                {
                    return getDrive(path);
                }
                else if (!string.IsNullOrEmpty(getShare(path)))
                {
                    return getShare(path);
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        private static void doCreateDirectory(string directoryPath, bool isLastPart)
        {
            directoryPath = CheckAddLongPathPrefix(directoryPath);

            if (!PInvokeHelper.CreateDirectory(directoryPath, IntPtr.Zero))
            {
                // http://msdn.microsoft.com/en-us/library/ms681382(VS.85).aspx.

                var lastWin32Error = Marshal.GetLastWin32Error();

                // Kann bereits vorhanden sein, oder keine Berechtigung haben,
                // oder auch eine Datei sein.
                if (lastWin32Error == ERROR_ALREADY_EXISTS)
                {
                    if (FileExists(directoryPath))
                    {
                        // Fehler auslösen.
                    }
                    else
                    {
                        if (!internalExists(directoryPath, out var currentError) && currentError == ERROR_ACCESS_DENIED)
                        {
                            lastWin32Error = currentError;
                        }
                        else
                        {
                            // Weiter machen erlauben.
                            return;
                        }
                    }
                }

                if (isLastPart)
                {
                    throw new Win32Exception(
                        lastWin32Error,
                        string.Format(
                            Resources.ErrorCreatingDirectory,
                            lastWin32Error,
                            directoryPath,
                            CheckAddDotEnd(new Win32Exception(lastWin32Error).Message)));
                }
                else
                {
                    // Ansonsten einfach weiter probieren; es kann auf dem aktuellen Ordner zwar
                    // nicht möglich sein, jedoch im Unterordner.
                }
            }
        }

        private const int ERROR_ACCESS_DENIED = 0x05;
        private const int ERROR_ALREADY_EXISTS = 0xB7;

        public static bool DirectoryIsEmpty(string directoryPath)
        {
            // TODO: Optimize with something like http://stackoverflow.com/a/757925/107625

            return
                string.IsNullOrEmpty(directoryPath) ||
                !DirectoryExists(directoryPath) ||
                GetFiles(directoryPath).Length <= 0 &&
                GetDirectories(directoryPath).Length <= 0;
        }

        public static bool DirectoryExists(string directoryPath)
        {
            return internalExists(directoryPath, out _);
        }

        private static bool internalExists(string directoryPath, out int lastError)
        {
            directoryPath = CheckAddLongPathPrefix(directoryPath);

            var data = default(PInvokeHelper.WIN32_FILE_ATTRIBUTE_DATA);

            var b = PInvokeHelper.GetFileAttributesEx(directoryPath, 0, ref data);
            lastError = Marshal.GetLastWin32Error();

            return b &&
                   data.dwFileAttributes != -1 &&
                   (data.dwFileAttributes & 16) != 0;
        }

        [PublicAPI]
        [CanBeNull]
        public static ZlpFileDateInfos GetFileDateInfos(
            string filePath)
        {
            filePath = CheckAddLongPathPrefix(filePath);

            var result = PInvokeHelper.FindFirstFile(filePath.TrimEnd('\\'), out var fd);

            if (result == PInvokeHelper.INVALID_HANDLE_VALUE)
            {
                return null;
            }

            try
            {
                if (result.ToInt64() == PInvokeHelper.ERROR_FILE_NOT_FOUND)
                {
                    return null;
                }
                else
                {
                    var r = new ZlpFileDateInfos();

                    if (true)
                    {
                        var ft = fd.ftLastWriteTime;
                        var hft2 = ((long)ft.dwHighDateTime << 32) + ft.dwLowDateTime;
                        r.LastWriteTime = getLocalTime(hft2);
                    }

                    if (true)
                    {
                        var ft = fd.ftLastAccessTime;
                        var hft2 = ((long)ft.dwHighDateTime << 32) + ft.dwLowDateTime;
                        r.LastAccessTime = getLocalTime(hft2);
                    }

                    if (true)
                    {
                        var ft = fd.ftCreationTime;
                        var hft2 = ((long)ft.dwHighDateTime << 32) + ft.dwLowDateTime;
                        r.CreationTime = getLocalTime(hft2);
                    }

                    return r;
                }
            }
            finally
            {
                PInvokeHelper.FindClose(result);
            }
        }

        [PublicAPI]
        public static void SetFileDateInfos(
            string filePath,
            ZlpFileDateInfos infos)
        {
            if (infos == null) throw new ArgumentNullException(nameof(infos));

            if (MustBeLongPath(filePath))
            {
                filePath = CheckAddLongPathPrefix(filePath);

                using var handle = CreateFileHandle(
                    filePath,
                    CreationDisposition.OpenExisting,
                    // Fix by Richard, 2015-04-14.
                    // See https://msdn.microsoft.com/en-us/library/windows/desktop/ms724933%28v=vs.85%29.aspx,
                    // See https://msdn.microsoft.com/en-us/library/windows/desktop/aa364399%28v=vs.85%29.aspx
                    FileAccess.FileWriteAttributes,
                    FileShare.Read | FileShare.Write);
                var dLastWrite = infos.LastWriteTime.ToFileTime();
                var dLastAccess = infos.LastAccessTime.ToFileTime();
                var dCreation = infos.CreationTime.ToFileTime();

                if (!PInvokeHelper.SetFileTime4(handle.DangerousGetHandle(), ref dCreation, ref dLastAccess,
                        ref dLastWrite))
                {
                    var lastWin32Error = Marshal.GetLastWin32Error();
                    var x = new Win32Exception(
                        lastWin32Error,
                        string.Format(
                            Resources.ErrorSettingsWriteTime,
                            lastWin32Error,
                            filePath,
                            CheckAddDotEnd(new Win32Exception(lastWin32Error).Message)))
                    {
                        Data =
                        {
                            [nameof(filePath)] = filePath
                        }
                    };

                    throw x;
                }
            }
            else
            {
                // 2012-08-29, Uwe Keim: Since we currently get Access Denied 5,
                // do use the .NET functions (which seem to not have these issues)
                // if possible.

                // Sergey Filippov: Item number 16314 from Codeplex issues fix
                if (File.Exists(filePath))
                {
                    File.SetLastWriteTime(filePath, infos.LastWriteTime);
                    File.SetLastAccessTime(filePath, infos.LastAccessTime);
                    File.SetCreationTime(filePath, infos.CreationTime);
                }
                else if (Directory.Exists(filePath))
                {
                    Directory.SetLastWriteTime(filePath, infos.LastWriteTime);
                    Directory.SetLastAccessTime(filePath, infos.LastAccessTime);
                    Directory.SetCreationTime(filePath, infos.CreationTime);
                }
                else
                {
                    var x = new FileNotFoundException(Resources.FileNotFound, filePath)
                    {
                        Data =
                        {
                            [nameof(filePath)] = filePath
                        }
                    };

                    throw x;
                }
            }
        }

        public static DateTime GetFileLastWriteTime(
            string filePath)
        {
            filePath = CheckAddLongPathPrefix(filePath);

            var result = PInvokeHelper.FindFirstFile(filePath.TrimEnd('\\'), out var fd);

            if (result == PInvokeHelper.INVALID_HANDLE_VALUE)
            {
                return DateTime.MinValue;
            }

            try
            {
                if (result.ToInt64() == PInvokeHelper.ERROR_FILE_NOT_FOUND)
                {
                    return DateTime.MinValue;
                }
                else
                {
                    var ft = fd.ftLastWriteTime;

                    var hft2 = ((long)ft.dwHighDateTime << 32) + ft.dwLowDateTime;
                    return getLocalTime(hft2);
                }
            }
            finally
            {
                PInvokeHelper.FindClose(result);
            }
        }

        public static DateTime GetFileLastAccessTime(
            string filePath)
        {
            filePath = CheckAddLongPathPrefix(filePath);

            var result = PInvokeHelper.FindFirstFile(filePath.TrimEnd('\\'), out var fd);

            if (result == PInvokeHelper.INVALID_HANDLE_VALUE)
            {
                return DateTime.MinValue;
            }

            try
            {
                if (result.ToInt64() == PInvokeHelper.ERROR_FILE_NOT_FOUND)
                {
                    return DateTime.MinValue;
                }
                else
                {
                    var ft = fd.ftLastAccessTime;

                    var hft2 = ((long)ft.dwHighDateTime << 32) + ft.dwLowDateTime;
                    return getLocalTime(hft2);
                }
            }
            finally
            {
                PInvokeHelper.FindClose(result);
            }
        }

        public static DateTime GetFileCreationTime(
            string filePath)
        {
            filePath = CheckAddLongPathPrefix(filePath);

            var result = PInvokeHelper.FindFirstFile(filePath.TrimEnd('\\'), out var fd);

            if (result == PInvokeHelper.INVALID_HANDLE_VALUE)
            {
                return DateTime.MinValue;
            }

            try
            {
                if (result.ToInt64() == PInvokeHelper.ERROR_FILE_NOT_FOUND)
                {
                    return DateTime.MinValue;
                }
                else
                {
                    var ft = fd.ftCreationTime;

                    var hft2 = ((long)ft.dwHighDateTime << 32) + ft.dwLowDateTime;
                    return getLocalTime(hft2);
                }
            }
            finally
            {
                PInvokeHelper.FindClose(result);
            }
        }

        private static DateTime getLocalTime(long utcFileTime)
        {
            return DateTime.FromFileTime(utcFileTime);
            //return DateTime.FromFileTimeUtc(utcFileTime);
        }

        public static void SetFileLastWriteTime(
            string filePath,
            DateTime date)
        {
            if (MustBeLongPath(filePath))
            {
                filePath = CheckAddLongPathPrefix(filePath);

                using var handle = CreateFileHandle(
                    filePath,
                    CreationDisposition.OpenExisting,
                    // Fix by Richard, 2015-04-14.
                    // See https://msdn.microsoft.com/en-us/library/windows/desktop/ms724933%28v=vs.85%29.aspx,
                    // See https://msdn.microsoft.com/en-us/library/windows/desktop/aa364399%28v=vs.85%29.aspx
                    FileAccess.FileWriteAttributes,
                    FileShare.Read | FileShare.Write);
                var d = date.ToFileTime();

                if (!PInvokeHelper.SetFileTime3(handle.DangerousGetHandle(), IntPtr.Zero, IntPtr.Zero, ref d))
                {
                    var lastWin32Error = Marshal.GetLastWin32Error();
                    var x = new Win32Exception(
                        lastWin32Error,
                        string.Format(
                            Resources.ErrorSettingsWriteTime,
                            lastWin32Error,
                            filePath,
                            CheckAddDotEnd(new Win32Exception(lastWin32Error).Message)))
                    {
                        Data =
                        {
                            [nameof(filePath)] = filePath,
                            [nameof(date)] = date
                        }
                    };

                    throw x;
                }
            }
            else
            {
                // 2012-08-29, Uwe Keim: Since we currently get Access Denied 5,
                // do use the .NET functions (which seem to not have these issues)
                // if possible.

                // Sergey Filippov: Item number 16314 from Codeplex issues fix
                if (File.Exists(filePath))
                {
                    File.SetLastWriteTime(filePath, date);
                }
                else if (Directory.Exists(filePath))
                {
                    Directory.SetLastWriteTime(filePath, date);
                }
                else
                {
                    var x = new FileNotFoundException(Resources.FileNotFound, filePath)
                    {
                        Data =
                        {
                            [nameof(filePath)] = filePath,
                            [nameof(date)] = date
                        }
                    };

                    throw x;
                }
            }
        }

        public static void SetFileLastAccessTime(
            string filePath,
            DateTime date)
        {
            if (MustBeLongPath(filePath))
            {
                filePath = CheckAddLongPathPrefix(filePath);

                using var handle = CreateFileHandle(
                    filePath,
                    CreationDisposition.OpenExisting,
                    // Fix by Richard, 2015-04-14.
                    // See https://msdn.microsoft.com/en-us/library/windows/desktop/ms724933%28v=vs.85%29.aspx,
                    // See https://msdn.microsoft.com/en-us/library/windows/desktop/aa364399%28v=vs.85%29.aspx
                    FileAccess.FileWriteAttributes,
                    FileShare.Read | FileShare.Write);
                var d = date.ToFileTime();

                if (!PInvokeHelper.SetFileTime2(handle.DangerousGetHandle(), IntPtr.Zero, ref d, IntPtr.Zero))
                {
                    var lastWin32Error = Marshal.GetLastWin32Error();
                    var x = new Win32Exception(
                        lastWin32Error,
                        string.Format(
                            Resources.ErrorSettingAccessTime,
                            lastWin32Error,
                            filePath,
                            CheckAddDotEnd(new Win32Exception(lastWin32Error).Message)))
                    {
                        Data =
                        {
                            [nameof(filePath)] = filePath,
                            [nameof(date)] = date
                        }
                    };

                    throw x;
                }
            }
            else
            {
                // 2012-08-29, Uwe Keim: Since we currently get Access Denied 5,
                // do use the .NET functions (which seem to not have these issues)
                // if possible.

                // Sergey Filippov: Item number 16314 from Codeplex issues fix
                if (File.Exists(filePath))
                {
                    File.SetLastAccessTime(filePath, date);
                }
                else if (Directory.Exists(filePath))
                {
                    Directory.SetLastAccessTime(filePath, date);
                }
                else
                {
                    var x = new FileNotFoundException(Resources.FileNotFound, filePath)
                    {
                        Data =
                        {
                            [nameof(filePath)] = filePath,
                            [nameof(date)] = date
                        }
                    };

                    throw x;
                }
            }
        }

        public static void SetFileCreationTime(
            string filePath,
            DateTime date)
        {
            if (MustBeLongPath(filePath))
            {
                filePath = CheckAddLongPathPrefix(filePath);

                using var handle = CreateFileHandle(
                    filePath,
                    CreationDisposition.OpenExisting,
                    // Fix by Richard, 2015-04-14.
                    // See https://msdn.microsoft.com/en-us/library/windows/desktop/ms724933%28v=vs.85%29.aspx,
                    // See https://msdn.microsoft.com/en-us/library/windows/desktop/aa364399%28v=vs.85%29.aspx
                    FileAccess.FileWriteAttributes,
                    FileShare.Read | FileShare.Write);
                var d = date.ToFileTime();

                if (!PInvokeHelper.SetFileTime1(handle.DangerousGetHandle(), ref d, IntPtr.Zero, IntPtr.Zero))
                {
                    var lastWin32Error = Marshal.GetLastWin32Error();
                    var x = new Win32Exception(
                        lastWin32Error,
                        string.Format(
                            Resources.ErrorSettingCreationTime,
                            lastWin32Error,
                            filePath,
                            CheckAddDotEnd(new Win32Exception(lastWin32Error).Message)))
                    {
                        Data =
                        {
                            [nameof(filePath)] = filePath,
                            [nameof(date)] = date
                        }
                    };

                    throw x;
                }
            }
            else
            {
                // 2012-08-29, Uwe Keim: Since we currently get Access Denied 5,
                // do use the .NET functions (which seem to not have these issues)
                // if possible.

                // Sergey Filippov: Item number 16314 from Codeplex issues fix
                if (File.Exists(filePath))
                {
                    File.SetCreationTime(filePath, date);
                }
                else if (Directory.Exists(filePath))
                {
                    Directory.SetCreationTime(filePath, date);
                }
                else
                {
                    var x = new FileNotFoundException(Resources.FileNotFound, filePath)
                    {
                        Data =
                        {
                            [nameof(filePath)] = filePath,
                            [nameof(date)] = date
                        }
                    };

                    throw x;
                }
            }
        }

        //private static void doGetFileTime(
        //    string filePath,
        //    DateTime creationTime,
        //    DateTime lastAccessTime,
        //    DateTime lastWriteTime)
        //{
        //}

        //private static void doSetFileTime(
        //    string filePath,
        //    DateTime creationTime,
        //    DateTime lastAccessTime,
        //    DateTime lastWriteTime)
        //{
        //    filePath = CheckAddLongPathPrefix(filePath);

        //    using (var handle = CreateFileHandle(
        //        filePath,
        //        CreationDisposition.OpenExisting, FileAccess.GenericAll,
        //        FileShare.Read | FileShare.Write))
        //    {
        //        var d1 = creationTime.ToFileTime();
        //        var d2 = lastAccessTime.ToFileTime();
        //        var d3 = lastWriteTime.ToFileTime();

        //        if (!PInvokeHelper.SetFileTime(handle.DangerousGetHandle(), ref d1, ref d2, ref d3))
        //        {
        //            var lastWin32Error = Marshal.GetLastWin32Error();
        //            throw new Win32Exception(
        //                lastWin32Error,
        //                string.Format(
        //                    "Error {0} setting file time '{1}': {2}",
        //                    lastWin32Error,
        //                    filePath,
        //                    CheckAddDotEnd(new Win32Exception(lastWin32Error).Message)));
        //        }
        //    }
        //}

        [SecurityCritical]
        [SecuritySafeCritical]
        public static long GetFileLength(string filePath)
        {
#if WANT_TRACE
            Trace.TraceInformation(@"About to get file length for path '{0}'.", filePath);
#endif

            // 2014-06-10, Uwe Keim: Weil das auf 64-bit-Windows 8 nicht sauber läuft,
            // zunächst mal bei kürzeren Pfaden die eingebaute Methode nehmen.
            if (!MustBeLongPath(filePath))
            {
                return new FileInfo(filePath).Length;
            }

            filePath = CheckAddLongPathPrefix(filePath);

            var result = PInvokeHelper.FindFirstFile(filePath.TrimEnd('\\'), out var fd);

            if (result == PInvokeHelper.INVALID_HANDLE_VALUE)
            {
                return 0;
            }

            try
            {
                if (result.ToInt64() == PInvokeHelper.ERROR_FILE_NOT_FOUND)
                {
                    return 0;
                }
                else
                {
                    using var sfh = new SafeFileHandle(result, false);
                    if (sfh.IsInvalid)
                    {
                        var num = Marshal.GetLastWin32Error();
                        if (num is 2 or 3 or 21)
                            // http://msdn.microsoft.com/en-us/library/windows/desktop/ms681382(v=vs.85).aspx
                        {
                            return 0;
                        }
                        else
                        {
                            return 0;
                        }
                    }

                    // http://zetalongpaths.codeplex.com/discussions/580478#post1351470
                    // https://mcdrummerman.wordpress.com/2010/07/13/win32_find_data-and-negative-file-sizes/

                    //store nFileSizeLow
                    var fDataFSize = (long)fd.nFileSizeLow;

                    //store individual file size for later accounting usage
                    long fileSize;

                    if (fDataFSize < 0 && (long)fd.nFileSizeHigh > 0)
                    {
                        fileSize = fDataFSize + 0x100000000 + fd.nFileSizeHigh * 0x100000000;
                    }
                    else
                    {
                        if ((long)fd.nFileSizeHigh > 0)
                        {
                            fileSize = fDataFSize + fd.nFileSizeHigh * 0x100000000;
                        }
                        else if (fDataFSize < 0)
                        {
                            fileSize = fDataFSize + 0x100000000;
                        }
                        else
                        {
                            fileSize = fDataFSize;
                        }
                    }

                    return fileSize;




                    /*
                        var low = fd.nFileSizeLow;
                        var high = fd.nFileSizeHigh;
    
                        //return (high * (0xffffffff + 1)) + low;
                        //return (((ulong)high) << 32) + low;
                        var l = ((high << 0x20) | (low & 0xffffffffL));
                            // Copied from FileInfo.Length via Reflector.NET.
                        return (ulong) l;*/

                    var low = fd.nFileSizeLow;
                    var high = fd.nFileSizeHigh;

#if WANT_TRACE
                    Trace.TraceInformation(@"FindFirstFile returned LOW = {0}, HIGH = {1}.", low, high);
                    Trace.Flush();
#endif

                    try
                    {
                        return (long)high << 32 | (low & 0xffffffffL);

                        //try
                        //{
                        //var sign = ((long) high << 32 | (low & 0xffffffffL));

                        //try
                        //{
                        //return sign <= 0 ? 0 : unchecked((ulong) sign);
                        //}
                        //    catch (OverflowException x)
                        //    {
                        //        var y = new OverflowException(@"Error getting file length (cast).", x);

                        //        y.Data[@"low"] = low;
                        //        y.Data[@"high"] = high;
                        //        y.Data[@"signed value"] = sign;

                        //        throw y;
                        //    }
                        //}
                        //catch (OverflowException x)
                        //{
                        //    var y = new OverflowException(@"Error getting file length (sign).", x);

                        //    y.Data[@"low"] = low;
                        //    y.Data[@"high"] = high;

                        //    throw y;
                        //}
                    }
                    catch (OverflowException x)
                    {
#if WANT_TRACE
                        Trace.TraceInformation(
                            @"Got overflow exception ('{3}') for path '{0}'. LOW = {1}, HIGH = {2}.", filePath, low,
                            high, x.Message);
                        Trace.Flush();
#endif

                        throw;
                    }
                }
            }
            finally
            {
                PInvokeHelper.FindClose(result);
            }
        }

        public static ZlpFileInfo[] GetFiles(string directoryPath, string pattern = @"*.*")
        {
            return GetFiles(directoryPath, pattern, SearchOption.TopDirectoryOnly);
        }

        public static ZlpFileInfo[] GetFiles(string directoryPath, SearchOption searchOption)
        {
            return GetFiles(directoryPath, @"*.*", searchOption);
        }

        public static ZlpFileInfo[] GetFiles(string directoryPath, string pattern, SearchOption searchOption)
        {
            if (directoryPath == null) throw new ArgumentNullException(nameof(directoryPath));
            if (pattern == null) throw new ArgumentNullException(nameof(pattern));

            directoryPath = CheckAddLongPathPrefix(directoryPath);

            var results = new List<ZlpFileInfo>();
            var findHandle =
                PInvokeHelper.FindFirstFile(directoryPath.TrimEnd('\\') + "\\" + pattern, out var findData);

            if (findHandle != PInvokeHelper.INVALID_HANDLE_VALUE)
            {
                try
                {
                    bool found;
                    do
                    {
                        var currentFileName = findData.cFileName;

                        // if this is a file, find its contents
                        if (((int)findData.dwFileAttributes & PInvokeHelper.FILE_ATTRIBUTE_DIRECTORY) == 0)
                        {
                            results.Add(new ZlpFileInfo(ZlpPathHelper.Combine(directoryPath, currentFileName)));
                        }

                        // find next
                        found = PInvokeHelper.FindNextFile(findHandle, out findData);
                    } while (found);
                }
                finally
                {
                    // close the find handle
                    PInvokeHelper.FindClose(findHandle);
                }
            }

            if (searchOption == SearchOption.AllDirectories)
            {
                foreach (var dir in GetDirectories(directoryPath))
                {
                    results.AddRange(GetFiles(dir.FullName, pattern, searchOption));
                }
            }

            return results.ToArray();
        }

        public static ZlpDirectoryInfo[] GetDirectories(string directoryPath, string pattern = @"*")
        {
            return GetDirectories(directoryPath, pattern, SearchOption.TopDirectoryOnly);
        }

        public static ZlpDirectoryInfo[] GetDirectories(string directoryPath, SearchOption searchOption)
        {
            return GetDirectories(directoryPath, @"*", searchOption);
        }

        public static IZlpFileSystemInfo[] GetFileSystemInfos(string directoryPath, string pattern = @"*.*")
        {
            return GetFileSystemInfos(directoryPath, pattern, SearchOption.TopDirectoryOnly);
        }

        public static IZlpFileSystemInfo[] GetFileSystemInfos(string directoryPath, SearchOption searchOption)
        {
            return GetFileSystemInfos(directoryPath, @"*.*", searchOption);
        }

        public static IZlpFileSystemInfo[] GetFileSystemInfos(string directoryPath, string pattern,
            SearchOption searchOption)
        {
            directoryPath = CheckAddLongPathPrefix(directoryPath);

            var results = new List<IZlpFileSystemInfo>();
            var findHandle =
                PInvokeHelper.FindFirstFile(directoryPath.TrimEnd('\\') + @"\" + pattern, out var findData);

            if (findHandle != PInvokeHelper.INVALID_HANDLE_VALUE)
            {
                try
                {
                    bool found;
                    do
                    {
                        var currentFileName = findData.cFileName;

                        // if this is a directory, find its contents
                        if (((int)findData.dwFileAttributes & PInvokeHelper.FILE_ATTRIBUTE_DIRECTORY) != 0)
                        {
                            if (currentFileName != @"." && currentFileName != @"..")
                            {
                                results.Add(
                                    new ZlpDirectoryInfo(ZlpPathHelper.Combine(directoryPath, currentFileName)));
                            }
                        }
                        else
                        {
                            results.Add(new ZlpFileInfo(ZlpPathHelper.Combine(directoryPath, currentFileName)));
                        }

                        // find next
                        found = PInvokeHelper.FindNextFile(findHandle, out findData);
                    } while (found);
                }
                finally
                {
                    // close the find handle
                    PInvokeHelper.FindClose(findHandle);
                }
            }


            if (searchOption == SearchOption.AllDirectories)
            {
                foreach (var dir in GetDirectories(directoryPath))
                {
                    results.AddRange(GetFileSystemInfos(dir.FullName, pattern, searchOption));
                }
            }

            return results.ToArray();
        }

        public static ZlpDirectoryInfo[] GetDirectories(string directoryPath, string pattern, SearchOption searchOption)
        {
            directoryPath = CheckAddLongPathPrefix(directoryPath);

            var results = new List<ZlpDirectoryInfo>();
            var findHandle =
                PInvokeHelper.FindFirstFile(directoryPath.TrimEnd('\\') + @"\" + pattern, out var findData);

            if (findHandle != PInvokeHelper.INVALID_HANDLE_VALUE)
            {
                try
                {
                    bool found;
                    do
                    {
                        var currentFileName = findData.cFileName;

                        // if this is a directory, find its contents
                        if (((int)findData.dwFileAttributes & PInvokeHelper.FILE_ATTRIBUTE_DIRECTORY) != 0)
                        {
                            if (currentFileName != @"." && currentFileName != @"..")
                            {
                                results.Add(
                                    new ZlpDirectoryInfo(ZlpPathHelper.Combine(directoryPath, currentFileName)));
                            }
                        }

                        // find next
                        found = PInvokeHelper.FindNextFile(findHandle, out findData);
                    } while (found);
                }
                finally
                {
                    // close the find handle
                    PInvokeHelper.FindClose(findHandle);
                }
            }


            if (searchOption == SearchOption.AllDirectories)
            {
                foreach (var dir in GetDirectories(directoryPath))
                {
                    results.AddRange(GetDirectories(dir.FullName, pattern, searchOption));
                }
            }

            return results.ToArray();
        }

        /// <summary>
        /// The is current path longer, then supported by standard System.IO methods
        /// </summary>
        /// <param name="path">
        /// The path to check.
        /// </param>
        /// <returns>
        /// True, if path longer then MAX_PATH constant, or is UNC path, else - False
        /// </returns>
        [PublicAPI]
        public static bool IsPathLong(string path)
        {
            return MustBeLongPath(path);
        }

        private static bool MustBeLongPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }
            else if (path.StartsWith(@"\\?\"))
            {
                return true;
            }
            else if (path.Contains(@"~"))
            {
                // See https://github.com/UweKeim/ZetaLongPaths/issues/12
                // Example: "C:\\Users\\cliente\\Desktop\\DRIVES~2\\mdzip\\PASTAC~1\\SUBPAS~1\\PASTAC~1\\SUBPAS~1\\SUBDAS~1\\bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb.txt"
                return true;
            }
            else if (path.Length > PInvokeHelper.MAX_PATH)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        internal static string CheckAddLongPathPrefix(string path)
        {
            if (string.IsNullOrEmpty(path) || path.StartsWith(@"\\?\"))
            {
                return path;
            }
            else if (path.Length > PInvokeHelper.MAX_PATH ||
                     // See https://github.com/UweKeim/ZetaLongPaths/issues/12
                     // Example: "C:\\Users\\cliente\\Desktop\\DRIVES~2\\mdzip\\PASTAC~1\\SUBPAS~1\\PASTAC~1\\SUBPAS~1\\SUBDAS~1\\bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb.txt"
                     path.Contains(@"~"))
            {
                return ForceAddLongPathPrefix(path);
            }
            else
            {
                return path;
            }
        }

        public static string ForceRemoveLongPathPrefix(string path)
        {
            if (string.IsNullOrEmpty(path) || !path.StartsWith(@"\\?\"))
            {
                return path;
            }
            else if (path.StartsWith(@"\\?\UNC\", StringComparison.OrdinalIgnoreCase))
            {
                return @"\\" + path.Substring(@"\\?\UNC\".Length);
            }
            else
            {
                return path.Substring(@"\\?\".Length);
            }
        }

        private static string ForceAddLongPathPrefix(string path)
        {
            if (string.IsNullOrEmpty(path) || path.StartsWith(@"\\?\"))
            {
                return path;
            }
            else
            {
                // http://msdn.microsoft.com/en-us/library/aa365247.aspx

                if (path.StartsWith(@"\\"))
                {
                    // UNC.
                    return @"\\?\UNC\" + path.Substring(2);
                }
                else
                {
                    return @"\\?\" + path;
                }
            }
        }

        internal static string CheckAddDotEnd(
            string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return @".";
            }
            else
            {
                text = text.Trim();
                if (text.EndsWith(@"."))
                {
                    return text;
                }
                else
                {
                    return text + @".";
                }
            }
        }
    }

    public sealed class ZlpFileDateInfos
    {
        public DateTime CreationTime { get; set; }
        public DateTime LastWriteTime { get; set; }
        public DateTime LastAccessTime { get; set; }
    }
}