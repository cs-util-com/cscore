namespace ZetaLongPaths.UnitTests
{
    using Tools;

    [TestFixture]
    public class DirectoryInfoTest
    {
        [Test]
        public void TestMove()
        {
            var path = ZlpDirectoryInfo.GetTemp().CombineDirectory(Guid.NewGuid().ToString()).CheckCreate();
            try
            {
                var p1 = path.CombineDirectory(@"a").CheckCreate();
                var p2 = path.CombineDirectory(@"b");

                var f1 = p1.CombineFile("1.txt");
                f1.WriteAllText("1");

                Assert.DoesNotThrow(() => p1.MoveTo(p2));
            }
            finally
            {
                path.SafeDelete();
            }
        }

        [Test]
        public void TestMove2()
        {
            var path = ZlpDirectoryInfo.GetTemp().CombineDirectory(Guid.NewGuid().ToString()).CheckCreate();
            try
            {
                var p1 = path.CombineDirectory(@"a").CheckCreate();
                var p2 = path.CombineDirectory(@"b").CheckCreate(); // Das "CheckCreate()" hier ist falsch.

                var f1 = p1.CombineFile("1.txt");
                f1.WriteAllText("1");

                Assert.Throws<Win32Exception>(() => p1.MoveTo(p2));
            }
            finally
            {
                path.SafeDelete();
            }
        }

        [Test]
        public void TestFolders()
        {
            var dirInfo1 = new ZlpDirectoryInfo(@"C:\Foo\Bar");
            Console.WriteLine(dirInfo1.Name); //"Bar"
            var dirInfo2 = new ZlpDirectoryInfo(@"C:\Foo\Bar\");
            Console.WriteLine(dirInfo2.Name); //"", an empty string

            var dirInfo3 = new DirectoryInfo(@"C:\Foo\Bar");
            Console.WriteLine(dirInfo1.Name);
            var dirInfo4 = new DirectoryInfo(@"C:\Foo\Bar\");
            Console.WriteLine(dirInfo2.Name);

            Assert.AreEqual(dirInfo1.Name, dirInfo3.Name);
            Assert.AreEqual(dirInfo2.Name, dirInfo4.Name);
        }

        [Test]
        public void TestCreateWithLimitedPermission()
        {
            // Only in development environment.
            if (Directory.Exists(@"\\nas001\Data\users\ukeim\Ablage\restricted\"))
            {
                if (true)
                {
                    ZlpIOHelper.DeleteDirectoryContents(@"\\nas001\Data\users\ukeim\Ablage\restricted\", true);

                    var dirInfo1 = new ZlpDirectoryInfo(@"\\nas001\Data\users\ukeim\Ablage\restricted\my\folder");

                    // Der Benutzer hat keine Rechte, um "restricted" zu erstellen, nur darin enthaltene.
                    using (new ZlpImpersonator(@"small_user", @"office", @"ThisIsAnUnsecurePassword"))
                    {
                        Assert.DoesNotThrow(delegate { new DirectoryInfo(dirInfo1.FullName).Create(); });
                    }
                }
                if (true)
                {
                    ZlpIOHelper.DeleteDirectoryContents(@"\\nas001\Data\users\ukeim\Ablage\restricted\", true);

                    var dirInfo1 = new ZlpDirectoryInfo(@"\\nas001\Data\users\ukeim\Ablage\restricted\my\folder");

                    // Der Benutzer hat keine Rechte, um "restricted" zu erstellen, nur darin enthaltene.
                    using (new ZlpImpersonator(@"small_user", @"office", @"ThisIsAnUnsecurePassword"))
                    {
                        Assert.DoesNotThrow(delegate { dirInfo1.Create(); });
                    }
                }
            }
        }

        [Test]
        public void TestGetFileSystemInfos()
        {
            var path = ZlpDirectoryInfo.GetTemp().CombineDirectory(Guid.NewGuid().ToString()).CheckCreate();
            try
            {
                var p1 = path.CombineDirectory(@"a").CheckCreate();
                path.CombineDirectory(@"b").CheckCreate();

                var f1 = p1.CombineFile("1.txt");
                f1.WriteAllText("1");

                Assert.IsTrue(path.GetFileSystemInfos().Length == 2);
                Assert.IsTrue(path.GetFileSystemInfos(SearchOption.AllDirectories).Length == 3);
                Assert.IsTrue(
                    path.GetFileSystemInfos(SearchOption.AllDirectories).Where(f => f is ZlpFileInfo).ToList().Count ==
                    1);
                Assert.IsTrue(
                    path.GetFileSystemInfos(SearchOption.AllDirectories)
                        .Where(f => f is ZlpDirectoryInfo)
                        .ToList()
                        .Count == 2);

            }
            finally
            {
                path.SafeDelete();
            }
        }

        [Test]
        public void TestGeneral()
        {
            // Ordner mit Punkt am Ende.
            var dir = $@"C:\Ablage\{Guid.NewGuid():N}.";
            Assert.IsFalse(new ZlpDirectoryInfo(dir).Exists);
            new ZlpDirectoryInfo(dir).CheckCreate();
            Assert.IsTrue(new ZlpDirectoryInfo(dir).Exists);
            new ZlpDirectoryInfo(dir).Delete(true);
            Assert.IsFalse(new ZlpDirectoryInfo(dir).Exists);


            //Assert.IsTrue(new ZlpDirectoryInfo(Path.GetTempPath()).CreationTime>DateTime.MinValue);
            //Assert.IsTrue(new ZlpDirectoryInfo(Path.GetTempPath()).Exists);
            //Assert.IsFalse(new ZlpDirectoryInfo(@"C:\Ablage\doesnotexistjdlkfjsdlkfj").Exists);
            //Assert.IsTrue(new ZlpDirectoryInfo(Path.GetTempPath()).Exists);
            //Assert.IsFalse(new ZlpDirectoryInfo(@"C:\Ablage\doesnotexistjdlkfjsdlkfj2").Exists);
            //Assert.IsFalse(new ZlpDirectoryInfo(@"\\zetac11\C$\Ablage").Exists);
            //Assert.IsFalse(new ZlpDirectoryInfo(@"\\zetac11\C$\Ablage\doesnotexistjdlkfjsdlkfj2").Exists);

            const string s1 =
                @"C:\Users\Chris\Documents\Development\ADC\InterStore.NET\Visual Studio 2008\6.4.2\Zeta Resource Editor";
            const string s2 =
                @"C:\Users\Chris\Documents\Development\ADC\InterStore.NET\Visual Studio 2008\6.4.2\Web\central\Controls\App_LocalResources\ItemSearch";

            var s3 = ZlpPathHelper.GetRelativePath(s1, s2);
            Assert.AreEqual(s3, @"..\Web\central\Controls\App_LocalResources\ItemSearch");

            var ext = ZlpPathHelper.GetExtension(s3);
            Assert.IsEmpty(ext ?? string.Empty);

            ext = ZlpPathHelper.GetExtension(@"C:\Ablage\Uwe.txt");
            Assert.AreEqual(ext, @".txt");

            const string path = @"C:\Ablage\Test";
            Assert.AreEqual(
                new DirectoryInfo(path).Name,
                new ZlpDirectoryInfo(path).Name);

            Assert.AreEqual(
                new DirectoryInfo(path).FullName,
                new ZlpDirectoryInfo(path).FullName);

            const string filePath = @"C:\Ablage\Test\file.txt";
            var fn1 = new FileInfo(filePath).Directory?.FullName;
            var fn2 = new ZlpFileInfo(filePath).Directory.FullName;

            var fn1A = new FileInfo(filePath).DirectoryName;
            var fn2A = new ZlpFileInfo(filePath).DirectoryName;

            Assert.AreEqual(fn1, fn2);
            Assert.AreEqual(fn1A, fn2A);

            var fn = new ZlpDirectoryInfo(@"\\zetac11\C$\Ablage\doesnotexistjdlkfjsdlkfj2").Parent.FullName;

            Assert.AreEqual(fn, @"\\zetac11\C$\Ablage");

            fn = new ZlpDirectoryInfo(@"\\zetac11\C$\Ablage\doesnotexistjdlkfjsdlkfj2\").Parent.FullName;

            Assert.AreEqual(fn, @"\\zetac11\C$\Ablage");
        }

        [Test]
        public void TestToString()
        {
            var a = new ZlpDirectoryInfo(@"C:\ablage\test.txt");
            var b = new DirectoryInfo(@"C:\ablage\test.txt");

            var x = a.ToString();
            var y = b.ToString();

            Assert.AreEqual(x, y);

            // --

            a = new ZlpDirectoryInfo(@"C:\ablage\");
            b = new DirectoryInfo(@"C:\ablage\");

            x = a.ToString();
            y = b.ToString();

            Assert.AreEqual(x, y);

            // --

            a = new ZlpDirectoryInfo(@"test.txt");
            b = new DirectoryInfo(@"test.txt");

            x = a.ToString();
            y = b.ToString();

            Assert.AreEqual(x, y);

            // --

            a = new ZlpDirectoryInfo(@"c:\ablage\..\ablage\test.txt");
            b = new DirectoryInfo(@"c:\ablage\..\ablage\test.txt");

            x = a.ToString();
            y = b.ToString();

            Assert.AreEqual(x, y);

            // --

            a = new ZlpDirectoryInfo(@"\ablage\test.txt");
            b = new DirectoryInfo(@"\ablage\test.txt");

            x = a.ToString();
            y = b.ToString();

            Assert.AreEqual(x, y);

            // --

            a = new ZlpDirectoryInfo(@"ablage\test.txt");
            b = new DirectoryInfo(@"ablage\test.txt");

            x = a.ToString();
            y = b.ToString();

            Assert.AreEqual(x, y);
        }
    }
}