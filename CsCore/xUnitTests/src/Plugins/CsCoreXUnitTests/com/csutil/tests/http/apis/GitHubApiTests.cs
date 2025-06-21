using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using com.csutil.http.apis;
using com.csutil.xml;
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
            var slnFileDownload = await csCoreSolutionFile.DownloadFile().CopyToSeekableStreamIfNeeded(true);

            LookIntoCsProjectFileViaXmlParsing(slnFileDownload);

            // Print out file content as string: 
            using var reader = new StreamReader(slnFileDownload);
            string slnFileContentAsString = reader.ReadToEnd();
            Log.d($"Content of file {csCoreSolutionFile.html_url}:\n {slnFileContentAsString}");

        }

        // Since PlainNetClassLib.csproj is an XML file, it could also be parsed and compared to the local PlainNetClassLib.csproj file:
        private static void LookIntoCsProjectFileViaXmlParsing(Stream slnFileDownload) {
            CsProjectFile downloadedCsProjectFile = slnFileDownload.ParseAsXmlInto<CsProjectFile>();

            DirectoryInfo currentDir = new DirectoryInfo(Directory.GetCurrentDirectory());
            var localCsProjectFile = currentDir.FindParentWithName("CsCore").GetChildDir("PlainNetClassLib").GetChild("PlainNetClassLib.csproj");
            Assert.True(localCsProjectFile.Exists, "Expected the local csProj file to exist");
            CsProjectFile localCsProjectFileContent = localCsProjectFile.OpenRead().ParseAsXmlInto<CsProjectFile>();

            // Get the project version of the current open csharp project to compare it to the one from the csProj file:
            var comparison = localCsProjectFileContent.Properties.Version.CompareTo(downloadedCsProjectFile.Properties.Version);
            if (comparison < 0) {
                throw Log.e("The downloaded csProj file has a newer version than the local one, update to the latest CsCore version?");
            }
            if (comparison > 0) {
                throw Log.e("The downloaded csProj file has an older version than the local one, are you working on a new version?");
            }
            Log.d("downloaded PlainNetClassLib.csproj => Properties.Version=" + downloadedCsProjectFile.Properties.Version);
            Log.d("local PlainNetClassLib.csproj => Properties.Version=" + localCsProjectFileContent.Properties.Version);
        }

        [XmlRoot("Project")]
        public class CsProjectFile { // As an example a few of the fields of the XML

            [XmlElement("PropertyGroup")]
            public PropertyGroup Properties;

            public class PropertyGroup {
                [XmlElement("Version")]
                public string VersionRaw;
                public Version Version => new Version(VersionRaw);
            }

        }

    }

}