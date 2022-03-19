namespace ZetaLongPaths
{
    public sealed class ZlpHandleExceptionInfo
    {
        [PublicAPI] public Exception Exception { get; }

        [PublicAPI] public int CurrentRetryCount { get; }

        public ZlpHandleExceptionInfo(Exception exception, int currentRetryCount)
        {
            Exception = exception;
            CurrentRetryCount = currentRetryCount;
        }

        /// <summary>
        /// Return value. Set optionally to TRUE to force premature throwing.
        /// </summary>
        [DefaultValue(false)]
        [PublicAPI]
        public bool WantThrow { get; set; }
    }
}