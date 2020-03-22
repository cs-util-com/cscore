# StbImageLib
[![NuGet](https://img.shields.io/nuget/v/StbImageLib.svg)](https://www.nuget.org/packages/StbImageLib/) [![Build status](https://ci.appveyor.com/api/projects/status/w6os3e5th6p529la?svg=true)](https://ci.appveyor.com/project/RomanShapiro/stbimagelib) [![Chat](https://img.shields.io/discord/628186029488340992.svg)](https://discord.gg/ZeHxhCY)

StbImageLib is safe and refactored version of [StbImageSharp](https://github.com/StbSharp/StbImageSharp).

# Adding Reference
There are two ways of referencing StbImageLib in the project:
1. Through nuget: https://www.nuget.org/packages/StbImageLib/
2. As submodule:
    
    a. `git submodule add https://github.com/StbSharp/StbImageLib.git`
    
    b. Now there are two options:
       
      * Add StbImageLib/src/StbImageLib/StbImageLib.csproj to the solution
       
      * Include *.cs from StbImageLib/src/StbImageLib directly in the project. In this case, it might make sense to add STBSHARP_INTERNAL build compilation symbol to the project, so StbImageLib classes would become internal.

# Usage
Following code loads image from stream and converts it to 32-bit RGBA:
```c#
	ImageResult image;
	using (var stream = File.OpenRead(path))
	{
		image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
	}
```

If you are writing MonoGame application and would like to convert that data to the Texture2D. It could be done following way:
```c#
Texture2D texture = new Texture2D(GraphicsDevice, image.Width, image.Height, false, SurfaceFormat.Color);
texture.SetData(image.Data);
```

Or if you are writing WinForms app and would like StbSharp resulting bytes to be converted to the Bitmap. The sample code is:
```c#
byte[] data = image.Data;
// Convert rgba to bgra
for (int i = 0; i < x*y; ++i)
{
	byte r = data[i*4];
	byte g = data[i*4 + 1];
	byte b = data[i*4 + 2];
	byte a = data[i*4 + 3];


	data[i*4] = b;
	data[i*4 + 1] = g;
	data[i*4 + 2] = r;
	data[i*4 + 3] = a;
}

// Create Bitmap
Bitmap bmp = new Bitmap(_loadedImage.Width, _loadedImage.Height, PixelFormat.Format32bppArgb);
BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, _loadedImage.Width, _loadedImage.Height), ImageLockMode.WriteOnly,
	bmp.PixelFormat);

Marshal.Copy(data, 0, bmpData.Scan0, bmpData.Stride*bmp.Height);
bmp.UnlockBits(bmpData);
```

# Reliability & Performance
This repo contains special app that was written to measure reliability & performance of StbImageLib in comparison to the original stb_image.h: https://github.com/StbSharp/StbImageLib/tree/master/tests/StbImageSharp.Testing

It goes through every image file in the specified folder and tries to load it 10 times with StbImageLib, then 10 times with C++/CLI wrapper over theoriginal stb_image.h(Stb.Native). Then it compares whether the results are byte-wise similar and also calculates loading times. Also it sums up and reports loading times for each method.

I've used it over following set of images: https://github.com/StbSharp/TestImages

The byte-wise comprarison results are similar for both methods(except a few 16-bit PNGs and PSDs that arent supported yet by StbImageLib).

And performance comparison results are:
```
8 -- Total StbSharp Loading From memory Time: 57917 ms
8 -- Total Stb.Native Loading From memory Time: 39427 ms
```

# License
Public Domain

# Credits
* [stb](https://github.com/nothings/stb)
