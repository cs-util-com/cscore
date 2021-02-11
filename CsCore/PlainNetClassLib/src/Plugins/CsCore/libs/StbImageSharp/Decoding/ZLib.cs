using System;
using StbImageSharp.Utility;

namespace StbImageSharp.Decoding
{
	internal class ZLib
	{
		private class stbi__zhuffman
		{
			public readonly ushort[] fast = new ushort[1 << 9];
			public readonly ushort[] firstcode = new ushort[16];
			public readonly ushort[] firstsymbol = new ushort[16];
			public readonly int[] maxcode = new int[17];
			public readonly byte[] size = new byte[288];
			public readonly ushort[] value = new ushort[288];
		}

		private static readonly int[] stbi__zlength_base =
		{
			3, 4, 5, 6, 7, 8, 9, 10, 11, 13, 15, 17, 19, 23, 27, 31, 35, 43, 51, 59, 67, 83, 99, 115, 131, 163, 195,
			227, 258, 0, 0
		};

		private static readonly int[] stbi__zlength_extra =
			{0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 5, 0, 0, 0};

		private static readonly int[] stbi__zdist_base =
		{
			1, 2, 3, 4, 5, 7, 9, 13, 17, 25, 33, 49, 65, 97, 129, 193, 257, 385, 513, 769, 1025, 1537, 2049, 3073, 4097,
			6145, 8193, 12289, 16385, 24577, 0, 0
		};

		private static readonly int[] stbi__zdist_extra =
			{0, 0, 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7, 8, 8, 9, 9, 10, 10, 11, 11, 12, 12, 13, 13};

		private static readonly byte[] stbi__zdefault_length =
		{
			8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
			8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
			8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
			8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
			9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9,
			9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9,
			9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9,
			9, 9, 9, 9, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 8, 8, 8, 8, 8, 8, 8, 8
		};

		private static readonly byte[] stbi__zdefault_distance =
			{5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5};

		private static readonly byte[] length_dezigzag =
			{16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1, 15};

		private uint code_buffer;
		private int num_bits;
		private readonly stbi__zhuffman z_distance = new stbi__zhuffman();
		private int z_expandable;
		private readonly stbi__zhuffman z_length = new stbi__zhuffman();

		private FakePtr<byte> zbuffer;
		private FakePtr<byte> zbuffer_end;
		private FakePtr<byte> zout;
		private FakePtr<byte> zout_end;
		private byte[] zout_start;

		private byte stbi__zget8()
		{
			if (zbuffer.Offset >= zbuffer_end.Offset)
				return 0;
			return zbuffer.GetAndIncrease();
		}

		private void stbi__fill_bits()
		{
			do
			{
				code_buffer |= (uint)stbi__zget8() << num_bits;
				num_bits += 8;
			} while (num_bits <= 24);
		}

		private uint stbi__zreceive(int n)
		{
			uint k = 0;
			if (num_bits < n)
				stbi__fill_bits();
			k = (uint)(code_buffer & ((1 << n) - 1));
			code_buffer >>= n;
			num_bits -= n;
			return k;
		}

		private int stbi__zhuffman_decode_slowpath(stbi__zhuffman z)
		{
			var b = 0;
			var s = 0;
			var k = 0;
			k = MathExtensions.stbi__bit_reverse((int)code_buffer, 16);
			for (s = 9 + 1; ; ++s)
				if (k < z.maxcode[s])
					break;
			if (s == 16)
				return -1;
			b = (k >> (16 - s)) - z.firstcode[s] + z.firstsymbol[s];
			code_buffer >>= s;
			num_bits -= s;
			return z.value[b];
		}

		private int stbi__zhuffman_decode(stbi__zhuffman z)
		{
			var b = 0;
			var s = 0;
			if (num_bits < 16)
				stbi__fill_bits();
			b = z.fast[code_buffer & ((1 << 9) - 1)];
			if (b != 0)
			{
				s = b >> 9;
				code_buffer >>= s;
				num_bits -= s;
				return b & 511;
			}

			return stbi__zhuffman_decode_slowpath(z);
		}

		private int stbi__zexpand(FakePtr<byte> zout, int n)
		{
			var cur = 0;
			var limit = 0;
			var old_limit = 0;
			this.zout = zout;
			if (z_expandable == 0)
				Decoder.stbi__err("output buffer limit");
			cur = this.zout.Offset;
			limit = old_limit = zout_end.Offset;
			while (cur + n > limit) limit *= 2;

			Array.Resize(ref zout_start, limit);
			this.zout = new FakePtr<byte>(zout_start, cur);
			zout_end = new FakePtr<byte>(zout_start, limit);
			return 1;
		}

