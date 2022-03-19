# Zeta Long Paths

A .NET library to access files and directories with more than 260 characters length.

<!--[![Build state](https://travis-ci.org/UweKeim/ZetaLongPaths.svg?branch=master)](https://travis-ci.org/UweKeim/ZetaLongPaths "Travis CI build status")-->

## Introduction

This is a library that provides several classes and functions to perform basic operations on file paths and folder paths that are longer than the `MAX_PATH` limit of 260 characters.

If you want to use the additional convenience functions of this library with normal non-long paths, and in both .NET Core and Full, please see my new .NET Standard package [Zeta Short Paths](https://github.com/UweKeim/ZetaShortPaths).

## Quick usage

- **[NuGet .NET 4.5.2 package](https://www.nuget.org/packages/ZetaLongPaths)**

## Background

All .NET functions I came across that access the file system are limited to file paths and folder paths with less than 260 characters. This includes most (all?) of the classes in the [System.IO](http://msdn.microsoft.com/en-us/library/system.io.aspx) namespace like e.g. the [System.IO.FileInfo](http://msdn.microsoft.com/en-us/library/system.io.fileinfo.aspx) class.

Since I was in the need to actually access paths with more than 260 characters, I searched for a solution. Fortunately a solution exists; basically you have to P/Invoke Win32 functions that allow a special syntax to prefix a file and allow it then to be much longer than the 260 characters (about 32,000 characters).

## The library

So I started writing a very thin wrapper for the functions I required to work on long file names.

These resources helped me finding more:

  * "[Long Paths in .NET](http://blogs.msdn.com/bclteam/archive/2007/02/13/long-paths-in-net-part-1-of-3-kim-hamilton.aspx)" on the BCL Team Blog.
  * [pinvoke.net](http://pinvoke.net/) for finding signatures of Win32 functions.
  * [Using long path syntax with UNC](http://msdn.microsoft.com/en-us/library/aa365247.aspx), MSDN.

I started by using several functions from the BCL Team blog postings and added the functions they did not cover but which I needed in my project.

Among others, there are the following classes:

  * `ZlpFileInfo` - A class similar to [System.IO.FileInfo](http://msdn.microsoft.com/en-us/library/system.io.fileinfo.aspx) that wraps functions to work on file paths.
  * `ZlpDirectoryInfo` - A class similar to [System.IO.DirectoryInfo](http://msdn.microsoft.com/en-us/library/system.io.directoryinfo.aspx) that wraps functions to work on folder paths.
  * `ZlpIOHelper` - A set of static functions to provide similar features as the `ZlpFileInfo` and `ZlpDirectoryInfo` class but in a static context.
  * `ZlpPathHelper` - A set of static functions similar to [System.IO.Path](http://msdn.microsoft.com/en-us/library/system.io.path.aspx) that work on paths.

## Using the code

The project contains some unit tests to show basic functions.

If you are familiar with the [System.IO](http://msdn.microsoft.com/en-us/library/system.io.aspx) namespace, you should be able to use the classes of the library.

For example to get all files in a given folder path, use the following snippet:

    var folderPath = new ZlpDirectoryInfo( @"C:\My\Long\Folder\Path" );
	 
    foreach ( var filePath in folderPath.GetFiles() )
    {
        Console.Write( "File {0} has a size of {1}", 
            filePath.FullName, 
            filePath.Length );
    }

## Other libraries

Beside this library, there are other libraries available for accessing longer paths:

- [Long Path](http://bcl.codeplex.com/releases/view/42783) from the BCL CodePlex library.
- [Delimon.Win32.IO Library](https://gallery.technet.microsoft.com/DelimonWin32IO-Library-V40-7ff6b16c) from the Microsoft TechNet Gallery.
- [AlphaFS](https://github.com/alphaleonis/AlphaFS)

Personally, I've used none of these libraries. When I started developing this library either none of the other libraries existed or I have poorly searched.

According to user comments, the _Long Path_ library is rather restricted in terms of functionality; the _Delimon_ library is apparently much more powerful than my library.

## Conclusion

Please note that the library currently is limited in the number of provided functions. I will add more functions in the future, just tell me which you require.

I'm using the library in several widely used real-life projects like our **[Content Management System (CMS)](https://www.zeta-producer.com)**, our **[Test and Requirements Management tool](https://www.zeta-test.com)** and our **[Large File Uploader](https://www.zeta-uploader.com)**, so the library should be rather stable and reliable.

## History

(The full history is always available in the [commits list](https://github.com/UweKeim/ZetaLongPaths/commits/master)).

  * *2016-09-27* - I've just discovered that [.NET 4.6.2 now supports long paths natively](https://blogs.msdn.microsoft.com/dotnet/2016/08/02/announcing-net-framework-4-6-2/). So if you are using my library you probably don't need it anymore if you target .NET 4.6.2 or above. (Or maybe [it is not yet ready for prime time](https://blogs.msdn.microsoft.com/jeremykuhne/2016/07/30/net-4-6-2-and-long-paths-on-windows-10/))
  * *2016-08-12* - First introduction of a .NET Core library (.NET Standard 1.6). See [this NuGet package](https://www.nuget.org/packages/ZetaLongPaths.NetStandard).
  * *2016-07-28* - Added functions to deal with short (8.3 "DOS") and long paths.
  * *2014-07-18* - Added functions like `MoveFileToRecycleBin()` to delete files and folders by moving them to the recycle bin.
  * *2014-06-25* - First release to GitHub. Also available at [The Code Project](http://www.codeproject.com/Articles/44904/Zeta-Long-Paths).
  * *2012-12-21* - Added an [NuGet package](http://nuget.org/packages/ZetaLongPaths).
  * *2012-09-20* - Some very few methods added. Stability release.
  * *2012-08-10* - Added several new methods.
  * *2011-10-11* - Fixed an issue inside _ZlpFileInfo.Exists_.
  * *2011-01-31* - Added functions _MoveFile_ and _MoveDirectory_.
  * *2010-03-24* - Added functions to get file owner, creation time, last access time, last write time.
  * *2010-02-16* - Maintenance release.
  * *2009-11-25* - First release to [CodePlex.com](https://zetalongpaths.codeplex.com).

(This is not a complete history; only the milestones are noted)
