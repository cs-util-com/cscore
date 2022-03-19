namespace ZetaLongPaths.UnitTests
{
    [TestFixture]
    public class PathHelperTest
    {
        [Test]
        public void TestGeneral5()
        {
            var s1 = @"a";
            var s2 = @"b";

            Assert.AreEqual(s1.NullOrEmptyOther(s2), s1);

            s1 = null;
            s2 = @"b";

            Assert.AreEqual(s1.NullOrEmptyOther(s2), s2);

            s1 = null;
            s2 = null;

            Assert.AreEqual(s1.NullOrEmptyOther(s2), s2);

            s1 = @"a";
            s2 = null;

            Assert.AreEqual(s1.NullOrEmptyOther(s2), s1);

            s1 = string.Empty;
            s2 = @"b";

            Assert.AreEqual(s1.NullOrEmptyOther(s2), s2);

            s1 = string.Empty;
            s2 = string.Empty;

            Assert.AreEqual(s1.NullOrEmptyOther(s2), s2);

            s1 = @"a";
            s2 = string.Empty;

            Assert.AreEqual(s1.NullOrEmptyOther(s2), s1);

            s1 = null;
            s2 = string.Empty;

            Assert.AreEqual(s1.NullOrEmptyOther(s2), s2);
        }

        [Test]
        public void TestGeneral1()
        {
            // --
            // Conversion between short and long paths.

            var lp1 = Assembly.GetEntryAssembly()?.Location;
            if (lp1 == null) return;

            var sp1 = ZlpIOHelper.ForceRemoveLongPathPrefix(ZlpPathHelper.GetShortPath(lp1));
            var lp2 = ZlpIOHelper.ForceRemoveLongPathPrefix(ZlpPathHelper.GetLongPath(sp1));
            var sp2 = ZlpIOHelper.ForceRemoveLongPathPrefix(ZlpPathHelper.GetShortPath(lp2));

            Assert.AreEqual(lp1.ToLower(), lp2.ToLower());
            Assert.AreEqual(sp1.ToLower(), sp2.ToLower());

            // --
            // Getting file sizes for short and long paths.

            var lengthA1 = new ZlpFileInfo(sp1).Length;
            var lengthA2 = new ZlpFileInfo(sp2).Length;

            var lengthB1 = new ZlpFileInfo(lp1).Length;
            var lengthB2 = new ZlpFileInfo(lp2).Length;

            Assert.AreEqual(lengthA1, lengthA2);
            Assert.AreEqual(lengthA1, lengthB1);
            Assert.AreEqual(lengthA1, lengthB2);
        }

        [Test]
        public void TestGeneral2()
        {
            var s1 =
                @"C:\Users\ukeim\Documents\Visual Studio 2008\Projects\Zeta Producer 9\Zeta Producer Main\Deploy\Origin\Enterprise\C-Allgaier\Windows\Packaging\Stationary\DEU\FirstStart\StandardProject";
            var s2 = ZlpPathHelper.GetFullPath(s1);

            Assert.AreEqual(
                @"C:\Users\ukeim\Documents\Visual Studio 2008\Projects\Zeta Producer 9\Zeta Producer Main\Deploy\Origin\Enterprise\C-Allgaier\Windows\Packaging\Stationary\DEU\FirstStart\StandardProject",
                s2);

            // --

            s1 = @"c:\ablage\..\windows\notepad.exe";
            s2 = ZlpPathHelper.GetFullPath(s1);

            Assert.AreEqual(@"c:\windows\notepad.exe", s2);

            //--

            s1 = @"lalala-123";
            s2 = ZlpPathHelper.GetFileNameWithoutExtension(s1);

            Assert.AreEqual(@"lalala-123", s2);

            //--

            s1 = @"lalala-123.txt";
            s2 = ZlpPathHelper.GetFileNameWithoutExtension(s1);

            Assert.AreEqual(@"lalala-123", s2);

            //--

            s1 = @"C:\Ablage\lalala-123.txt";
            s2 = ZlpPathHelper.GetFileNameWithoutExtension(s1);

            Assert.AreEqual(@"lalala-123", s2);

            //--

            s1 = @"\\nas001\data\folder\lalala-123.txt";
            s2 = ZlpPathHelper.GetFileNameWithoutExtension(s1);

            Assert.AreEqual(@"lalala-123", s2);

            //--

            s1 = @"c:\ablage\..\windows\notepad.exe";
            s2 = ZlpPathHelper.GetFileNameWithoutExtension(s1);

            Assert.AreEqual(@"notepad", s2);

            //--

            s1 = @"c:\ablage\..\windows\notepad.exe";
            s2 = ZlpPathHelper.GetExtension(s1);

            Assert.AreEqual(@".exe", s2);

            //--

            //--

            s1 = @"c:\ablage\..\windows\notepad.file.exe";
            s2 = ZlpPathHelper.GetExtension(s1);

            Assert.AreEqual(@".exe", s2);

            //--

            s1 = @"c:\ablage\..\windows\notepad.exe";
            s2 = ZlpPathHelper.ChangeExtension(s1, @".com");

            Assert.AreEqual(@"c:\ablage\..\windows\notepad.com", s2);

            // --

            s1 = @"file.ext";
            s2 = @"c:\ablage\path1\path2";
            var s3 = @"c:\ablage\path1\path2\file.ext";
            var s4 = ZlpPathHelper.GetAbsolutePath(s1, s2);

            Assert.AreEqual(s3, s4);

            var s5 = s1.MakeAbsoluteTo(new ZlpDirectoryInfo(s2));

            Assert.AreEqual(s3, s5);

            // --

            s1 = @"c:\folder1\folder2\folder4\";
            s2 = @"c:\folder1\folder2\folder3\file1.txt";
            s3 = ZlpPathHelper.GetRelativePath(s1, s2);

            s4 = @"..\folder3\file1.txt";

            Assert.AreEqual(s3, s4);
        }

        [Test]
        public void TestCompareWithFrameworkFunctions()
        {
            // --

            var s1 = ZlpPathHelper.GetFileNameFromFilePath(@"/suchen.html");
            var s2 = Path.GetFileName(@"/suchen.html");

            Assert.AreEqual(s1, s2);

            // --

            s1 = ZlpPathHelper.GetDirectoryPathNameFromFilePath(@"sitemap.xml");
            s2 = Path.GetDirectoryName(@"sitemap.xml");

            Assert.AreEqual(s1, s2);

            //s1 = ZlpPathHelper.GetDirectoryPathNameFromFilePath(@"");
            //s2 = Path.GetDirectoryName(@"");

            //Assert.AreEqual(s1, s2);

            s1 = ZlpPathHelper.GetDirectoryPathNameFromFilePath(@"c:\ablage\sitemap.xml");
            s2 = Path.GetDirectoryName(@"c:\ablage\sitemap.xml");

            Assert.AreEqual(s1, s2);

            s1 = ZlpPathHelper.GetDirectoryPathNameFromFilePath(@"c:\ablage\");
            s2 = Path.GetDirectoryName(@"c:\ablage\");

            Assert.AreEqual(s1, s2);

            s1 = ZlpPathHelper.GetDirectoryPathNameFromFilePath(@"c:\ablage");
            s2 = Path.GetDirectoryName(@"c:\ablage");

            Assert.AreEqual(s1, s2);

            s1 = ZlpPathHelper.GetDirectoryPathNameFromFilePath(@"c:/ablage/sitemap.xml");
            s2 = Path.GetDirectoryName(@"c:/ablage/sitemap.xml");

            Assert.AreEqual(s1, s2);

            // --

            s1 = @"c:\folder1\folder2\folder3\file1.txt";

            var s3 = ZlpPathHelper.GetFileNameFromFilePath(s1);
            var s4 = Path.GetFileName(s1);

            Assert.AreEqual(s3, s4);

            // --

            s1 = @"c:\folder1\folder2\folder3\file1";

            s3 = ZlpPathHelper.GetFileNameFromFilePath(s1);
            s4 = Path.GetFileName(s1);

            Assert.AreEqual(s3, s4);

            // --

            s1 = @"c:\folder1\folder2\folder3\file1.";

            s3 = ZlpPathHelper.GetFileNameFromFilePath(s1);
            s4 = Path.GetFileName(s1);

            Assert.AreEqual(s3, s4);

            // --

            s1 = @"file1.txt";

            s3 = ZlpPathHelper.GetFileNameFromFilePath(s1);
            s4 = Path.GetFileName(s1);

            Assert.AreEqual(s3, s4);

            // --

            s1 = @"file1";

            s3 = ZlpPathHelper.GetFileNameFromFilePath(s1);
            s4 = Path.GetFileName(s1);

            Assert.AreEqual(s3, s4);

            // --

            s1 = @"file1.";

            s3 = ZlpPathHelper.GetFileNameFromFilePath(s1);
            s4 = Path.GetFileName(s1);

            Assert.AreEqual(s3, s4);

            // --

            s1 = @"c:\folder1\folder2\folder3\file1.txt";

            s3 = ZlpPathHelper.GetFileNameWithoutExtension(s1);
            s4 = Path.GetFileNameWithoutExtension(s1);

            Assert.AreEqual(s3, s4);

            // --

            s1 = @"c:\folder1\folder2\folder3\file1";

            s3 = ZlpPathHelper.GetFileNameWithoutExtension(s1);
            s4 = Path.GetFileNameWithoutExtension(s1);

            Assert.AreEqual(s3, s4);

            // --

            s1 = @"c:\folder1\folder2\folder3\file1.";

            s3 = ZlpPathHelper.GetFileNameWithoutExtension(s1);
            s4 = Path.GetFileNameWithoutExtension(s1);

            Assert.AreEqual(s3, s4);

            // --

            s1 = @"file1.txt";

            s3 = ZlpPathHelper.GetFileNameWithoutExtension(s1);
            s4 = Path.GetFileNameWithoutExtension(s1);

            Assert.AreEqual(s3, s4);

            // --

            s1 = @"file1";

            s3 = ZlpPathHelper.GetFileNameWithoutExtension(s1);
            s4 = Path.GetFileNameWithoutExtension(s1);

            Assert.AreEqual(s3, s4);

            // --

            s1 = @"file1.";

            s3 = ZlpPathHelper.GetFileNameWithoutExtension(s1);
            s4 = Path.GetFileNameWithoutExtension(s1);

            Assert.AreEqual(s3, s4);
        }

        [Test]
        public void TestEvenMoreFunctions()
        {
            var r = ZlpPathHelper.GetFileNameWithoutExtension(@"\\?\C:\Simulazioni\Albero\scratch_file.txt");
            Assert.AreEqual(r, @"scratch_file");

            r = ZlpPathHelper.GetFileNameWithoutExtension(@"\\?\C:\Simulazioni\Albero\scratch_file.");
            Assert.AreEqual(r, @"scratch_file");

            r = ZlpPathHelper.GetFileNameWithoutExtension(@"\\?\C:\Simulazioni\Albero\scratch_file");
            Assert.AreEqual(r, @"scratch_file");
        }

        [Test]
        public void TestGeneral3()
        {
            var s1 = @"c:\folder1\folder2\folder3\file1.txt";
            var s2 = ZlpPathHelper.ChangeFileNameWithoutExtension(s1, @"file2");

            Assert.AreEqual(s2, @"c:\folder1\folder2\folder3\file2.txt");

            s1 = @"c:\folder1\folder2\folder3\file1.txt";
            s2 = ZlpPathHelper.ChangeFileName(s1, @"file2.md");

            Assert.AreEqual(s2, @"c:\folder1\folder2\folder3\file2.md");
        }

        [Test]
        public void TestGeneral4()
        {
            var longRelativeFilePath = @"folder1\folder2\folder3\folder4\folder5\folder6\folder7\folder8\folder9\folder10\folder11\folder12\folder13\folder14\folder15\file.txt";
            var longBasePath = @"c:\folder1\folder2\folder3\folder4\folder5\folder6\folder7\folder8\folder9\folder10\folder11\folder12\folder13\folder14\folder15\";

            var absolutePath = ZlpPathHelper.GetAbsolutePath(longRelativeFilePath, longBasePath);
            Assert.IsTrue(absolutePath.StartsWith(@"\\?\"));

            var isAbsolutePath = ZlpPathHelper.IsAbsolutePath(absolutePath);
            Assert.IsTrue(isAbsolutePath);

            // --

            absolutePath = @"C:\Ablage\123.txt";
            isAbsolutePath = ZlpPathHelper.IsAbsolutePath(absolutePath);
            Assert.IsTrue(isAbsolutePath);

            // --

            var relativePath = @"Ablage\123.txt";
            isAbsolutePath = ZlpPathHelper.IsAbsolutePath(relativePath);
            Assert.IsFalse(isAbsolutePath);

            // --

            relativePath = @"123.txt";
            isAbsolutePath = ZlpPathHelper.IsAbsolutePath(relativePath);
            Assert.IsFalse(isAbsolutePath);
        }
    }
}