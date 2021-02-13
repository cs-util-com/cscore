using System.IO;
using StbImageSharp.Utility;

namespace StbImageSharp.Decoding
{
#if !STBSHARP_INTERNAL
	public
#else
	internal
#endif
	class TgaDecoder : Decoder
	{
		private TgaDecoder(Stream stream) : base(stream)
		{
		}

		private static int stbi__tga_get_comp(int bits_per_pixel, int is_grey, out int is_rgb16)
		{
			is_rgb16 = 0;
			switch (bits_per_pixel)
			{
				case 8:
					return 1;
				case 15:
				case 16:
					if (bits_per_pixel == 16 && is_grey != 0)
						return 2;
					is_rgb16 = 1;
					return 3;
				case 24:
				case 32:
					return bits_per_pixel / 8;
				default:
					return 0;
			}
		}

		private void stbi__tga_read_rgb16(FakePtr<byte> _out_)
		{
			var px = (ushort)stbi__get16le();
			var fiveBitMask = (ushort)31;
			var r = (px >> 10) & fiveBitMask;
			var g = (px >> 5) & fiveBitMask;
			var b = px & fiveBitMask;
			_out_[0] = (byte)(r * 255 / 31);
			_out_[1] = (byte)(g * 255 / 31);
			_out_[2] = (byte)(b * 255 / 31);
		}

		private ImageResult InternalDecode(ColorComponents? requiredComponents)
		{
			var tga_offset = (int)stbi__get8();
			var tga_indexed = (int)stbi__get8();
			var tga_image_type = (int)stbi__get8();
			var tga_is_RLE = 0;
			var tga_palette_start = stbi__get16le();
			var tga_palette_len = stbi__get16le();
			var tga_palette_bits = (int)stbi__get8();
			var tga_x_origin = stbi__get16le();
			var tga_y_origin = stbi__get16le();
			var tga_width = stbi__get16le();
			var tga_height = stbi__get16le();
			var tga_bits_per_pixel = (int)stbi__get8();
			var tga_comp = 0;
			var tga_rgb16 = 0;
			var tga_inverted = (int)stbi__get8();
			byte[] tga_data;
			byte[] tga_palette = null;
			var i = 0;
			var j = 0;
			var raw_data = new byte[4];
			raw_data[0] = 0;

			var RLE_count = 0;
			var RLE_repeating = 0;
			var read_next_pixel = 1;
			if (tga_image_type >= 8)
			{
				tga_image_type -= 8;
				tga_is_RLE = 1;
			}

			tga_inverted = 1 - ((tga_inverted >> 5) & 1);
			if (tga_indexed != 0)
				tga_comp = stbi__tga_get_comp(tga_palette_bits, 0, out tga_rgb16);
			else
				tga_comp = stbi__tga_get_comp(tga_bits_per_pixel, tga_image_type == 3 ? 1 : 0, out tga_rgb16);
			if (tga_comp == 0)
				stbi__err("bad format");

			tga_data = new byte[tga_width * tga_height * tga_comp];
			stbi__skip(tga_offset);
			if (tga_indexed == 0 && tga_is_RLE == 0 && tga_rgb16 == 0)
			{
				for (i = 0; i < tga_height; ++i)
				{
					var row = tga_inverted != 0 ? tga_height - i - 1 : i;
					stbi__getn(tga_data, row * tga_width * tga_comp, tga_width * tga_comp);
				}
			}
			else
			{
				if (tga_indexed != 0)
				{
					stbi__skip(tga_palette_start);
					tga_palette = new byte[tga_palette_len * tga_comp];
					if (tga_rgb16 != 0)
					{
						var pal_entry = new FakePtr<byte>(tga_palette);
						for (i = 0; i < tga_palette_len; ++i)
						{
							stbi__tga_read_rgb16(pal_entry);
							pal_entry += tga_comp;
						}
					}
					else if (!stbi__getn(tga_palette, 0, tga_palette_len * tga_comp))
					{
						stbi__err("bad palette");
					}
				}

				for (i = 0; i < tga_width * tga_height; ++i)
				{
					if (tga_is_RLE != 0)
					{
						if (RLE_count == 0)
						{
							var RLE_cmd = (int)stbi__get8();
							RLE_count = 1 + (RLE_cmd & 127);
							RLE_repeating = RLE_cmd >> 7;
							read_next_pixel = 1;
						}
						else if (RLE_repeating == 0)
						{
							read_next_pixel = 1;
						}
					}
					else
					{
						read_next_pixel = 1;
					}

					if (read_next_pixel != 0)
					{
						if (tga_indexed != 0)
						{
							var pal_idx = tga_bits_per_pixel == 8 ? stbi__get8() : stbi__get16le();
							if (pal_idx >= tga_palette_len) pal_idx = 0;
							pal_idx *= tga_comp;
							for (j = 0; j < tga_comp; ++j) raw_data[j] = tga_palette[pal_idx + j];
						}
						else if (tga_rgb16 != 0)
						{
							stbi__tga_read_rgb16(new FakePtr<byte>(raw_data));
						}
						else
						{
							for (j = 0; j < tga_comp; ++j) raw_data[j] = stbi__get8();
						}

						read_next_pixel = 0;
					}

					for (j = 0; j < tga_comp; ++j) tga_data[i * tga_comp + j] = raw_data[j];
					--RLE_count;
				}

				if (tga_inverted != 0)
					for (j = 0; j * 2 < tga_height; ++j)
					{
						var index1 = j * tga_width * tga_comp;
						var index2 = (tga_height - 1 - j) * tga_width * tga_comp;
						for (i = tga_width * tga_comp; i > 0; --i)
						{
							var temp = tga_data[index1];
							tga_data[index1] = tga_data[index2];
							tga_data[index2] = temp;
							++index1;
							++index2;
						}
					}
			}

			if (tga_comp >= 3 && tga_rgb16 == 0)
			{
				var tga_pixel = new FakePtr<byte>(tga_data);
				for (i = 0; i < tga_width * tga_height; ++i)
				{
					var temp = tga_pixel[0];
					tga_pixel[0] = tga_pixel[2];
					tga_pixel[2] = temp;
					tga_pixel += tga_comp;
				}
			}

			var req_comp = requiredComponents.ToReqComp();
			if (req_comp != 0 && req_comp != tga_comp)
				tga_data = Conversion.stbi__convert_format(tga_data, tga_comp, req_comp, (uint)tga_width,
					(uint)tga_height);
			tga_palette_start = tga_palette_len = tga_palette_bits = tga_x_origin = tga_y_origin = 0;

			return new ImageResult
			{
				Width = tga_width,
				Height = tga_height,
				SourceComponents = (ColorComponents)tga_comp,
				ColorComponents = requiredComponents != null ? requiredComponents.Value : (ColorComponents)tga_comp,
				BitsPerChannel = 8,
				Data = tga_data
			};
		}

