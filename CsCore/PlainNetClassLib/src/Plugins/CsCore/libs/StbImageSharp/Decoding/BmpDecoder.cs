using System;
using System.IO;
using System.Runtime.InteropServices;
using StbImageSharp.Utility;

namespace StbImageSharp.Decoding
{
#if !STBSHARP_INTERNAL
	public
#else
	internal
#endif
	class BmpDecoder : Decoder
	{
		[StructLayout(LayoutKind.Sequential)]
		public struct stbi__bmp_data
		{
			public int bpp;
			public int offset;
			public int hsz;
			public uint mr;
			public uint mg;
			public uint mb;
			public uint ma;
			public uint all_a;
		}

		private static readonly uint[] mul_table = { 0, 0xff, 0x55, 0x49, 0x11, 0x21, 0x41, 0x81, 0x01 };
		private static readonly uint[] shift_table = { 0, 0, 0, 1, 0, 2, 4, 6, 0 };

		private BmpDecoder(Stream stream) : base(stream)
		{
		}

		private static int stbi__high_bit(uint z)
		{
			var n = 0;
			if (z == 0)
				return -1;
			if (z >= 0x10000)
			{
				n += 16;
				z >>= 16;
			}

			if (z >= 0x00100)
			{
				n += 8;
				z >>= 8;
			}

			if (z >= 0x00010)
			{
				n += 4;
				z >>= 4;
			}

			if (z >= 0x00004)
			{
				n += 2;
				z >>= 2;
			}

			if (z >= 0x00002)
			{
				n += 1;
				z >>= 1;
			}

			return n;
		}

		private static int stbi__bitcount(uint a)
		{
			a = (a & 0x55555555) + ((a >> 1) & 0x55555555);
			a = (a & 0x33333333) + ((a >> 2) & 0x33333333);
			a = (a + (a >> 4)) & 0x0f0f0f0f;
			a = a + (a >> 8);
			a = a + (a >> 16);
			return (int)(a & 0xff);
		}

		private static int stbi__shiftsigned(uint v, int shift, int bits)
		{
			if (shift < 0)
				v <<= -shift;
			else
				v >>= shift;
			v >>= 8 - bits;
			return (int)(v * (int)mul_table[bits]) >> (int)shift_table[bits];
		}

		private void stbi__bmp_parse_header(ref stbi__bmp_data info)
		{
			var hsz = 0;
			if (stbi__get8() != 'B' || stbi__get8() != 'M')
				stbi__err("not BMP");
			stbi__get32le();
			stbi__get16le();
			stbi__get16le();
			info.offset = (int)stbi__get32le();
			info.hsz = hsz = (int)stbi__get32le();
			info.mr = info.mg = info.mb = info.ma = 0;
			if (hsz != 12 && hsz != 40 && hsz != 56 && hsz != 108 && hsz != 124)
				stbi__err("unknown BMP");
			if (hsz == 12)
			{
				img_x = stbi__get16le();
				img_y = stbi__get16le();
			}
			else
			{
				img_x = (int)stbi__get32le();
				img_y = (int)stbi__get32le();
			}

			if (stbi__get16le() != 1)
				stbi__err("bad BMP");
			info.bpp = stbi__get16le();
			if (hsz != 12)
			{
				var compress = (int)stbi__get32le();
				if (compress == 1 || compress == 2)
					stbi__err("BMP RLE");
				stbi__get32le();
				stbi__get32le();
				stbi__get32le();
				stbi__get32le();
				stbi__get32le();
				if (hsz == 40 || hsz == 56)
				{
					if (hsz == 56)
					{
						stbi__get32le();
						stbi__get32le();
						stbi__get32le();
						stbi__get32le();
					}

					if (info.bpp == 16 || info.bpp == 32)
					{
						if (compress == 0)
						{
							if (info.bpp == 32)
							{
								info.mr = 0xffu << 16;
								info.mg = 0xffu << 8;
								info.mb = 0xffu << 0;
								info.ma = 0xffu << 24;
								info.all_a = 0;
							}
							else
							{
								info.mr = 31u << 10;
								info.mg = 31u << 5;
								info.mb = 31u << 0;
							}
						}
						else if (compress == 3)
						{
							info.mr = stbi__get32le();
							info.mg = stbi__get32le();
							info.mb = stbi__get32le();
							if (info.mr == info.mg && info.mg == info.mb) stbi__err("bad BMP");
						}
						else
						{
							stbi__err("bad BMP");
						}
					}
				}
				else
				{
					var i = 0;
					if (hsz != 108 && hsz != 124)
						stbi__err("bad BMP");
					info.mr = stbi__get32le();
					info.mg = stbi__get32le();
					info.mb = stbi__get32le();
					info.ma = stbi__get32le();
					stbi__get32le();
					for (i = 0; i < 12; ++i) stbi__get32le();
					if (hsz == 124)
					{
						stbi__get32le();
						stbi__get32le();
						stbi__get32le();
						stbi__get32le();
					}
				}
			}
		}

