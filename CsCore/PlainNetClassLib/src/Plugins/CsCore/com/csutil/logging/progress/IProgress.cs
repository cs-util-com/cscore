using System;

namespace com.csutil.progress {

    public interface IProgress : IDisposable {

        /// <summary> Current progress % value between 0 and 100 </summary>
        double percent { get; set; }

        /// <summary> The total count that the progress uses to calc the % </summary>
        double totalCount { get; set; }

    }

}