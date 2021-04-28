using System;
using System.Diagnostics;
using Zio;

namespace com.csutil.system {

    /// <summary> Allows the creation of symlinks (folders that actually point to a different folder on disk.
    /// See also https://github.com/karl-/unity-symlink-utility/blob/master/SymlinkUtility.cs
    /// </summary>
    public class SymLinker {

        /// <summary> See https://superuser.com/a/1291446 on difference between Junctions and normal SymLinks </summary>
        public enum SymLinkMode { Junction, AbsoluteSymLink }

        public static void CreateSymlink(DirectoryEntry source, DirectoryEntry target, SymLinkMode mode = SymLinkMode.Junction) {
            source.ThrowErrorIfNull("source");
            target.ThrowErrorIfNull("target");
            if (target.Exists) {
                if (IsSymlink(target)) {
                    if (GetOriginalPathForSymlink(target) == source) { return; }
                    throw new ArgumentException($"target folder {target} is already symlink but does not point to source {source}");
                }
                throw new ArgumentException($"target folder {target} already exists");
            }
            AssertTargetNotSubDirOfSource(target, source);

            string sourcePath = source.GetFullFileSystemPath();
            string targetPath = target.GetFullFileSystemPath();
            if (EnvironmentV2.instance.IsWindows()) {
                string linkOption = mode == SymLinkMode.Junction ? "/J" : "/D";
                string command = string.Format("mklink {0} \"{1}\" \"{2}\"", linkOption, targetPath, sourcePath);
                bool needAdminRights = mode != SymLinkMode.Junction; // Symlinks require admin privilege, junctions do not.
                ExecuteWindowsCmdCommand(command, needAdminRights);
            } else { // MacOSX and Linux
                // For some reason, OSX doesn't want to create a symlink with quotes around the paths, so escape the spaces instead.
                sourcePath = sourcePath.Replace(" ", "\\ ");
                targetPath = targetPath.Replace(" ", "\\ ");
                string command = string.Format("ln -s {0} {1}", sourcePath, targetPath);
                ExecuteBashCommand(command);
            }
        }

        private static void ExecuteWindowsCmdCommand(string command, bool runAsAdmin = false) {
            var startInfo = new ProcessStartInfo {
                FileName = "CMD.exe",
                Arguments = "/C " + command,
                UseShellExecute = runAsAdmin,
                RedirectStandardError = !runAsAdmin,
                CreateNoWindow = true,
            };
            // Run process in admin mode https://stackoverflow.com/questions/2532769/how-to-start-a-process-as-administrator-mode-in-c-sharp
            if (runAsAdmin) { startInfo.Verb = "runas"; }
            ExecuteProcess(startInfo);
        }

        private static void ExecuteBashCommand(string command) {
            command = command.Replace("\"", "\"\"");
            ExecuteProcess(new ProcessStartInfo {
                FileName = "/bin/bash",
                Arguments = "-c \"" + command + "\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            });
        }

        private static void ExecuteProcess(ProcessStartInfo processStartInfos) {
            using (var proc = new Process() { StartInfo = processStartInfos }) {
                proc.Start();
                proc.WaitForExit();
                if (!proc.StandardError.EndOfStream) {
                    throw new InvalidOperationException(proc.StandardError.ReadToEnd());
                }
            }
        }

        private static void AssertTargetNotSubDirOfSource(DirectoryEntry target, DirectoryEntry source) {
            if (target == source) { throw new InvalidOperationException($"target {target} must not be subdir of symlink source folder"); }
            if (source.Path == UPath.Root) { return; }
            AssertTargetNotSubDirOfSource(target, source.Parent);
        }

        public static DirectoryEntry GetOriginalPathForSymlink(DirectoryEntry target) {
            if (!IsSymlink(target)) { throw new ArgumentException($"target {target} is not a valid symlink folder"); }
            throw new NotImplementedException();
        }

        public static bool IsSymlink(DirectoryEntry target) {
            target.ThrowErrorIfNull("target");
            throw new NotImplementedException();
        }

    }

}