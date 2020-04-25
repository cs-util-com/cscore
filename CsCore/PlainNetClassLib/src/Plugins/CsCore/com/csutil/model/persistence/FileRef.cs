using System.Collections.Generic;
using Zio;
using System;
using System.Linq;

namespace com.csutil.model {

    public class FileRef {

        public string dir { get; set; }
        public string fileName { get; set; }
        public string url { get; set; }
        public List<CheckSum> checksums { get; set; }
        public string mimeType { get; set; }

        public UPath GetPath() { return (UPath)dir / fileName; }

        public void SetPath(FileEntry file) {
            UPath value = file.Path;
            dir = "" + value.GetDirectory();
            fileName = value.GetName();
        }

        public void AddCheckSum(string type, string hash) {
            if (hash == null) { throw new ArgumentNullException($"The passed {type}-hash was null"); }
            if (checksums == null) { checksums = new List<CheckSum>(); }
            checksums.Add(new CheckSum() { typ = type, hash = hash });
        }

        public bool HasMatchingChecksum(string hash) {
            return !hash.IsNullOrEmpty() && checksums != null && checksums.Any(x => x.hash == hash);
        }

        public class CheckSum {
            public const string TYPE_MD5 = "md5";
            public const string TYPE_SHA1 = "sha1";
            public const string TYPE_ETAG = "etag";

            public string hash { get; set; }
            public string typ { get; set; }
        }

    }

}