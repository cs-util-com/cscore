using System.Collections.Generic;

namespace com.csutil.model {

    public interface IFileRef {

        string dir { get; set; }
        string fileName { get; set; }
        string url { get; set; }
        Dictionary<string, object> checksums { get; set; }
        string mimeType { get; set; }

    }

}