using StbImageSharp.Decoding;
using System.IO;

namespace StbImageSharp
{
#if !STBSHARP_INTERNAL
	public
#else
	internal
#endif
	class ImageResult
	{
		public int Width { get; set; }
		public int Height { get; set; }
		public ColorComponents ColorComponents { get; set; }
		public ColorComponents SourceComponents { get; set; }

		/// <summary>
		/// Either 8 or 16
		/// </summary>
		public int BitsPerChannel { get; set; }
		public byte[] Data { get; set; }

		public static ImageResult FromMemory(byte[] data, ColorComponents? requiredComponents = null, bool use8BitsPerChannel = true)
		{
			using (var stream = new MemoryStream(data))
			{
				return FromStream(stream, requiredComponents, use8BitsPerChannel);
			}
		}

		public static ImageResult FromStream(Stream stream, ColorComponents? requiredComponents = null, bool use8BitsPerChannel = true)
		{
			ImageResult result = null;
			if (JpgDecoder.Test(stream))
			{
				result = JpgDecoder.Decode(stream, requiredComponents);
			}
			else if (PngDecoder.Test(stream))
			{
				result = PngDecoder.Decode(stream, requiredComponents);
			}
			else if (BmpDecoder.Test(stream))
			{
				result = BmpDecoder.Decode(stream, requiredComponents);
			}
			else if (GifDecoder.Test(stream))
			{
				result = GifDecoder.Decode(stream, requiredComponents);
			}
			else if (PsdDecoder.Test(stream))
			{
				result = PsdDecoder.Decode(stream, requiredComponents);
			}
			else if (TgaDecoder.Test(stream))
			{
				result = TgaDecoder.Decode(stream, requiredComponents);
			}

			if (result == null)
			{
				Decoder.stbi__err("unknown image type");
			}

			if (use8BitsPerChannel && result.BitsPerChannel != 8)
			{
				result.Data = Conversion.stbi__convert_16_to_8(result.Data, result.Width, result.Height, (int)result.ColorComponents);
			}

			return result;
		}
	}
}