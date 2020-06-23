To allow the visual regression system to compare screenshots pixel by pixel and detect 
differences, ImageMagick is used, which is a C++ library that handles image loading, 
resizing, comparison. This library is added to cscore as an optional dependency since
it adds significant size (the C++ library binaries) and can't be used on any runtime
Unity platform out of the box. 

You will have to open the following links and there click the "Download package" buttons to
download the 2 needed .nupkg files. These downloaded .nupkg file can be opened with any 
zip application like 7zip ( https://www.7-zip.org/ )

https://www.nuget.org/packages/Magick.NET-Q8-AnyCPU/ 
(This contains the native C++ library with all the image loading logic)
1. Copy the complete \runtimes folder into your project (to 
   Assets\Plugins\CsCore\libs\Magick_NET\runtimes\).
2. On Windows: If you have a 32bit system delete the \win-x64 folder, if you have a 64bit 
   system delete the \win-x86 folder (Alternatively to deleting you can also configure Unity
   to use the correct DLL if you want to include it in a built)
3. Navitate to the folder \lib\netstandard20\ and extract the DLL in
   there into your project (to Assets\Plugins\CsCore\libs\Magick_NET).

https://www.nuget.org/packages/Magick.NET.Core/
(This contains pure C# code and is the wrapper for the native C++ library)
4. Navitate to the folder \lib\netstandard20\ and extract the DLL in
   there into your project (to Assets\Plugins\CsCore\libs\Magick_NET).

Now you included the required DLLs into your project. 

The last think left to do is enable the ENABLE_IMAGE_MAGICK define so that the parts of the 
cscore library become active that use ImageMagick. To do that either put it anywhere in 
your code like this:

#define ENABLE_IMAGE_MAGICK
(Docu what a #define is at https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/preprocessor-directives/preprocessor-define )

Alternatively open your Unity Project Settings -> Player and there add ENABLE_IMAGE_MAGICK to 
the "Scripting Define Symbols" which causes the same effect. Docu about this feature: 
https://docs.unity3d.com/Manual/PlatformDependentCompilation.html