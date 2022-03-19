namespace ZetaLongPaths.Native.FileOperations
{
    using Interop;

    public sealed class FileOperationProgressSink : IFileOperationProgressSink
    {
        public void StartOperations()
        {
            TraceAction(@"StartOperations", @"", 0);
        }

        public void FinishOperations(uint hrResult)
        {
            TraceAction(@"FinishOperations", @"", hrResult);
        }

        public void PreRenameItem(uint dwFlags,
            IShellItem psiItem, string pszNewName)
        {
            TraceAction(@"PreRenameItem", psiItem, 0);
        }

        public void PostRenameItem(uint dwFlags,
            IShellItem psiItem, string pszNewName,
            uint hrRename, IShellItem psiNewlyCreated)
        {
            TraceAction(@"PostRenameItem", psiNewlyCreated, hrRename);
        }

        public void PreMoveItem(
            uint dwFlags, IShellItem psiItem,
            IShellItem psiDestinationFolder, string pszNewName)
        {
            TraceAction(@"PreMoveItem", psiItem, 0);
        }

        public void PostMoveItem(
            uint dwFlags, IShellItem psiItem,
            IShellItem psiDestinationFolder,
            string pszNewName, uint hrMove,
            IShellItem psiNewlyCreated)
        {
            TraceAction(@"PostMoveItem", psiNewlyCreated, hrMove);
        }

        public void PreCopyItem(
            uint dwFlags, IShellItem psiItem,
            IShellItem psiDestinationFolder, string pszNewName)
        {
            TraceAction(@"PreCopyItem", psiItem, 0);
        }

        public void PostCopyItem(
            uint dwFlags, IShellItem psiItem,
            IShellItem psiDestinationFolder, string pszNewName,
            uint hrCopy, IShellItem psiNewlyCreated)
        {
            TraceAction(@"PostCopyItem", psiNewlyCreated, hrCopy);
        }

        public void PreDeleteItem(
            uint dwFlags, IShellItem psiItem)
        {
            TraceAction(@"PreDeleteItem", psiItem, 0);
        }

        public void PostDeleteItem(
            uint dwFlags, IShellItem psiItem,
            uint hrDelete, IShellItem psiNewlyCreated)
        {
            TraceAction(@"PostDeleteItem", psiItem, hrDelete);
        }

        public void PreNewItem(uint dwFlags,
            IShellItem psiDestinationFolder, string pszNewName)
        {
            TraceAction(@"PreNewItem", pszNewName, 0);
        }

        public void PostNewItem(uint dwFlags,
            IShellItem psiDestinationFolder, string pszNewName,
            string pszTemplateName, uint dwFileAttributes,
            uint hrNew, IShellItem psiNewItem)
        {
            TraceAction(@"PostNewItem", psiNewItem, hrNew);
        }

        public void UpdateProgress(
            uint iWorkTotal, uint iWorkSoFar)
        {
            Debug.WriteLine($@"UpdateProgress: {iWorkSoFar}/{iWorkTotal}");
        }

        public void ResetTimer() { }
        public void PauseTimer() { }
        public void ResumeTimer() { }

        [Conditional(@"DEBUG")]
        private static void TraceAction(
            string action, string item, uint hresult)
        {
            var message = $@"{action} ({ hresult})";
            // ReSharper disable once RedundantAssignment
            if (!string.IsNullOrEmpty(item)) message += $@" : {item}";
            // ReSharper disable once InvocationIsSkipped
            Debug.WriteLine(message);
        }

        [Conditional(@"DEBUG")]
        private static void TraceAction(
            string action, IShellItem item, uint hresult)
        {
            // ReSharper disable once InvocationIsSkipped
            TraceAction(action,
                item?.GetDisplayName(SIGDN.SIGDN_NORMALDISPLAY),
                hresult);
        }
    }
}