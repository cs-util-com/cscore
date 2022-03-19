namespace TestConsole
{
    internal static class Program
    {
        private static void Main()
        {
            doTest01();
            return;

            try
            {
                const string name = @"D:\SomeStuff\Name Space\More.Stuff\Test";

                var dirInfo5 = new ZlpDirectoryInfo(name);
                Console.WriteLine($@"'{dirInfo5.Name}'.");

                var dirInfo6 = new DirectoryInfo(name);
                Console.WriteLine($@"'{dirInfo6.Name}'.");

                if (dirInfo5.Name != dirInfo6.Name) throw new ZlpException(@"5-6");

                // --

                var dirInfo1 = new ZlpDirectoryInfo(@"C:\Foo\Bar");
                Console.WriteLine(dirInfo1.Name); //"Bar"
                var dirInfo2 = new ZlpDirectoryInfo(@"C:\Foo\Bar\");
                Console.WriteLine(dirInfo2.Name); //"", an empty string

                var dirInfo3 = new DirectoryInfo(@"C:\Foo\Bar");
                Console.WriteLine(dirInfo1.Name);
                var dirInfo4 = new DirectoryInfo(@"C:\Foo\Bar\");
                Console.WriteLine(dirInfo2.Name);

                if (dirInfo1.Name != dirInfo3.Name) throw new ZlpException(@"1-3");
                if (dirInfo2.Name != dirInfo4.Name) throw new ZlpException(@"2-4");

                // --

                var f1 = new ZlpFileInfo(
                    @"C:\Ablage\test-only\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Lalala.txt");
                f1.Directory.Create();
                f1.WriteAllText("lalala.");
                Console.WriteLine("f1.Length: " + f1.Length);

                var f2 = new ZlpFileInfo(
                    @"D:\Ablage\test-only\Ablage2\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Lalala.txt");
                f2.Directory.Create();

                f1.MoveTo(f2, true);
                Console.WriteLine("f2.Length: " + f2.Length);

                new ZlpDirectoryInfo(@"C:\Ablage\test-only\").Delete(true);
                new ZlpDirectoryInfo(@"D:\Ablage\test-only\").Delete(true);
                //f1.MoveToRecycleBin();


                var f = new ZlpFileInfo(@"C:\Ablage\Lalala.txt");
                f.WriteAllText("lalala.");
                f.MoveToRecycleBin();

                var d = new ZlpDirectoryInfo(@"C:\Ablage\LalalaOrdner");
                d.Create();
                d.MoveToRecycleBin();
            }
            catch (Exception x)
            {
                Console.WriteLine(x.ToString());
                throw;
            }
        }

        private static void doTest01()
        {
            try
            {
                Console.WriteLine();
                Console.WriteLine();

                const string longFileOnC = @"C:\Ablage\test-only\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\LalalaC.txt";

                var f1 = new ZlpFileInfo(longFileOnC);
                f1.Directory.Create();
                f1.WriteAllText("lalala.");
                Console.WriteLine($"f1.FullName.Length: {f1.FullName.Length}");

                const string longFileOnD = @"D:\Ablage\test-only\Ablage2\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\Ablage\LalalaD.txt";

                var f2 = new ZlpFileInfo(longFileOnD);
                f2.Directory.Create();

                //f1.MoveTo(f2, true);
                Console.WriteLine($"f2.FullName.Length: {f2.FullName.Length}");

                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("sourceFile:");
                Console.WriteLine(f1.FullName);
                Console.WriteLine();
                Console.WriteLine("destinationFile:");
                Console.WriteLine(f2.FullName);

                ZlpIOHelper.MoveFile(f1.FullName, f2.FullName, true);
            }
            finally
            {
                Console.WriteLine();

                const string cAblageTestOnly = @"C:\Ablage\test-only\";
                new ZlpDirectoryInfo(cAblageTestOnly).Delete(true);
                Console.WriteLine($"deleted: {cAblageTestOnly}");

                const string dAblageTestOnly = @"D:\Ablage\test-only\";
                new ZlpDirectoryInfo(dAblageTestOnly).Delete(true);
                Console.WriteLine($"deleted: {dAblageTestOnly}");
            }

        }
    }
}