		private int stbi__parse_huffman_block()
		{
			var zout = this.zout;
			for (; ; )
			{
				var z = stbi__zhuffman_decode(z_length);
				if (z < 256)
				{
					if (z < 0)
						Decoder.stbi__err("bad huffman code");
					if (zout.Offset >= zout_end.Offset)
					{
						if (stbi__zexpand(zout, 1) == 0)
							return 0;
						zout = this.zout;
					}

					zout.SetAndIncrease((byte)z);
				}
				else
				{
					var len = 0;
					var dist = 0;
					if (z == 256)
					{
						this.zout = zout;
						return 1;
					}

					z -= 257;
					len = stbi__zlength_base[z];
					if (stbi__zlength_extra[z] != 0)
						len += (int)stbi__zreceive(stbi__zlength_extra[z]);
					z = stbi__zhuffman_decode(z_distance);
					if (z < 0)
						Decoder.stbi__err("bad huffman code");
					dist = stbi__zdist_base[z];
					if (stbi__zdist_extra[z] != 0)
						dist += (int)stbi__zreceive(stbi__zdist_extra[z]);
					if (zout.Offset < dist)
						Decoder.stbi__err("bad dist");
					if (zout.Offset + len > zout_end.Offset)
					{
						if (stbi__zexpand(zout, len) == 0)
							return 0;
						zout = this.zout;
					}

					var p = new FakePtr<byte>(zout, -dist);
					if (dist == 1)
					{
						var v = p.Value;
						if (len > 0)
						{
							zout.memset(v, len);
							zout += len;
						}
					}
					else
					{
						if (len != 0)
							do
							{
								zout.SetAndIncrease(p.GetAndIncrease());
							} while (--len != 0);
					}
				}
			}
		}

		private static int stbi__zbuild_huffman(stbi__zhuffman z, FakePtr<byte> sizelist, int num)
		{
			var i = 0;
			var k = 0;
			var code = 0;
			var next_code = new int[16];
			var sizes = new int[17];
			sizes.Clear();
			z.fast.Clear();
			for (i = 0; i < num; ++i) ++sizes[sizelist[i]];
			sizes[0] = 0;
			for (i = 1; i < 16; ++i)
				if (sizes[i] > 1 << i)
					Decoder.stbi__err("bad sizes");
			code = 0;
			for (i = 1; i < 16; ++i)
			{
				next_code[i] = code;
				z.firstcode[i] = (ushort)code;
				z.firstsymbol[i] = (ushort)k;
				code = code + sizes[i];
				if (sizes[i] != 0)
					if (code - 1 >= 1 << i)
						Decoder.stbi__err("bad codelengths");
				z.maxcode[i] = code << (16 - i);
				code <<= 1;
				k += sizes[i];
			}

			z.maxcode[16] = 0x10000;
			for (i = 0; i < num; ++i)
			{
				var s = (int)sizelist[i];
				if (s != 0)
				{
					var c = next_code[s] - z.firstcode[s] + z.firstsymbol[s];
					var fastv = (ushort)((s << 9) | i);
					z.size[c] = (byte)s;
					z.value[c] = (ushort)i;
					if (s <= 9)
					{
						var j = MathExtensions.stbi__bit_reverse(next_code[s], s);
						while (j < 1 << 9)
						{
							z.fast[j] = fastv;
							j += 1 << s;
						}
					}

					++next_code[s];
				}
			}

			return 1;
		}

