using System;
using System.IO;
using System.Linq;

namespace com.csutil {

    public static class FileExtensions {

        public static DirectoryInfo GetChildDir(this DirectoryInfo self, string childFolder, bool assertThatChildMustExist = false) {
            var c = new DirectoryInfo(self.FullPath() + childFolder);
            if (assertThatChildMustExist) {
                AssertV2.IsTrue(c.IsNotNullAndExists(), "childFolder '" + childFolder + "' doesnt exist! Path=" + c.FullPath());
            }
            return c;
        }

        public static FileInfo GetChild(this DirectoryInfo self, string childFile, bool assertThatChildMustExist = false) {
            var c = new FileInfo(self.FullPath() + childFile);
            if (assertThatChildMustExist) {
                AssertV2.IsTrue(c.IsNotNullAndExists(), "childFile '" + childFile + "' doesnt exist! Path=" + c.FullPath());
            }
            return c;
        }

        public static DirectoryInfo CreateV2(this DirectoryInfo self) {
            if (!self.ExistsV2()) { self.Create(); self.Refresh(); }
            return self;
        }

        /// <summary> 
        /// After renaming a Directory its .Name property does not return the correct folder name anymore! 
        /// Thats why NameV2 has to be used instead, which is more expensive but always updated correctly
        /// </summary>
        public static string NameV2(this DirectoryInfo self) {
            var p = self.FullName;
            if (p.EndsWith(Path.DirectorySeparatorChar)) {
                p = p.Substring("" + Path.DirectorySeparatorChar, false);
            }
            return Path.GetFileName(p); // GetFileName works for folders if the path doesnt end in /
        }

        public static string FullPath(this DirectoryInfo self) {
            var p = self.FullName;
            if (!p.EndsWith(Path.DirectorySeparatorChar)) { return p + Path.DirectorySeparatorChar; }
            return p;
        }

        public static string FullPath(this FileInfo self) { return self.FullName; }

        public static bool IsNotNullAndExists(this FileSystemInfo self) {
            if (self == null) { return false; } else { return self.ExistsV2(); }
        }

        public static bool ExistsV2(this FileSystemInfo self) {
            self.Refresh();
            return self.Exists;
        }

        public static DirectoryInfo ParentDir(this FileInfo self) {
            return self.Directory;
        }

        public static FileInfo SetExtension(this FileInfo self, string newExtension) {
            return new FileInfo(Path.ChangeExtension(self.FullPath(), newExtension));
        }

        public static bool DeleteV2(this FileSystemInfo self) {
            return DeleteV2(self, () => { self.Delete(); return true; });
        }

        public static bool DeleteV2(this DirectoryInfo self, bool deleteAlsoIfNotEmpty = true) {
            return DeleteV2(self, () => {
                if (deleteAlsoIfNotEmpty) { // Recursively delete all children first:
                    if (!self.IsEmtpy()) {
                        foreach (var subDir in self.GetDirectories()) { subDir.DeleteV2(deleteAlsoIfNotEmpty); }
                        foreach (var file in self.GetFiles()) { file.DeleteV2(); }
                    }
                }
                self.Refresh();
                if (!self.IsEmtpy()) { throw new IOException("Cant delete non-emtpy dir: " + self.FullPath()); }
                try { self.Delete(); return true; } catch (Exception e) { Log.e(e); }
                return false;
            });
        }

        public static bool IsEmtpy(this DirectoryInfo self) { // TODO use old method to avoid exceptions?
            try { return !self.EnumerateFileSystemInfos().Any(); } catch (System.Exception) { return true; }
        }

        private static bool DeleteV2(FileSystemInfo self, Func<bool> deleteAction) {
            if (self != null && self.ExistsV2()) {
                var res = deleteAction();
                self.Refresh();
                AssertV2.IsFalse(!res || self.ExistsV2(), "Still exists: " + self.FullName);
                return res;
            }
            return false;
        }

        public static bool Rename(this FileInfo self, string newName) {
            self.MoveTo(self.ParentDir().GetChild(newName).FullPath());
            return self.ExistsV2();
        }