		private ImageResult InternalDecode(ColorComponents? requiredComponents)
		{
			byte[] _out_;
			var mr = (uint)0;
			var mg = (uint)0;
			var mb = (uint)0;
			var ma = (uint)0;
			uint all_a = 0;
			var pal = new byte[256 * 4];
			var psize = 0;
			var i = 0;
			var j = 0;
			var width = 0;
			var flip_vertically = 0;
			var pad = 0;
			var target = 0;
			var info = new stbi__bmp_data();
			info.all_a = 255;
			stbi__bmp_parse_header(ref info);
			flip_vertically = img_y > 0 ? 1 : 0;
			img_y = Math.Abs(img_y);
			mr = info.mr;
			mg = info.mg;
			mb = info.mb;
			ma = info.ma;
			all_a = info.all_a;
			if (info.hsz == 12)
			{
				if (info.bpp < 24)
					psize = (info.offset - 14 - 24) / 3;
			}
			else
			{
				if (info.bpp < 16)
					psize = (info.offset - 14 - info.hsz) >> 2;
			}

			img_n = ma != 0 ? 4 : 3;
			if (requiredComponents != null && (int)requiredComponents.Value >= 3)
				target = (int)requiredComponents.Value;
			else
				target = img_n;
			_out_ = new byte[target * img_x * img_y];
			if (info.bpp < 16)
			{
				var z = 0;
				if (psize == 0 || psize > 256) stbi__err("invalid");
				for (i = 0; i < psize; ++i)
				{
					pal[i * 4 + 2] = stbi__get8();
					pal[i * 4 + 1] = stbi__get8();
					pal[i * 4 + 0] = stbi__get8();
					if (info.hsz != 12)
						stbi__get8();
					pal[i * 4 + 3] = 255;
				}

				stbi__skip(info.offset - 14 - info.hsz - psize * (info.hsz == 12 ? 3 : 4));
				if (info.bpp == 1)
					width = (img_x + 7) >> 3;
				else if (info.bpp == 4)
					width = (img_x + 1) >> 1;
				else if (info.bpp == 8)
					width = img_x;
				else
					stbi__err("bad bpp");
				pad = -width & 3;
				if (info.bpp == 1)
					for (j = 0; j < img_y; ++j)
					{
						var bit_offset = 7;
						var v = (int)stbi__get8();
						for (i = 0; i < img_x; ++i)
						{
							var color = (v >> bit_offset) & 0x1;
							_out_[z++] = pal[color * 4 + 0];
							_out_[z++] = pal[color * 4 + 1];
							_out_[z++] = pal[color * 4 + 2];
							if (target == 4)
								_out_[z++] = 255;
							if (i + 1 == img_x)
								break;
							if (--bit_offset < 0)
							{
								bit_offset = 7;
								v = stbi__get8();
							}
						}

						stbi__skip(pad);
					}
				else
					for (j = 0; j < img_y; ++j)
					{
						for (i = 0; i < img_x; i += 2)
						{
							var v = (int)stbi__get8();
							var v2 = 0;
							if (info.bpp == 4)
							{
								v2 = v & 15;
								v >>= 4;
							}

							_out_[z++] = pal[v * 4 + 0];
							_out_[z++] = pal[v * 4 + 1];
							_out_[z++] = pal[v * 4 + 2];
							if (target == 4)
								_out_[z++] = 255;
							if (i + 1 == img_x)
								break;
							v = info.bpp == 8 ? stbi__get8() : v2;
							_out_[z++] = pal[v * 4 + 0];
							_out_[z++] = pal[v * 4 + 1];
							_out_[z++] = pal[v * 4 + 2];
							if (target == 4)
								_out_[z++] = 255;
						}

						stbi__skip(pad);
					}
			}
			else
			{
				var rshift = 0;
				var gshift = 0;
				var bshift = 0;
				var ashift = 0;
				var rcount = 0;
				var gcount = 0;
				var bcount = 0;
				var acount = 0;
				var z = 0;
				var easy = 0;
				stbi__skip(info.offset - 14 - info.hsz);
				if (info.bpp == 24)
					width = 3 * img_x;
				else if (info.bpp == 16)
					width = 2 * img_x;
				else
					width = 0;
				pad = -width & 3;
				if (info.bpp == 24)
					easy = 1;
				else if (info.bpp == 32)
					if (mb == 0xff && mg == 0xff00 && mr == 0x00ff0000 && ma == 0xff000000)
						easy = 2;
				if (easy == 0)
				{
					if (mr == 0 || mg == 0 || mb == 0) stbi__err("bad masks");
					rshift = stbi__high_bit(mr) - 7;
					rcount = stbi__bitcount(mr);
					gshift = stbi__high_bit(mg) - 7;
					gcount = stbi__bitcount(mg);
					bshift = stbi__high_bit(mb) - 7;
					bcount = stbi__bitcount(mb);
					ashift = stbi__high_bit(ma) - 7;
					acount = stbi__bitcount(ma);
				}

				for (j = 0; j < img_y; ++j)
				{
					if (easy != 0)
					{
						for (i = 0; i < img_x; ++i)
						{
							byte a = 0;
							_out_[z + 2] = stbi__get8();
							_out_[z + 1] = stbi__get8();
							_out_[z + 0] = stbi__get8();
							z += 3;
							a = (byte)(easy == 2 ? stbi__get8() : 255);
							all_a |= a;
							if (target == 4)
								_out_[z++] = a;
						}
					}
					else
					{
						var bpp = info.bpp;
						for (i = 0; i < img_x; ++i)
						{
							var v = bpp == 16 ? (uint)stbi__get16le() : stbi__get32le();
							uint a = 0;
							_out_[z++] = (byte)(stbi__shiftsigned(v & mr, rshift, rcount) & 255);
							_out_[z++] = (byte)(stbi__shiftsigned(v & mg, gshift, gcount) & 255);
							_out_[z++] = (byte)(stbi__shiftsigned(v & mb, bshift, bcount) & 255);
							a = (uint)(ma != 0 ? stbi__shiftsigned(v & ma, ashift, acount) : 255);
							all_a |= a;
							if (target == 4)
								_out_[z++] = (byte)(a & 255);
						}
					}

					stbi__skip(pad);
				}
			}

			if (target == 4 && all_a == 0)
				for (i = 4 * img_x * img_y - 1; i >= 0; i -= 4)
					_out_[i] = 255;
			if (flip_vertically != 0)
			{
				byte t = 0;
				var ptr = new FakePtr<byte>(_out_);
				for (j = 0; j < img_y >> 1; ++j)
				{
					var p1 = ptr + j * img_x * target;
					var p2 = ptr + (img_y - 1 - j) * img_x * target;
					for (i = 0; i < img_x * target; ++i)
					{
						t = p1[i];
						p1[i] = p2[i];
						p2[i] = t;
					}
				}
			}

			if (requiredComponents != null && (int)requiredComponents.Value != target)
				_out_ = Conversion.stbi__convert_format(_out_, target, (int)requiredComponents.Value, (uint)img_x,
					(uint)img_y);

			return new ImageResult
			{
				Width = img_x,
				Height = img_y,
				SourceComponents = (ColorComponents)img_n,
				ColorComponents = requiredComponents != null ? requiredComponents.Value : (ColorComponents)img_n,
				BitsPerChannel = 8,
				Data = _out_
			};
		}

