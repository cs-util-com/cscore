using Zio;
using Zio.FileSystems;

namespace com.csutil.keyvaluestore {

    public class ZipFileBasedKeyValueStore : FileBasedKeyValueStore {

        /// <summary> Will use a passed in zip file as the storage for the key value store.
        /// Will automatically persist all changes to the zip file in intervals based on the
        /// specified <see cref="maxAllowedOpenChanges"/> value. </summary>
        /// <param name="zipFile"> The existing or new zip file to use </param>
        /// <param name="maxAllowedOpenChanges"> 0 means any change will be persisted immediately,
        /// the larger the number the faster the store will be able to take new data in with the
        /// risk that the application crashes the last x changes to it would be lost, so 0 is the
        /// only session that guarantees that all changes are persisted to the zip file. </param>
        /// <returns></returns>
        public static ZipFileBasedKeyValueStore New(FileEntry zipFile, int maxAllowedOpenChanges = 0) {
            var zipFileSystem = zipFile.OpenOrCreateAsZip();
            var zipRootDir = zipFileSystem.GetRootDirectory();
            var store = new ZipFileBasedKeyValueStore(zipRootDir, maxAllowedOpenChanges);
            store.SourceZipFile = zipFile;
            store._zipFileSystem = zipFileSystem;
            return store;
        }

        /// <summary> The parent zip file that is used by the store </summary>
        private FileEntry SourceZipFile { get; set; }

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
            lock (folderAccessLock) {
                this._zipFileSystem.Dispose();
                this._zipFileSystem = SourceZipFile.OpenOrCreateAsZip();
                this.folderForAllFiles = _zipFileSystem.GetRootDirectory();
            }
        }

        public override void Dispose() {
            _zipFileSystem.Dispose();
            base.Dispose();
        }

    }

}