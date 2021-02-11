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
	class GifDecoder : Decoder
	{
		[StructLayout(LayoutKind.Sequential)]
		private struct stbi__gif_lzw
		{
			public short prefix;
			public byte first;
			public byte suffix;
		}

		private int w;
		private int h;
		private byte[] _out_;
		private byte[] background;
		private byte[] history;
		private int flags;
		private int bgindex;
		private int ratio;
		private int transparent;
		private int eflags;
		private int delay;
		private readonly byte[] pal;
		private readonly byte[] lpal;
		private readonly stbi__gif_lzw[] codes = new stbi__gif_lzw[8192];
		private byte[] color_table;
		private int parse;
		private int step;
		private int lflags;
		private int start_x;
		private int start_y;
		private int max_x;
		private int max_y;
		private int cur_x;
		private int cur_y;
		private int line_size;

		private GifDecoder(Stream stream) : base(stream)
		{
			pal = new byte[256 * 4];
			lpal = new byte[256 * 4];
		}

		private void stbi__gif_parse_colortable(byte[] pal, int num_entries, int transp)
		{
			int i;
			for (i = 0; i < num_entries; ++i)
			{
				pal[i * 4 + 2] = stbi__get8();
				pal[i * 4 + 1] = stbi__get8();
				pal[i * 4] = stbi__get8();
				pal[i * 4 + 3] = (byte)(transp == i ? 0 : 255);
			}
		}

		private int stbi__gif_header(out int comp, int is_info)
		{
			byte version = 0;
			if (stbi__get8() != 'G' || stbi__get8() != 'I' || stbi__get8() != 'F' || stbi__get8() != '8')
				stbi__err("not GIF");
			version = stbi__get8();
			if (version != '7' && version != '9')
				stbi__err("not GIF");
			if (stbi__get8() != 'a')
				stbi__err("not GIF");
			w = stbi__get16le();
			h = stbi__get16le();
			flags = stbi__get8();
			bgindex = stbi__get8();
			ratio = stbi__get8();
			transparent = -1;

			comp = 4;
			if (is_info != 0)
				return 1;
			if ((flags & 0x80) != 0)
				stbi__gif_parse_colortable(pal, 2 << (flags & 7), -1);
			return 1;
		}

		private void stbi__out_gif_code(ushort code)
		{
			var idx = 0;
			if (codes[code].prefix >= 0)
				stbi__out_gif_code((ushort)codes[code].prefix);
			if (cur_y >= max_y)
				return;
			idx = cur_x + cur_y;
			history[idx / 4] = 1;
			var c = new FakePtr<byte>(color_table, codes[code].suffix * 4);
			if (c[3] > 128)
			{
				var p = new FakePtr<byte>(_out_, idx);
				p[0] = c[2];
				p[1] = c[1];
				p[2] = c[0];
				p[3] = c[3];
			}

			cur_x += 4;
			if (cur_x >= max_x)
			{
				cur_x = start_x;
				cur_y += step;
				while (cur_y >= max_y && parse > 0)
				{
					step = (1 << parse) * line_size;
					cur_y = start_y + (step >> 1);
					--parse;
				}
			}
		}

		private byte[] stbi__process_gif_raster()
		{
			byte lzw_cs = 0;
			var len = 0;
			var init_code = 0;
			uint first = 0;
			var codesize = 0;
			var codemask = 0;
			var avail = 0;
			var oldcode = 0;
			var bits = 0;
			var valid_bits = 0;
			var clear = 0;
			lzw_cs = stbi__get8();
			if (lzw_cs > 12)
				return null;
			clear = 1 << lzw_cs;
			first = 1;
			codesize = lzw_cs + 1;
			codemask = (1 << codesize) - 1;
			bits = 0;
			valid_bits = 0;
			for (init_code = 0; init_code < clear; init_code++)
			{
				codes[init_code].prefix = -1;
				codes[init_code].first = (byte)init_code;
				codes[init_code].suffix = (byte)init_code;
			}

			avail = clear + 2;
			oldcode = -1;
			len = 0;
			for (; ; )
				if (valid_bits < codesize)
				{
					if (len == 0)
					{
						len = stbi__get8();
						if (len == 0)
							return _out_;
					}

					--len;
					bits |= stbi__get8() << valid_bits;
					valid_bits += 8;
				}
				else
				{
					var code = bits & codemask;
					bits >>= codesize;
					valid_bits -= codesize;
					if (code == clear)
					{
						codesize = lzw_cs + 1;
						codemask = (1 << codesize) - 1;
						avail = clear + 2;
						oldcode = -1;
						first = 0;
					}
					else if (code == clear + 1)
					{
						stbi__skip(len);
						while ((len = stbi__get8()) > 0) stbi__skip(len);
						return _out_;
					}
					else if (code <= avail)
					{
						if (first != 0) stbi__err("no clear code");
						if (oldcode >= 0)
						{
							var idx = avail++;
							if (avail > 8192) stbi__err("too many codes");
							codes[idx].prefix = (short)oldcode;
							codes[idx].first = codes[oldcode].first;
							codes[idx].suffix = code == avail ? codes[idx].first : codes[code].first;
						}
						else if (code == avail)
						{
							stbi__err("illegal code in raster");
						}

						stbi__out_gif_code((ushort)code);
						if ((avail & codemask) == 0 && avail <= 0x0FFF)
						{
							codesize++;
							codemask = (1 << codesize) - 1;
						}

						oldcode = code;
					}
					else
					{
						stbi__err("illegal code in raster");
					}
				}
		}

		private byte[] stbi__gif_load_next(out int comp, FakePtr<byte>? two_back)
		{
			comp = 0;

			var dispose = 0;
			var first_frame = 0;
			var pi = 0;
			var pcount = 0;
			first_frame = 0;
			if (_out_ == null)
			{
				if (stbi__gif_header(out comp, 0) == 0)
					return null;
				pcount = w * h;
				_out_ = new byte[4 * pcount];
				Array.Clear(_out_, 0, _out_.Length);
				background = new byte[4 * pcount];
				Array.Clear(background, 0, background.Length);
				history = new byte[pcount];
				Array.Clear(history, 0, history.Length);
				first_frame = 1;
			}
			else
			{
				var ptr = new FakePtr<byte>(_out_);
				dispose = (eflags & 0x1C) >> 2;
				pcount = w * h;
				if (dispose == 3 && two_back == null) dispose = 2;
				if (dispose == 3)
				{
					for (pi = 0; pi < pcount; ++pi)
						if (history[pi] != 0)
							new FakePtr<byte>(ptr, pi * 4).memcpy(new FakePtr<byte>(two_back.Value, pi * 4), 4);
				}
				else if (dispose == 2)
				{
					for (pi = 0; pi < pcount; ++pi)
						if (history[pi] != 0)
							new FakePtr<byte>(ptr, pi * 4).memcpy(new FakePtr<byte>(background, pi * 4), 4);
				}

				new FakePtr<byte>(background).memcpy(ptr, 4 * w * h);
			}

			Array.Clear(history, 0, w * h);
			for (; ; )
			{
				var tag = (int)stbi__get8();
				switch (tag)
				{
					case 0x2C:
					{
						var x = 0;
						var y = 0;
						var w = 0;
						var h = 0;
						byte[] o;
						x = stbi__get16le();
						y = stbi__get16le();
						w = stbi__get16le();
						h = stbi__get16le();
						if (x + w > w || y + h > h)
							stbi__err("bad Image Descriptor");
						line_size = w * 4;
						start_x = x * 4;
						start_y = y * line_size;
						max_x = start_x + w * 4;
						max_y = start_y + h * line_size;
						cur_x = start_x;
						cur_y = start_y;
						if (w == 0)
							cur_y = max_y;
						lflags = stbi__get8();
						if ((lflags & 0x40) != 0)
						{
							step = 8 * line_size;
							parse = 3;
						}
						else
						{
							step = line_size;
							parse = 0;
						}

						if ((lflags & 0x80) != 0)
						{
							stbi__gif_parse_colortable(lpal, 2 << (lflags & 7),
								(eflags & 0x01) != 0 ? transparent : -1);
							color_table = lpal;
						}
						else if ((flags & 0x80) != 0)
						{
							color_table = pal;
						}
						else
						{
							stbi__err("missing color table");
						}

						o = stbi__process_gif_raster();
						if (o == null)
							return null;
						pcount = w * h;
						if (first_frame != 0 && bgindex > 0)
							for (pi = 0; pi < pcount; ++pi)
								if (history[pi] == 0)
								{
									pal[bgindex * 4 + 3] = 255;
									new FakePtr<byte>(_out_, pi * 4).memcpy(new FakePtr<byte>(pal, bgindex), 4);
								}

						return o;
					}
					case 0x21:
					{
						var len = 0;
						var ext = (int)stbi__get8();
						if (ext == 0xF9)
						{
							len = stbi__get8();
							if (len == 4)
							{
								eflags = stbi__get8();
								delay = 10 * stbi__get16le();
								if (transparent >= 0) pal[transparent * 4 + 3] = 255;
								if ((eflags & 0x01) != 0)
								{
									transparent = stbi__get8();
									if (transparent >= 0) pal[transparent * 4 + 3] = 0;
								}
								else
								{
									stbi__skip(1);
									transparent = -1;
								}
							}
							else
							{
								stbi__skip(len);
								break;
							}
						}

						while ((len = stbi__get8()) != 0) stbi__skip(len);
						break;
					}
					case 0x3B:
						return null;
					default:
						stbi__err("unknown code");
						break;
				}
			}
		}

	/*		private void* stbi__load_gif_main(int** delays, int* x, int* y, int* z, int* comp, int req_comp)
			{
				if ((IsGif(Stream)))
				{
					int layers = (int)(0);
					byte* u = null;
					byte* _out_ = null;
					byte* two_back = null;
					int stride = 0;
					if ((delays) != null)
					{
						*delays = null;
					}
					do
					{
						u = stbi__gif_load_next(comp, (int)(req_comp), two_back);
						if ((u) != null)
						{
							*x = (int)(w);
							*y = (int)(h);
							++layers;
							stride = (int)(w * h * 4);
							if ((_out_) != null)
							{
								_out_ = (byte*)(CRuntime.realloc(_out_, (ulong)(layers * stride)));
								if ((delays) != null)
								{
									*delays = (int*)(CRuntime.realloc(*delays, (ulong)(sizeof(int) * layers)));
								}
							}
							else
							{
								_out_ = (byte*)(Utility.stbi__malloc((ulong)(layers * stride)));
								if ((delays) != null)
								{
									*delays = (int*)(Utility.stbi__malloc((ulong)(layers * sizeof(int))));
								}
							}
							CRuntime.memcpy(_out_ + ((layers - 1) * stride), u, (ulong)(stride));
							if ((layers) >= (2))
							{
								two_back = _out_ - 2 * stride;
							}
							if ((delays) != null)
							{
								(*delays)[layers - 1U] = (int)(delay);
							}
						}
					}
					while (u != null);
					CRuntime.free(_out_);
					CRuntime.free(history);
					CRuntime.free(background);
					if (((req_comp) != 0) && (req_comp != 4))
						_out_ = stbi__convert_format(_out_, (int)(4), (int)(req_comp), (uint)(layers * w), (uint)(h));
					*z = (int)(layers);
					return _out_;
				}
				else
				{
					stbi__err("not GIF");
				}

			}*/

		private ImageResult InternalDecode(ColorComponents? requiredComponents)
		{
			int comp;
			var u = stbi__gif_load_next(out comp, null);
			if (u == null) throw new Exception("could not decode gif");

			if (requiredComponents != null && requiredComponents.Value != ColorComponents.RedGreenBlueAlpha)
				u = Conversion.stbi__convert_format(u, 4, (int)requiredComponents.Value, (uint)w, (uint)h);

			return new ImageResult
			{
				Width = w,
				Height = h,
				SourceComponents = (ColorComponents)comp,
				ColorComponents = requiredComponents != null ? requiredComponents.Value : (ColorComponents)comp,
				BitsPerChannel = 8,
				Data = u
			};
		}

		private static bool InternalTest(Stream stream)
		{
			var sz = 0;
			if (stream.stbi__get8() != 'G' || stream.stbi__get8() != 'I' || stream.stbi__get8() != 'F' ||
				stream.stbi__get8() != '8')
				return false;
			sz = stream.stbi__get8();
			if (sz != '9' && sz != '7')
				return false;
			if (stream.stbi__get8() != 'a')
				return false;
			return true;
		}

		public static bool Test(Stream stream)
		{
			var result = InternalTest(stream);
			stream.Rewind();
			return result;
		}

		public static ImageInfo? Info(Stream stream)
		{
			var decoder = new GifDecoder(stream);

			int comp;
			var r = decoder.stbi__gif_header(out comp, 1);
			stream.Rewind();
			if (r == 0) return null;

			return new ImageInfo
			{
				Width = decoder.w,
				Height = decoder.h,
				ColorComponents = (ColorComponents)comp,
				BitsPerChannel = 8
			};
		}

		public static ImageResult Decode(Stream stream, ColorComponents? requiredComponents = null)
		{
			var decoder = new GifDecoder(stream);
			return decoder.InternalDecode(requiredComponents);
		}
	}
}