		private static bool TestInternal(Stream stream)
		{
			var sz = 0;
			if (stream.ReadByte() != 'B')
				return false;
			if (stream.ReadByte() != 'M')
				return false;

			stream.stbi__get32le();
			stream.stbi__get16le();
			stream.stbi__get16le();
			stream.stbi__get32le();
			sz = (int)stream.stbi__get32le();
			var r = sz == 12 || sz == 40 || sz == 56 || sz == 108 || sz == 124;
			return r;
		}

		public static bool Test(Stream stream)
		{
			var r = TestInternal(stream);
			stream.Rewind();
			return r;
		}

		public static ImageInfo? Info(Stream stream)
		{
			var info = new stbi__bmp_data
			{
				all_a = 255
			};

			var decoder = new BmpDecoder(stream);
			try
			{
				decoder.stbi__bmp_parse_header(ref info);
			}
			catch (Exception)
			{
				return null;
			}
			finally
			{
				stream.Rewind();
			}

			return new ImageInfo
			{
				Width = decoder.img_x,
				Height = decoder.img_y,
				ColorComponents = info.ma != 0 ? ColorComponents.RedGreenBlueAlpha : ColorComponents.RedGreenBlue,
				BitsPerChannel = 8
			};
		}

		public static ImageResult Decode(Stream stream, ColorComponents? requiredComponents = null)
		{
			var decoder = new BmpDecoder(stream);
			return decoder.InternalDecode(requiredComponents);
		}
	}
}