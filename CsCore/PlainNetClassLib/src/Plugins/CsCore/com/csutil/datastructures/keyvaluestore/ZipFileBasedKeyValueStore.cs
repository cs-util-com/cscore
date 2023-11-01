using Zio;
using Zio.FileSystems;

namespace com.csutil.keyvaluestore {

    public class ZipFileBasedKeyValueStore : FileBasedKeyValueStore {

        public static ZipFileBasedKeyValueStore New(FileEntry zipFile, int maxAllowedOpenChanges = 0) {
            var zipFileSystem = zipFile.OpenOrCreateAsZip();
            var zipRootDir = zipFileSystem.GetRootDirectory();
            var store = new ZipFileBasedKeyValueStore(zipRootDir, maxAllowedOpenChanges);
            store.SourceZipFile = zipFile;
            store._zipFileSystem = zipFileSystem;
            return store;
        }

        /// <summary> The parent zip file that is used by the store </summary>
        public FileEntry SourceZipFile { get; private set; }

        private ZipArchiveFileSystem _zipFileSystem;
        private readonly int _maxAllowedOpenChanges;

        private ZipFileBasedKeyValueStore(DirectoryEntry zipRootDir, int maxAllowedOpenChanges) : base(zipRootDir) {
            _maxAllowedOpenChanges = maxAllowedOpenChanges;
        }

        protected override int FlushOpenChangesIfNeeded(int openChanges) {
            if (openChanges > _maxAllowedOpenChanges) {
                CloseAndReopenZip();
                return 0;
            }
            return openChanges;
        }

        private void CloseAndReopenZip() {
            this._zipFileSystem.Dispose();
            this._zipFileSystem = SourceZipFile.OpenOrCreateAsZip();
            this.folderForAllFiles = _zipFileSystem.GetRootDirectory();
        }

        public override void Dispose() {
            _zipFileSystem.Dispose();
            base.Dispose();
        }

    }

}