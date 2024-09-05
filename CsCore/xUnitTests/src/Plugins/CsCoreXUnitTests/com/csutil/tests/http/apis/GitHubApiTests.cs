using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using com.csutil.http.apis;
using Xunit;

namespace com.csutil.integrationTests.http {

    public class GitHubApiTests {

        public GitHubApiTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public async Task ExampleUsage1() {

            List<GitHub.Content> contentInRootLevel = await GitHub.LoadRepoContent("cs-util-com/cscore");
            Assert.NotEmpty(contentInRootLevel.Where(content => content.IsFile())); // There are files on the root level

            IEnumerable<GitHub.Content> rootFolders = contentInRootLevel.Where(content => content.IsFolder());
            GitHub.Content folderCsCore = rootFolders.Single(content => content.name == "CsCore");
            Log.d("Will not look into the CsCore folder: " + folderCsCore.html_url);
            List<GitHub.Content> folderCsCoreContent = await folderCsCore.LoadFolderContent();

            // There is also a simpler way to navigate into a folder: 
            List<GitHub.Content> folderOfPlainNetCode = await folderCsCoreContent.LoadSubFolder("PlainNetClassLib");

            GitHub.Content csCoreSolutionFile = folderOfPlainNetCode.Single(content => content.name == "PlainNetClassLib.csproj");
            Log.d("Will not download file: " + csCoreSolutionFile.download_url);
            Stream slnFileDownload = await csCoreSolutionFile.DownloadFile();
            using var reader = new StreamReader(slnFileDownload);
            string slnFileContentAsString = reader.ReadToEnd();
            Log.d($"Content of file {csCoreSolutionFile.html_url}:\n {slnFileContentAsString}");

        }

    }

}