		public static bool Test(Stream stream)
		{
			try
			{
				stream.stbi__get8();
				var tga_color_type = (int)stream.stbi__get8();
				if (tga_color_type > 1)
					return false;
				var sz = (int)stream.stbi__get8();
				if (tga_color_type == 1)
				{
					if (sz != 1 && sz != 9)
						return false;
					stream.stbi__skip(4);
					sz = stream.stbi__get8();
					if (sz != 8 && sz != 15 && sz != 16 && sz != 24 && sz != 32)
						return false;
					stream.stbi__skip(4);
				}
				else
				{
					if (sz != 2 && sz != 3 && sz != 10 && sz != 11)
						return false;
					stream.stbi__skip(9);
				}

				if (stream.stbi__get16le() < 1)
					return false;
				if (stream.stbi__get16le() < 1)
					return false;
				sz = stream.stbi__get8();
				if (tga_color_type == 1 && sz != 8 && sz != 16)
					return false;
				if (sz != 8 && sz != 15 && sz != 16 && sz != 24 && sz != 32)
					return false;

				return true;
			}
			finally
			{
				stream.Rewind();
			}
		}

		public static ImageInfo? Info(Stream stream)
		{
			try
			{
				var tga_w = 0;
				var tga_h = 0;
				var tga_comp = 0;
				var tga_image_type = 0;
				var tga_bits_per_pixel = 0;
				var tga_colormap_bpp = 0;
				var sz = 0;
				var tga_colormap_type = 0;
				stream.stbi__get8();
				tga_colormap_type = stream.stbi__get8();
				if (tga_colormap_type > 1) return null;

				tga_image_type = stream.stbi__get8();
				if (tga_colormap_type == 1)
				{
					if (tga_image_type != 1 && tga_image_type != 9) return null;
					stream.stbi__skip(4);
					sz = stream.stbi__get8();
					if (sz != 8 && sz != 15 && sz != 16 && sz != 24 && sz != 32) return null;
					stream.stbi__skip(4);
					tga_colormap_bpp = sz;
				}
				else
				{
					if (tga_image_type != 2 && tga_image_type != 3 && tga_image_type != 10 && tga_image_type != 11)
						return null;
					stream.stbi__skip(9);
					tga_colormap_bpp = 0;
				}

				tga_w = stream.stbi__get16le();
				if (tga_w < 1) return null;

				tga_h = stream.stbi__get16le();
				if (tga_h < 1) return null;

				tga_bits_per_pixel = stream.stbi__get8();
				stream.stbi__get8();
				int is_rgb16;
				if (tga_colormap_bpp != 0)
				{
					if (tga_bits_per_pixel != 8 && tga_bits_per_pixel != 16) return null;
					tga_comp = stbi__tga_get_comp(tga_colormap_bpp, 0, out is_rgb16);
				}
				else
				{
					tga_comp = stbi__tga_get_comp(tga_bits_per_pixel,
						tga_image_type == 3 || tga_image_type == 11 ? 1 : 0, out is_rgb16);
				}

				if (tga_comp == 0) return null;

				return new ImageInfo
				{
					Width = tga_w,
					Height = tga_h,
					ColorComponents = (ColorComponents)tga_comp,
					BitsPerChannel = tga_bits_per_pixel
				};
			}
			finally
			{
				stream.Rewind();
			}
		}

		public static ImageResult Decode(Stream stream, ColorComponents? requiredComponents = null)
		{
			var decoder = new TgaDecoder(stream);
			return decoder.InternalDecode(requiredComponents);
		}
	}
}