namespace ZetaLongPaths.Native
{
    public enum FileSeekOrigin : uint
    {
        Begin = 0,
        Current = 1,
        End = 2
    }

    [Flags]
    public enum FileAccess : uint
    {
        GenericRead = 0x80000000,
        GenericWrite = 0x40000000,
        GenericExecute = 0x20000000,
        GenericAll = 0x10000000,
        FileReadAttributes = 0x80, // https://msdn.microsoft.com/en-us/library/windows/desktop/gg258116(v=vs.85).aspx
        FileWriteAttributes = 0x100, // https://msdn.microsoft.com/en-us/library/windows/desktop/gg258116(v=vs.85).aspx
        FileAppendData = 4 // https://docs.microsoft.com/en-us/windows/win32/fileio/file-access-rights-constants
    }

    [Flags]
    public enum FileShare : uint
    {
        None = 0x00000000,
        Read = 0x00000001,
        Write = 0x00000002,
        Delete = 0x00000004
    }

    public enum CreationDisposition : uint
    {
        New = 1,
        CreateAlways = 2,
        OpenExisting = 3,
        OpenAlways = 4,
        TruncateExisting = 5
    }

    [Flags]
    public enum FileAttributes : uint
    {
        Readonly = 0x00000001,
        Hidden = 0x00000002,
        System = 0x00000004,
        Directory = 0x00000010,
        Archive = 0x00000020,
        Device = 0x00000040,
        Normal = 0x00000080,
        Temporary = 0x00000100,
        SparseFile = 0x00000200,
        ReparsePoint = 0x00000400,
        Compressed = 0x00000800,
        Offline = 0x00001000,
        NotContentIndexed = 0x00002000,
        Encrypted = 0x00004000,
        Write_Through = 0x80000000,
        Overlapped = 0x40000000,
        NoBuffering = 0x20000000,
        RandomAccess = 0x10000000,
        SequentialScan = 0x08000000,
        DeleteOnClose = 0x04000000,
        BackupSemantics = 0x02000000,
        PosixSemantics = 0x01000000,
        OpenReparsePoint = 0x00200000,
        OpenNoRecall = 0x00100000,
        FirstPipeInstance = 0x00080000
    }

    [Flags]
    public enum MoveFileExFlags : uint
    {
        // https://msdn.microsoft.com/en-us/library/windows/desktop/aa365240(v=vs.85).aspx
        None = 0x0,
        ReplaceExisting = 0x1,
        CopyAllowed = 0x2,
        DelayUntilReboot = 0x4,
        WriteThrough = 0x8,
        CreateHardlink = 0x10,
        FailIfNotTrackable = 0x20
    }
}