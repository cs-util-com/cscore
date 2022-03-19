namespace ZetaLongPaths
{
    public class ZlpSimpleFileAccessProtectorInformation
    {
        [PublicAPI] public static ZlpSimpleFileAccessProtectorInformation Default => new();

        [PublicAPI]
        public static int DefaultRetryCount =>
            ZlpSimpleFileAccessProtector.GetConfigIntOrDef(@"zlp.sfap.retryCount", 3);

        [PublicAPI]
        public static int DefaultSleepDelaySeconds =>
            ZlpSimpleFileAccessProtector.GetConfigIntOrDef(@"zlp.sfap.sleepDelaySeconds", 2);

        [PublicAPI] public bool Use { get; set; } = true;

        [PublicAPI] public string Info { get; set; }

        [PublicAPI] public int RetryCount { get; set; } = DefaultRetryCount;

        [PublicAPI] public int SleepDelaySeconds { get; set; } = DefaultSleepDelaySeconds;

        [PublicAPI] public bool DoGarbageCollectBeforeSleep { get; set; } = true;

        [PublicAPI] public ZlpHandleExceptionDelegate HandleException { get; set; }
    }
}