		private int stbi__compute_huffman_codes()
		{
			var z_codelength = new stbi__zhuffman();
			var lencodes = new byte[286 + 32 + 137];
			var codelength_sizes = new byte[19];
			var i = 0;
			var n = 0;
			var hlit = (int)(stbi__zreceive(5) + 257);
			var hdist = (int)(stbi__zreceive(5) + 1);
			var hclen = (int)(stbi__zreceive(4) + 4);
			var ntot = hlit + hdist;
			codelength_sizes.Clear();
			for (i = 0; i < hclen; ++i)
			{
				var s = (int)stbi__zreceive(3);
				codelength_sizes[length_dezigzag[i]] = (byte)s;
			}

			if (stbi__zbuild_huffman(z_codelength, new FakePtr<byte>(codelength_sizes), 19) == 0)
				return 0;
			n = 0;
			while (n < ntot)
			{
				var c = stbi__zhuffman_decode(z_codelength);
				if (c < 0 || c >= 19)
					Decoder.stbi__err("bad codelengths");
				if (c < 16)
				{
					lencodes[n++] = (byte)c;
				}
				else
				{
					var fill = (byte)0;
					if (c == 16)
					{
						c = (int)(stbi__zreceive(2) + 3);
						if (n == 0)
							Decoder.stbi__err("bad codelengths");
						fill = lencodes[n - 1];
					}
					else if (c == 17)
					{
						c = (int)(stbi__zreceive(3) + 3);
					}
					else
					{
						c = (int)(stbi__zreceive(7) + 11);
					}

					if (ntot - n < c)
						Decoder.stbi__err("bad codelengths");
					lencodes.Set(n, c, fill);
					n += c;
				}
			}

			if (n != ntot)
				Decoder.stbi__err("bad codelengths");
			if (stbi__zbuild_huffman(z_length, new FakePtr<byte>(lencodes), hlit) == 0)
				return 0;
			if (stbi__zbuild_huffman(z_distance, new FakePtr<byte>(lencodes, hlit), hdist) == 0)
				return 0;
			return 1;
		}

		private int stbi__parse_uncompressed_block()
		{
			var header = new byte[4];
			var len = 0;
			var nlen = 0;
			var k = 0;
			if ((num_bits & 7) != 0)
				stbi__zreceive(num_bits & 7);
			k = 0;
			while (num_bits > 0)
			{
				header[k++] = (byte)(code_buffer & 255);
				code_buffer >>= 8;
				num_bits -= 8;
			}

			while (k < 4) header[k++] = stbi__zget8();
			len = header[1] * 256 + header[0];
			nlen = header[3] * 256 + header[2];
			if (nlen != (len ^ 0xffff))
				Decoder.stbi__err("zlib corrupt");
			if (zbuffer.Offset + len > zbuffer_end.Offset)
				Decoder.stbi__err("read past buffer");
			if (zout.Offset + len > zout_end.Offset)
				if (stbi__zexpand(zout, len) == 0)
					return 0;
			for (var i = 0; i < len; i++) zout[i] = zbuffer[i];
			zbuffer += len;
			zout += len;
			return 1;
		}

		private int stbi__parse_zlib_header()
		{
			var cmf = (int)stbi__zget8();
			var cm = cmf & 15;
			var flg = (int)stbi__zget8();
			if ((cmf * 256 + flg) % 31 != 0)
				Decoder.stbi__err("bad zlib header");
			if ((flg & 32) != 0)
				Decoder.stbi__err("no preset dict");
			if (cm != 8)
				Decoder.stbi__err("bad compression");
			return 1;
		}

		private int stbi__parse_zlib(int parse_header)
		{
			var final = 0;
			var type = 0;
			if (parse_header != 0)
				if (stbi__parse_zlib_header() == 0)
					return 0;
			num_bits = 0;
			code_buffer = 0;
			do
			{
				final = (int)stbi__zreceive(1);
				type = (int)stbi__zreceive(2);
				if (type == 0)
				{
					if (stbi__parse_uncompressed_block() == 0)
						return 0;
				}
				else if (type == 3)
				{
					return 0;
				}
				else
				{
					if (type == 1)
					{
						if (stbi__zbuild_huffman(z_length, new FakePtr<byte>(stbi__zdefault_length), 288) == 0)
							return 0;
						if (stbi__zbuild_huffman(z_distance, new FakePtr<byte>(stbi__zdefault_distance), 32) == 0)
							return 0;
					}
					else
					{
						if (stbi__compute_huffman_codes() == 0)
							return 0;
					}

					if (stbi__parse_huffman_block() == 0)
						return 0;
				}
			} while (final == 0);

			return 1;
		}

		private int stbi__do_zlib(byte[] obuf, int olen, int exp, int parse_header)
		{
			zout_start = obuf;
			zout = new FakePtr<byte>(obuf);
			zout_end = new FakePtr<byte>(obuf, olen);
			z_expandable = exp;
			return stbi__parse_zlib(parse_header);
		}

		public static byte[] stbi_zlib_decode_malloc_guesssize_headerflag(byte[] buffer, int len, int initial_size,
			out int outlen, int parse_header)
		{
			outlen = 0;
			var a = new ZLib();
			var p = new byte[initial_size];
			a.zbuffer = new FakePtr<byte>(buffer);
			a.zbuffer_end = new FakePtr<byte>(buffer, +len);
			if (a.stbi__do_zlib(p, initial_size, 1, parse_header) != 0)
			{
				outlen = a.zout.Offset;
				return a.zout_start;
			}

			return null;
		}
	}
}