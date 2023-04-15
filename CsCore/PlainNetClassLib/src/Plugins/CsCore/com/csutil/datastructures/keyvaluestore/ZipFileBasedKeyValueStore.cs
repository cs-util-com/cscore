using Zio;
using Zio.FileSystems;

namespace com.csutil.keyvaluestore {

    public class ZipFileBasedKeyValueStore : FileBasedKeyValueStore {

        public static ZipFileBasedKeyValueStore New(FileEntry zipFile) {
            var zipFileSystem = zipFile.OpenAsZip();
            var zipRootDir = zipFileSystem.GetRootDirectory();
            var store = new ZipFileBasedKeyValueStore(zipRootDir);
            store.SourceZipFile = zipFile;
            store.zipFileSystem = zipFileSystem;
            return store;
        }

        /// <summary> The parent zip file that is used by the store </summary>
        public FileEntry SourceZipFile { get; private set; }

        private ZipArchiveFileSystem zipFileSystem;

        private ZipFileBasedKeyValueStore(DirectoryEntry zipRootDir) : base(zipRootDir) { }

        public override void Dispose() {
            zipFileSystem.Dispose();
            base.Dispose();
        }

    }

}