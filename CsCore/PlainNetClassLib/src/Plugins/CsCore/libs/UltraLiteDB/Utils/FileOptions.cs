namespace UltraLiteDB
{
	/// <summary>
	/// Datafile open options (for FileDiskService)
	/// </summary>
	public class FileOptions
    {
        public bool Journal { get; set; }
        public long InitialSize { get; set; }
        public long LimitSize { get; set; }
        public bool Async { get; set; }
        public bool Flush { get; set; } = false;

        public FileOptions()
        {
            this.Journal = true;
            this.InitialSize = 0;
            this.LimitSize = long.MaxValue;
            this.Flush = false;
        }
    }


}
