namespace ZetaLongPaths.UnitTests
{
    [TestFixture]
    public class FileInfoTest
    {
        [Test]
        public void TestToString()
        {
            var a = new ZlpFileInfo(@"C:\ablage\test.txt");
            var b = new FileInfo(@"C:\ablage\test.txt");

            var x = a.ToString();
            var y = b.ToString();

            Assert.AreEqual(x, y);

            // --

            a = new ZlpFileInfo(@"C:\ablage\");
            b = new FileInfo(@"C:\ablage\");

            x = a.ToString();
            y = b.ToString();

            Assert.AreEqual(x, y);

            // --

            a = new ZlpFileInfo(@"test.txt");
            b = new FileInfo(@"test.txt");

            x = a.ToString();
            y = b.ToString();

            Assert.AreEqual(x, y);

            // --

            a = new ZlpFileInfo(@"c:\ablage\..\ablage\test.txt");
            b = new FileInfo(@"c:\ablage\..\ablage\test.txt");

            x = a.ToString();
            y = b.ToString();

            Assert.AreEqual(x, y);

            // --

            a = new ZlpFileInfo(@"\ablage\test.txt");
            b = new FileInfo(@"\ablage\test.txt");

            x = a.ToString();
            y = b.ToString();

            Assert.AreEqual(x, y);

            // --

            a = new ZlpFileInfo(@"ablage\test.txt");
            b = new FileInfo(@"ablage\test.txt");

            x = a.ToString();
            y = b.ToString();

            Assert.AreEqual(x, y);

            // --

            a = new ZlpFileInfo(@"\\nas001\data\Users\ukeim\Ablage\F~$F_vPrd.xlsm");
            var exists = a.Exists;

            Assert.IsFalse(exists);
        }

        [Test]
        public void TestTilde()
        {
            // https://github.com/UweKeim/ZetaLongPaths/issues/24

            var path1 = ZlpDirectoryInfo.GetTemp().CombineDirectory(Guid.NewGuid().ToString()).CheckCreate();
            var path2 = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
            try
            {
                var p1 = path1.CombineDirectory(@"a~b").CheckCreate();
                var p2 = Directory.CreateDirectory(Path.Combine(path2.FullName, @"a~b")).FullName;

                var f1 = p1.CombineFile("1.txt");
                f1.WriteAllText("1");

                var f2 = Path.Combine(p2, "1.txt");
                File.WriteAllText(f2, "1");

                foreach (var file in p1.GetFiles())
                {
                    Console.WriteLine(file.FullName);
                }

                foreach (var file in Directory.GetFiles(p2))
                {
                    Console.WriteLine(file);
                }
            }
            finally
            {
                path1.SafeDelete();
                path2.Delete(true);
            }
        }
    }
}