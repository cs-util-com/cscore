namespace ZetaLongPaths.Native
{
    internal static class PInvokeHelper
    {
        // http://zetalongpaths.codeplex.com/Thread/View.aspx?ThreadId=230652&ANCHOR#Post557779
        internal const int MAX_PATH = 247;

        // http://msdn.microsoft.com/en-us/library/ms681382(VS.85).aspx
        internal const int ERROR_SUCCESS = 0;
        internal const int ERROR_FILE_NOT_FOUND = 2;
        internal const int ERROR_NO_MORE_FILES = 18;

        // http://www.dotnet247.com/247reference/msgs/21/108780.aspx
        [DllImport(@"advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern int GetNamedSecurityInfo(
            string pObjectName,
            int objectType,
            int securityInfo,
            out IntPtr ppsidOwner,
            out IntPtr ppsidGroup,
            out IntPtr ppDacl,
            out IntPtr ppSacl,
            out IntPtr ppSecurityDescriptor);

        [DllImport(@"advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern int LookupAccountSid(
            string systemName,
            IntPtr psid,
            StringBuilder accountName,
            ref int cbAccount,
            [Out] StringBuilder domainName,
            ref int cbDomainName,
            out int use);

        public const int OwnerSecurityInformation = 1;
        public const int SeFileObject = 1;

        [StructLayout(LayoutKind.Sequential)]
        internal struct FILETIME
        {
            internal uint dwLowDateTime;
            internal uint dwHighDateTime;
        }

        [BestFitMapping(false)]
        //[Serializable]
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct WIN32_FIND_DATA
        {
            [MarshalAs(UnmanagedType.U4)] internal FileAttributes dwFileAttributes;
            internal FILETIME ftCreationTime;
            internal FILETIME ftLastAccessTime;
            internal FILETIME ftLastWriteTime;
            internal uint nFileSizeHigh;
            internal uint nFileSizeLow;
            internal uint dwReserved0;
            internal uint dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)] internal string cFileName;
            // not using this
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)] internal string cAlternate;
        }

        [DllImport(@"kernel32.dll", SetLastError = true)]
        internal static extern bool ReadFile(
            SafeFileHandle hFile,
            IntPtr lpBuffer,
            uint nNumberOfBytesToRead,
            out uint lpNumberOfBytesRead,
            IntPtr lpOverlapped);

        [DllImport(@"kernel32.dll", SetLastError = true)]
        internal static extern bool WriteFile(
            SafeFileHandle hFile,
            IntPtr lpBuffer,
            uint nNumberOfBytesToWrite,
            out uint lpNumberOfBytesWritten,
            IntPtr lpOverlapped);

        [DllImport(@"kernel32.dll", EntryPoint = @"SetFilePointerEx", SetLastError = true)]
        public static extern bool Seek(
            SafeFileHandle hFile,
            long distance,
            out long newFilePointer,
            FileSeekOrigin origin);

        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES
        {
            public int nLength;
            public IntPtr lpSecurityDescriptor;
            public int bInheritHandle;
        }

        //[DllImport(@"kernel32.dll", CharSet = CharSet.Unicode)]
        //[return: MarshalAs(UnmanagedType.Bool)]
        //internal static extern bool CopyFile(string lpExistingFileName, string lpNewFileName, bool bFailIfExists);

        [DllImport(@"kernel32.dll",
            CharSet = CharSet.Unicode,
            CallingConvention = CallingConvention.StdCall,
            SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CopyFile(
            [MarshalAs(UnmanagedType.LPTStr)] string lpExistingFileName,
            [MarshalAs(UnmanagedType.LPTStr)] string lpNewFileName,
            [MarshalAs(UnmanagedType.Bool)] bool bFailIfExists);

        [DllImport(@"kernel32.dll",
            CharSet = CharSet.Unicode,
            CallingConvention = CallingConvention.StdCall,
            SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool MoveFile(
            [MarshalAs(UnmanagedType.LPTStr)] string lpExistingFileName,
            [MarshalAs(UnmanagedType.LPTStr)] string lpNewFileName);

        [DllImport(@"kernel32.dll",
            CharSet = CharSet.Unicode,
            CallingConvention = CallingConvention.StdCall,
            SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool MoveFileEx(
            [MarshalAs(UnmanagedType.LPTStr)] string lpExistingFileName,
            [MarshalAs(UnmanagedType.LPTStr)] string lpNewFileName,
            MoveFileExFlags dwFlags);

        [DllImport(@"kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CreateDirectory(
            [MarshalAs(UnmanagedType.LPTStr)] string lpPathName,
            IntPtr lpSecurityAttributes);

        [DllImport(@"kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern uint GetFileAttributes(
            [MarshalAs(UnmanagedType.LPTStr)] string lpFileName);

        [DllImport(@"kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true, BestFitMapping = false)]
        internal static extern bool GetFileAttributesEx(
            [MarshalAs(UnmanagedType.LPTStr)] string lpFileName,
            int fInfoLevelId,
            ref WIN32_FILE_ATTRIBUTE_DATA fileData);

        [StructLayout(LayoutKind.Sequential)]
        public struct WIN32_FILE_ATTRIBUTE_DATA
        {
            public int dwFileAttributes;
            public FILETIME ftCreationTime;
            public FILETIME ftLastAccessTime;
            public FILETIME ftLastWriteTime;
            public uint nFileSizeHigh;
            public uint nFileSizeLow;
        }

        [DllImport(@"kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetFileAttributes(
            [MarshalAs(UnmanagedType.LPTStr)] string lpFileName,
            [MarshalAs(UnmanagedType.U4)] FileAttributes dwFileAttributes);

        [DllImport(@"kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool RemoveDirectory(
            [MarshalAs(UnmanagedType.LPTStr)] string lpPathName);

        [DllImport(@"kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DeleteFile(
            [MarshalAs(UnmanagedType.LPTStr)] string lpFileName);

        [DllImport(@"kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern IntPtr FindFirstFile(
            [MarshalAs(UnmanagedType.LPTStr)] string lpFileName,
            out WIN32_FIND_DATA lpFindFileData);

        [DllImport(@"kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool FindNextFile(
            IntPtr hFindFile,
            out WIN32_FIND_DATA lpFindFileData);

        [DllImport(@"kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool FindClose(
            IntPtr hFindFile);

        [DllImport(@"kernel32.dll", SetLastError = true, EntryPoint = @"SetFileTime", ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetFileTime1(
            IntPtr hFile,
            ref long lpCreationTime,
            IntPtr lpLastAccessTime,
            IntPtr lpLastWriteTime);

        [DllImport(@"kernel32.dll", SetLastError = true, EntryPoint = @"SetFileTime", ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetFileTime2(
            IntPtr hFile,
            IntPtr lpCreationTime,
            ref long lpLastAccessTime,
            IntPtr lpLastWriteTime);

        [DllImport(@"kernel32.dll", SetLastError = true, EntryPoint = @"SetFileTime", ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetFileTime3(
            IntPtr hFile,
            IntPtr lpCreationTime,
            IntPtr lpLastAccessTime,
            ref long lpLastWriteTime);

        [DllImport(@"kernel32.dll", SetLastError = true, EntryPoint = @"SetFileTime", ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetFileTime4(
            IntPtr hFile,
            ref long lpCreationTime,
            ref long lpLastAccessTime,
            ref long lpLastWriteTime);

        [DllImport(@"kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern int GetFullPathName(
            [MarshalAs(UnmanagedType.LPTStr)] string lpFileName,
            int nBufferLength,
            /*[MarshalAs(UnmanagedType.LPTStr), Out]*/StringBuilder lpBuffer,
            IntPtr mustBeZero);

        [DllImport(@"kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern uint GetShortPathName(
            [MarshalAs(UnmanagedType.LPTStr)] string lpszLongPath,
            [MarshalAs(UnmanagedType.LPTStr), Out] StringBuilder lpszShortPath,
            uint cchBuffer);

        [DllImport(@"kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern uint GetLongPathName(
            [MarshalAs(UnmanagedType.LPTStr)] string lpszShortPath,
            [MarshalAs(UnmanagedType.LPTStr), Out] StringBuilder lpszLongPath,
            uint cchBuffer);

        //internal static extern uint GetFullPathName(
        //    string lpFileName,
        //    uint nBufferLength,
        //    [Out] StringBuilder lpBuffer,
        //    out StringBuilder lpFilePart);

        internal const int FILE_ATTRIBUTE_DIRECTORY = 0x00000010;
        internal const uint INVALID_FILE_ATTRIBUTES = 0xFFFFFFFF;

        internal static IntPtr INVALID_HANDLE_VALUE = new(-1);

        // Assume dirName passed in is already prefixed with \\?\
        public static List<string> FindFilesAndDirectories(
            string directoryPath)
        {
            var results = new List<string>();
            var findHandle = FindFirstFile(directoryPath.TrimEnd('\\') + @"\*", out var findData);

            try
            {
                if (findHandle != INVALID_HANDLE_VALUE)
                {
                    bool found;
                    do
                    {
                        var currentFileName = findData.cFileName;

                        // if this is a directory, find its contents
                        if (((int)findData.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY) != 0)
                        {
                            if (currentFileName != @"." && currentFileName != @"..")
                            {
                                var childResults = FindFilesAndDirectories(Path.Combine(directoryPath, currentFileName));
                                // add children and self to results
                                results.AddRange(childResults);
                                results.Add(Path.Combine(directoryPath, currentFileName));
                            }
                        }

                        // it's a file; add it to the results
                        else
                        {
                            results.Add(Path.Combine(directoryPath, currentFileName));
                        }

                        // find next
                        found = FindNextFile(findHandle, out findData);
                    } while (found);
                }
            }
            finally
            {
                // close the find handle
                FindClose(findHandle);
            }

            return results;
        }

        [DllImport(@"kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern SafeFileHandle CreateFile(
            [MarshalAs(UnmanagedType.LPTStr)] string lpFileName,
            FileAccess dwDesiredAccess,
            FileShare dwShareMode,
            IntPtr lpSecurityAttributes,
            CreationDisposition dwCreationDisposition,
            FileAttributes dwFlagsAndAttributes,
            IntPtr hTemplateFile);
    }
}