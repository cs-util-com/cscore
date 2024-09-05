using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace com.csutil.http.apis {

    public static class GitHub {

        /// <summary> Loads the content of a target GitHub repository </summary>
        /// <param name="repo"> e.g. 'cs-util-com/cscore' </param>
        public static Task<List<Content>> LoadRepoContent(string repo, string branch = null, string userAgent = "cSharpApp") {
            var repoContentGetUrl = $"https://api.github.com/repos/{repo}/contents";
            if (!branch.IsNullOrEmpty()) { repoContentGetUrl += $"?ref={branch}"; }
            return new Uri(repoContentGetUrl).SendGET().WithRequestHeaderUserAgent(userAgent).GetResult<List<Content>>();
        }

        public static Task<List<Content>> LoadFolderContent(this Content folder, string userAgent = "cSharpApp") {
            if (!folder.IsFolder()) { throw new ArgumentException($"Not a folder: {folder} but instead of type {folder.type}"); }
            return new Uri(folder.url).SendGET().WithRequestHeaderUserAgent(userAgent).GetResult<List<Content>>();
        }

        public static Task<List<Content>> LoadSubFolder(this IEnumerable<Content> folderCsCoreContent, string folderName, string userAgent = "cSharpApp") {
            return folderCsCoreContent.Single(content => content.name == folderName).LoadFolderContent(userAgent);
        }

        public static Task<Stream> DownloadFile(this Content file) {
            if (!file.IsFile()) { throw new ArgumentException($"Not a file: {file} but instead of type {file.type}"); }
            return new Uri(file.download_url).SendGET().GetResult<Stream>();
        }

        public class Content {
            public string name { get; set; }
            public string path { get; set; }
            public string sha { get; set; }
            public int size { get; set; }
            public string url { get; set; }
            public string html_url { get; set; }
            public string git_url { get; set; }
            public string download_url { get; set; }
            public string type { get; set; }
            public Links _links { get; set; }
            public bool IsFolder() => type == "dir";
            public bool IsFile() => type == "file";

            public class Links {
                public string self { get; set; }
                public string git { get; set; }
                public string html { get; set; }
            }

        }

    }

}