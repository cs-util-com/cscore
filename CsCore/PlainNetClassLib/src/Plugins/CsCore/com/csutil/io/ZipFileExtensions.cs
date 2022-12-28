using System.Collections.Generic;
using System.IO;
using System.Linq;
using com.csutil;

namespace ICSharpCode.SharpZipLib.Zip {

    public static class ZipFileExtensions {

        public static List<ZipEntryV2> GetEntries(this ZipFile z) {
            return z.GetEnumerator().ToIEnumerable<ZipEntry>().Map(e => new ZipEntryV2(z, e)).ToList();
        }

    }

    public class ZipEntryV2 {

        public readonly ZipFile ZipFile;
        public readonly ZipEntry ZipEntry;
        public Stream EntryStream => ZipEntry.IsFile ? ZipFile.GetInputStream(ZipEntry) : null;

        public ZipEntryV2(ZipFile zipFile, ZipEntry zipEntry) {
            ZipFile = zipFile;
            ZipEntry = zipEntry;
        }

    }

}