        public static bool MoveToV2(this FileInfo self, DirectoryInfo target) {
            self.MoveTo(target.FullPath() + self.Name);
            target.Refresh();
            return self.ExistsV2();
        }

        public static bool OpenInExternalApp(this FileSystemInfo self) {
            try {
                System.Diagnostics.Process.Start(@self.FullName);
                return true;
            } catch (System.Exception e) { Log.e(e); }
            return false;
        }

        public static bool MoveToV2(this DirectoryInfo source, DirectoryInfo target) {
            AssertNotIdentical(source, target);
            var tempCopyId = "" + Guid.NewGuid();
            var originalPath = source.FullPath();
            if (EnvironmentV2.isWebGL) {
                // In WebGL .MoveTo does not work correctly so copy+delete is tried instead:
                var tempDir = EnvironmentV2.instance.GetOrAddTempFolder("TmpCopies").GetChildDir(tempCopyId);
                source.CopyTo(new DirectoryInfo(tempDir.FullName));
            }
            source.MoveTo(target.FullPath());
            source.Refresh();
            if (EnvironmentV2.isWebGL) {
                if (!target.ExistsV2()) {
                    EmulateMoveViaCopyDelete(originalPath, tempCopyId, target);
                } else {
                    Log.e("WebGL TempCopy solution was not needed!");
                }
            }
            return target.ExistsV2();
        }

        /// <summary> Needed in WebGL because .MoveTo does not correctly move the files to
        /// the new target directory but instead only removes the original dir </summary>
        private static void EmulateMoveViaCopyDelete(string source, string tempDirPath, DirectoryInfo target) {
            var tempFolderPath = EnvironmentV2.instance.GetOrAddTempFolder("TmpCopies").GetChildDir(tempDirPath).FullName;
            var tempDir = new DirectoryInfo(tempFolderPath);
            if (tempDir.IsEmtpy()) {
                target.CreateV2();
                CleanupAfterEmulatedMove(source, tempDir);
            } else if (tempDir.CopyTo(target)) {
                CleanupAfterEmulatedMove(source, tempDir);
            } else {
                Log.e("Could not move tempDir=" + tempDir + " into target=" + target);
            }
        }

        private static void CleanupAfterEmulatedMove(string source, DirectoryInfo tempDir) {
            tempDir.DeleteV2();
            var originalDir = new DirectoryInfo(source);
            try { if (originalDir.ExistsV2()) { originalDir.DeleteV2(); } } catch (Exception e) { Log.e("Cleanup err of original dir: " + originalDir, e); }
        }

        private static void AssertNotIdentical(DirectoryInfo source, DirectoryInfo target) {
            if (Equals(source.FullPath(), target.FullPath())) {
                throw new OperationCanceledException("Identical source & target: " + source);
            }
        }

        public static bool Rename(this DirectoryInfo self, string newName) {
            var target = self.Parent.GetChildDir(newName);
            if (target.ExistsV2()) { throw new IOException("Cant rename, already exists: " + target); }
            return self.MoveToV2(target);
        }

        public static bool CopyTo(this DirectoryInfo source, DirectoryInfo target, bool replaceExisting = false) {
            AssertNotIdentical(source, target);
            if (!replaceExisting && target.IsNotNullAndExists()) {
                throw new ArgumentException("Cant copy to existing folder " + target);
            }
            foreach (var subDir in source.EnumerateDirectories()) {
                CopyTo(subDir, target.GetChildDir(subDir.NameV2()), replaceExisting);
            }
            target.CreateV2();
            foreach (var file in source.EnumerateFiles()) {
                var createdFile = file.CopyTo(target.GetChild(file.Name).FullPath(), replaceExisting);
                AssertV2.IsTrue(createdFile.ExistsV2(), "!createdFile.Exists: " + createdFile);
            }
            return target.ExistsV2();
        }

        public static long GetFileSize(this FileInfo self) { return self.Length; }

        public static string GetFileSizeString(this FileInfo self) {
            return ByteSizeToString.ByteSizeToReadableString(self.GetFileSize());
        }

        public static string GetNameWithoutExtension(this FileInfo self) {
            return Path.GetFileNameWithoutExtension(self.Name);
        }

    }

}