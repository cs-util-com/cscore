using StbImageSharp.Decoding;
using System.IO;

namespace StbImageSharp
{
#if !STBSHARP_INTERNAL
	public
#else
	internal
#endif
	struct ImageInfo
	{
		public int Width;
		public int Height;
		public ColorComponents ColorComponents;
		public int BitsPerChannel;

		public static ImageInfo? FromStream(Stream stream)
		{
			ImageInfo? info = null;

			if (JpgDecoder.Test(stream))
			{
				info = JpgDecoder.Info(stream);
			}
			else if (PngDecoder.Test(stream))
			{
				info = PngDecoder.Info(stream);
			}
			else if (BmpDecoder.Test(stream))
			{
				info = BmpDecoder.Info(stream);
			}
			else if (GifDecoder.Test(stream))
			{
				info = GifDecoder.Info(stream);
			}
			else if (PsdDecoder.Test(stream))
			{
				info = PsdDecoder.Info(stream);
			}
			else if (TgaDecoder.Test(stream))
			{
				info = TgaDecoder.Info(stream);
			}

			return info;
		}
	}
}
