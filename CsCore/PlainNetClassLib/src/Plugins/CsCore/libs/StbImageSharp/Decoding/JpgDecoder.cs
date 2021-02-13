using System;
using System.IO;
using System.Runtime.InteropServices;
using StbImageSharp.Utility;

namespace StbImageSharp.Decoding
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct img_comp
	{
		public int id;
		public int h, v;
		public int tq;
		public int hd, ha;
		public int dc_pred;

		public int x, y, w2, h2;
		public byte[] raw_data;
		public FakePtr<byte> data;
		public short[] raw_coeff;
		public FakePtr<short> coeff; // progressive only
		public byte[] linebuf;
		public int coeff_w, coeff_h; // number of 8x8 coefficient blocks
	}

	internal delegate void idct_block_kernel(FakePtr<byte> output, int out_stride, FakePtr<short> data);

	internal delegate void YCbCr_to_RGB_kernel(FakePtr<byte> output, FakePtr<byte> y, FakePtr<byte> pcb,
		FakePtr<byte> pcr, int count, int step);

	internal delegate FakePtr<byte> Resampler(FakePtr<byte> a, FakePtr<byte> b, FakePtr<byte> c, int d, int e);

	internal class stbi__resample
	{
		public int hs;
		public FakePtr<byte> line0;
		public FakePtr<byte> line1;
		public Resampler resample;
		public int vs;
		public int w_lores;
		public int ypos;
		public int ystep;
	}

#if !STBSHARP_INTERNAL
	public
#else
	internal
#endif
	class JpgDecoder : Decoder
	{
		private class stbi__huffman
		{
			public readonly ushort[] code = new ushort[256];
			public readonly int[] delta = new int[17];
			public readonly byte[] fast = new byte[1 << 9];
			public readonly uint[] maxcode = new uint[18];
			public readonly byte[] size = new byte[257];
			public readonly byte[] values = new byte[256];
		}

		private const int STBI__ZFAST_BITS = 9;

		private static readonly uint[] stbi__bmask =
			{0, 1, 3, 7, 15, 31, 63, 127, 255, 511, 1023, 2047, 4095, 8191, 16383, 32767, 65535};

		private static readonly int[] stbi__jbias =
			{0, -1, -3, -7, -15, -31, -63, -127, -255, -511, -1023, -2047, -4095, -8191, -16383, -32767};

		private static readonly byte[] stbi__jpeg_dezigzag =
		{
			0, 1, 8, 16, 9, 2, 3, 10, 17, 24, 32, 25, 18, 11, 4, 5, 12, 19, 26, 33, 40, 48, 41, 34, 27, 20, 13, 6, 7,
			14, 21, 28, 35, 42, 49, 56, 57, 50, 43, 36, 29, 22, 15, 23, 30, 37, 44, 51, 58, 59, 52, 45, 38, 31, 39, 46,
			53, 60, 61, 54, 47, 55, 62, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63
		};

		private readonly stbi__huffman[] huff_dc = new stbi__huffman[4];
		private readonly stbi__huffman[] huff_ac = new stbi__huffman[4];
		private readonly ushort[][] dequant;

		private readonly short[][] fast_ac;

		// sizes for components, interleaved MCUs
		private int img_h_max, img_v_max;
		private int img_mcu_x, img_mcu_y;
		private int img_mcu_w, img_mcu_h;

		// definition of jpeg image component
		private readonly img_comp[] img_comp = new img_comp[4];

		private uint code_buffer; // jpeg entropy-coded buffer
		private int code_bits; // number of valid bits
		private byte marker; // marker seen while filling entropy buffer
		private int nomore; // flag if we saw a marker so must stop

		private int progressive;
		private int spec_start;
		private int spec_end;
		private int succ_high;
		private int succ_low;
		private int eob_run;
		private int jfif;
		private int app14_color_transform; // Adobe APP14 tag
		private int rgb;

		private int scan_n;
		private readonly int[] order = new int[4];
		private int restart_interval, todo;

		// kernels
		private idct_block_kernel idct_block_kernel;
		private YCbCr_to_RGB_kernel YCbCr_to_RGB_kernel;
		private Resampler resample_row_hv_2_kernel;

		private JpgDecoder(Stream stream) : base(stream)
		{
			for (var i = 0; i < 4; ++i)
			{
				huff_ac[i] = new stbi__huffman();
				huff_dc[i] = new stbi__huffman();
			}

			for (var i = 0; i < img_comp.Length; ++i) img_comp[i] = new img_comp();

			fast_ac = new short[4][];
			for (var i = 0; i < fast_ac.Length; ++i) fast_ac[i] = new short[1 << STBI__ZFAST_BITS];

			dequant = new ushort[4][];
			for (var i = 0; i < dequant.Length; ++i) dequant[i] = new ushort[64];
		}

		private static int stbi__build_huffman(stbi__huffman h, int[] count)
		{
			var i = 0;
			var j = 0;
			var k = 0;
			uint code = 0;
			for (i = 0; i < 16; ++i)
				for (j = 0; j < count[i]; ++j)
					h.size[k++] = (byte)(i + 1);
			h.size[k] = 0;
			code = 0;
			k = 0;
			for (j = 1; j <= 16; ++j)
			{
				h.delta[j] = (int)(k - code);
				if (h.size[k] == j)
				{
					while (h.size[k] == j) h.code[k++] = (ushort)code++;
					if (code - 1 >= 1u << j)
						stbi__err("bad code lengths");
				}

				h.maxcode[j] = code << (16 - j);
				code <<= 1;
			}

			h.maxcode[j] = 0xffffffff;
			h.fast.Set((byte)255);
			for (i = 0; i < k; ++i)
			{
				var s = (int)h.size[i];
				if (s <= 9)
				{
					var c = h.code[i] << (9 - s);
					var m = 1 << (9 - s);
					for (j = 0; j < m; ++j) h.fast[c + j] = (byte)i;
				}
			}

			return 1;
		}

		private static void stbi__build_fast_ac(short[] fast_ac, stbi__huffman h)
		{
			var i = 0;
			for (i = 0; i < 1 << 9; ++i)
			{
				var fast = h.fast[i];
				fast_ac[i] = 0;
				if (fast < 255)
				{
					var rs = (int)h.values[fast];
					var run = (rs >> 4) & 15;
					var magbits = rs & 15;
					var len = (int)h.size[fast];
					if (magbits != 0 && len + magbits <= 9)
					{
						var k = ((i << len) & ((1 << 9) - 1)) >> (9 - magbits);
						var m = 1 << (magbits - 1);
						if (k < m)
							k += (int)((~0U << magbits) + 1);
						if (k >= -128 && k <= 127)
							fast_ac[i] = (short)(k * 256 + run * 16 + len + magbits);
					}
				}
			}
		}

		private void stbi__grow_buffer_unsafe()
		{
			do
			{
				var b = (uint)(nomore != 0 ? 0 : stbi__get8());
				if (b == 0xff)
				{
					var c = (int)stbi__get8();
					while (c == 0xff) c = stbi__get8();
					if (c != 0)
					{
						marker = (byte)c;
						nomore = 1;
						return;
					}
				}

				code_buffer |= b << (24 - code_bits);
				code_bits += 8;
			} while (code_bits <= 24);
		}

		private int stbi__jpeg_huff_decode(stbi__huffman h)
		{
			uint temp = 0;
			var c = 0;
			var k = 0;
			if (code_bits < 16)
				stbi__grow_buffer_unsafe();
			c = (int)((code_buffer >> (32 - 9)) & ((1 << 9) - 1));
			k = h.fast[c];
			if (k < 255)
			{
				var s = (int)h.size[k];
				if (s > code_bits)
					return -1;
				code_buffer <<= s;
				code_bits -= s;
				return h.values[k];
			}

			temp = code_buffer >> 16;
			for (k = 9 + 1; ; ++k)
				if (temp < h.maxcode[k])
					break;
			if (k == 17)
			{
				code_bits -= 16;
				return -1;
			}

			if (k > code_bits)
				return -1;
			c = (int)(((code_buffer >> (32 - k)) & stbi__bmask[k]) + h.delta[k]);
			code_bits -= k;
			code_buffer <<= k;
			return h.values[c];
		}

		private int stbi__extend_receive(int n)
		{
			uint k = 0;
			var sgn = 0;
			if (code_bits < n)
				stbi__grow_buffer_unsafe();
			sgn = (int)code_buffer >> 31;
			k = MathExtensions._lrotl(code_buffer, n);
			code_buffer = k & ~stbi__bmask[n];
			k &= stbi__bmask[n];
			code_bits -= n;
			return (int)(k + (stbi__jbias[n] & ~sgn));
		}

		private int stbi__jpeg_get_bits(int n)
		{
			uint k = 0;
			if (code_bits < n)
				stbi__grow_buffer_unsafe();
			k = MathExtensions._lrotl(code_buffer, n);
			code_buffer = k & ~stbi__bmask[n];
			k &= stbi__bmask[n];
			code_bits -= n;
			return (int)k;
		}

		private int stbi__jpeg_get_bit()
		{
			uint k = 0;
			if (code_bits < 1)
				stbi__grow_buffer_unsafe();
			k = code_buffer;
			code_buffer <<= 1;
			--code_bits;
			return (int)(k & 0x80000000);
		}

		private int stbi__jpeg_decode_block(short[] data, stbi__huffman hdc, stbi__huffman hac, short[] fac, int b,
			ushort[] dequant)
		{
			var diff = 0;
			var dc = 0;
			var k = 0;
			var t = 0;
			if (code_bits < 16)
				stbi__grow_buffer_unsafe();
			t = stbi__jpeg_huff_decode(hdc);
			if (t < 0)
				stbi__err("bad huffman code");

			Array.Clear(data, 0, data.Length);
			diff = t != 0 ? stbi__extend_receive(t) : 0;
			dc = img_comp[b].dc_pred + diff;
			img_comp[b].dc_pred = dc;
			data[0] = (short)(dc * dequant[0]);
			k = 1;
			do
			{
				uint zig = 0;
				var c = 0;
				var r = 0;
				var s = 0;
				if (code_bits < 16)
					stbi__grow_buffer_unsafe();
				c = (int)((code_buffer >> (32 - 9)) & ((1 << 9) - 1));
				r = fac[c];
				if (r != 0)
				{
					k += (r >> 4) & 15;
					s = r & 15;
					code_buffer <<= s;
					code_bits -= s;
					zig = stbi__jpeg_dezigzag[k++];
					data[zig] = (short)((r >> 8) * dequant[zig]);
				}
				else
				{
					var rs = stbi__jpeg_huff_decode(hac);
					if (rs < 0)
						stbi__err("bad huffman code");
					s = rs & 15;
					r = rs >> 4;
					if (s == 0)
					{
						if (rs != 0xf0)
							break;
						k += 16;
					}
					else
					{
						k += r;
						zig = stbi__jpeg_dezigzag[k++];
						data[zig] = (short)(stbi__extend_receive(s) * dequant[zig]);
					}
				}
			} while (k < 64);

			return 1;
		}

		private int stbi__jpeg_decode_block_prog_dc(FakePtr<short> data, stbi__huffman hdc, int b)
		{
			var diff = 0;
			var dc = 0;
			var t = 0;
			if (spec_end != 0)
				stbi__err("can't merge dc and ac");
			if (code_bits < 16)
				stbi__grow_buffer_unsafe();
			if (succ_high == 0)
			{
				data.Clear(64);
				t = stbi__jpeg_huff_decode(hdc);
				diff = t != 0 ? stbi__extend_receive(t) : 0;
				dc = img_comp[b].dc_pred + diff;
				img_comp[b].dc_pred = dc;
				data[0] = (short)(dc << succ_low);
			}
			else
			{
				if (stbi__jpeg_get_bit() != 0)
					data[0] += (short)(1 << succ_low);
			}

			return 1;
		}

		private int stbi__jpeg_decode_block_prog_ac(FakePtr<short> data, stbi__huffman hac, short[] fac)
		{
			var k = 0;
			if (spec_start == 0)
				stbi__err("can't merge dc and ac");
			if (succ_high == 0)
			{
				var shift = succ_low;
				if (eob_run != 0)
				{
					--eob_run;
					return 1;
				}

				k = spec_start;
				do
				{
					uint zig = 0;
					var c = 0;
					var r = 0;
					var s = 0;
					if (code_bits < 16)
						stbi__grow_buffer_unsafe();
					c = (int)((code_buffer >> (32 - 9)) & ((1 << 9) - 1));
					r = fac[c];
					if (r != 0)
					{
						k += (r >> 4) & 15;
						s = r & 15;
						code_buffer <<= s;
						code_bits -= s;
						zig = stbi__jpeg_dezigzag[k++];
						data[zig] = (short)((r >> 8) << shift);
					}
					else
					{
						var rs = stbi__jpeg_huff_decode(hac);
						if (rs < 0)
							stbi__err("bad huffman code");
						s = rs & 15;
						r = rs >> 4;
						if (s == 0)
						{
							if (r < 15)
							{
								eob_run = 1 << r;
								if (r != 0)
									eob_run += stbi__jpeg_get_bits(r);
								--eob_run;
								break;
							}

							k += 16;
						}
						else
						{
							k += r;
							zig = stbi__jpeg_dezigzag[k++];
							data[zig] = (short)(stbi__extend_receive(s) << shift);
						}
					}
				} while (k <= spec_end);
			}
			else
			{
				var bit = (short)(1 << succ_low);
				if (eob_run != 0)
				{
					--eob_run;
					for (k = spec_start; k <= spec_end; ++k)
					{
						var idx = stbi__jpeg_dezigzag[k];
						var value = data[idx];
						if (value != 0)
							if (stbi__jpeg_get_bit() != 0)
								if ((value & bit) == 0)
								{
									if (value > 0)
										data[idx] += bit;
									else
										data[idx] -= bit;
								}
					}
				}
				else
				{
					k = spec_start;
					do
					{
						var r = 0;
						var s = 0;
						var rs = stbi__jpeg_huff_decode(hac);
						if (rs < 0)
							stbi__err("bad huffman code");
						s = rs & 15;
						r = rs >> 4;
						if (s == 0)
						{
							if (r < 15)
							{
								eob_run = (1 << r) - 1;
								if (r != 0)
									eob_run += stbi__jpeg_get_bits(r);
								r = 64;
							}
						}
						else
						{
							if (s != 1)
								stbi__err("bad huffman code");
							if (stbi__jpeg_get_bit() != 0)
								s = bit;
							else
								s = -bit;
						}

						while (k <= spec_end)
						{
							var idx = stbi__jpeg_dezigzag[k++];
							var value = data[idx];
							if (value != 0)
							{
								if (stbi__jpeg_get_bit() != 0)
									if ((value & bit) == 0)
									{
										if (value > 0)
											data[idx] += bit;
										else
											data[idx] -= bit;
									}
							}
							else
							{
								if (r == 0)
								{
									data[idx] = (short)s;
									break;
								}

								--r;
							}
						}
					} while (k <= spec_end);
				}
			}

			return 1;
		}

		private static byte stbi__clamp(int x)
		{
			if (x < 0)
				return 0;
			if (x > 255)
				return 255;

			return (byte)x;
		}

		private static void stbi__idct_block(FakePtr<byte> _out_, int out_stride, FakePtr<short> data)
		{
			var i = 0;
			var val = new int[64];
			var v = new FakePtr<int>(val);
			FakePtr<byte> o;
			var d = data;
			for (i = 0; i < 8; ++i, ++d, ++v)
				if (d[8] == 0 && d[16] == 0 && d[24] == 0 && d[32] == 0 && d[40] == 0 && d[48] == 0 && d[56] == 0)
				{
					var dcterm = d[0] * 4;
					v[0] = v[8] = v[16] = v[24] = v[32] = v[40] = v[48] = v[56] = dcterm;
				}
				else
				{
					var t0 = 0;
					var t1 = 0;
					var t2 = 0;
					var t3 = 0;
					var p1 = 0;
					var p2 = 0;
					var p3 = 0;
					var p4 = 0;
					var p5 = 0;
					var x0 = 0;
					var x1 = 0;
					var x2 = 0;
					var x3 = 0;
					p2 = d[16];
					p3 = d[48];
					p1 = (p2 + p3) * (int)(0.5411961f * 4096 + 0.5);
					t2 = p1 + p3 * (int)(-1.847759065f * 4096 + 0.5);
					t3 = p1 + p2 * (int)(0.765366865f * 4096 + 0.5);
					p2 = d[0];
					p3 = d[32];
					t0 = (p2 + p3) * 4096;
					t1 = (p2 - p3) * 4096;
					x0 = t0 + t3;
					x3 = t0 - t3;
					x1 = t1 + t2;
					x2 = t1 - t2;
					t0 = d[56];
					t1 = d[40];
					t2 = d[24];
					t3 = d[8];
					p3 = t0 + t2;
					p4 = t1 + t3;
					p1 = t0 + t3;
					p2 = t1 + t2;
					p5 = (p3 + p4) * (int)(1.175875602f * 4096 + 0.5);
					t0 = t0 * (int)(0.298631336f * 4096 + 0.5);
					t1 = t1 * (int)(2.053119869f * 4096 + 0.5);
					t2 = t2 * (int)(3.072711026f * 4096 + 0.5);
					t3 = t3 * (int)(1.501321110f * 4096 + 0.5);
					p1 = p5 + p1 * (int)(-0.899976223f * 4096 + 0.5);
					p2 = p5 + p2 * (int)(-2.562915447f * 4096 + 0.5);
					p3 = p3 * (int)(-1.961570560f * 4096 + 0.5);
					p4 = p4 * (int)(-0.390180644f * 4096 + 0.5);
					t3 += p1 + p4;
					t2 += p2 + p3;
					t1 += p2 + p4;
					t0 += p1 + p3;
					x0 += 512;
					x1 += 512;
					x2 += 512;
					x3 += 512;
					v[0] = (x0 + t3) >> 10;
					v[56] = (x0 - t3) >> 10;
					v[8] = (x1 + t2) >> 10;
					v[48] = (x1 - t2) >> 10;
					v[16] = (x2 + t1) >> 10;
					v[40] = (x2 - t1) >> 10;
					v[24] = (x3 + t0) >> 10;
					v[32] = (x3 - t0) >> 10;
				}

			for (i = 0, v = new FakePtr<int>(val), o = _out_; i < 8; ++i, v += 8, o += out_stride)
			{
				var t0 = 0;
				var t1 = 0;
				var t2 = 0;
				var t3 = 0;
				var p1 = 0;
				var p2 = 0;
				var p3 = 0;
				var p4 = 0;
				var p5 = 0;
				var x0 = 0;
				var x1 = 0;
				var x2 = 0;
				var x3 = 0;
				p2 = v[2];
				p3 = v[6];
				p1 = (p2 + p3) * (int)(0.5411961f * 4096 + 0.5);
				t2 = p1 + p3 * (int)(-1.847759065f * 4096 + 0.5);
				t3 = p1 + p2 * (int)(0.765366865f * 4096 + 0.5);
				p2 = v[0];
				p3 = v[4];
				t0 = (p2 + p3) * 4096;
				t1 = (p2 - p3) * 4096;
				x0 = t0 + t3;
				x3 = t0 - t3;
				x1 = t1 + t2;
				x2 = t1 - t2;
				t0 = v[7];
				t1 = v[5];
				t2 = v[3];
				t3 = v[1];
				p3 = t0 + t2;
				p4 = t1 + t3;
				p1 = t0 + t3;
				p2 = t1 + t2;
				p5 = (p3 + p4) * (int)(1.175875602f * 4096 + 0.5);
				t0 = t0 * (int)(0.298631336f * 4096 + 0.5);
				t1 = t1 * (int)(2.053119869f * 4096 + 0.5);
				t2 = t2 * (int)(3.072711026f * 4096 + 0.5);
				t3 = t3 * (int)(1.501321110f * 4096 + 0.5);
				p1 = p5 + p1 * (int)(-0.899976223f * 4096 + 0.5);
				p2 = p5 + p2 * (int)(-2.562915447f * 4096 + 0.5);
				p3 = p3 * (int)(-1.961570560f * 4096 + 0.5);
				p4 = p4 * (int)(-0.390180644f * 4096 + 0.5);
				t3 += p1 + p4;
				t2 += p2 + p3;
				t1 += p2 + p4;
				t0 += p1 + p3;
				x0 += 65536 + (128 << 17);
				x1 += 65536 + (128 << 17);
				x2 += 65536 + (128 << 17);
				x3 += 65536 + (128 << 17);
				o[0] = stbi__clamp((x0 + t3) >> 17);
				o[7] = stbi__clamp((x0 - t3) >> 17);
				o[1] = stbi__clamp((x1 + t2) >> 17);
				o[6] = stbi__clamp((x1 - t2) >> 17);
				o[2] = stbi__clamp((x2 + t1) >> 17);
				o[5] = stbi__clamp((x2 - t1) >> 17);
				o[3] = stbi__clamp((x3 + t0) >> 17);
				o[4] = stbi__clamp((x3 - t0) >> 17);
			}
		}

		private byte stbi__get_marker()
		{
			byte x = 0;
			if (marker != 0xff)
			{
				x = marker;
				marker = 0xff;
				return x;
			}

			x = stbi__get8();
			if (x != 0xff)
				return 0xff;
			while (x == 0xff) x = stbi__get8();
			return x;
		}

		private void stbi__jpeg_reset()
		{
			code_bits = 0;
			code_buffer = 0;
			nomore = 0;
			img_comp[0].dc_pred = img_comp[1].dc_pred = img_comp[2].dc_pred = img_comp[3].dc_pred = 0;
			marker = 0xff;
			todo = restart_interval != 0 ? restart_interval : 0x7fffffff;
			eob_run = 0;
		}

		private int stbi__parse_entropy_coded_data()
		{
			stbi__jpeg_reset();
			if (progressive == 0)
			{
				if (scan_n == 1)
				{
					var i = 0;
					var j = 0;
					var data = new short[64];
					var n = order[0];
					var w = (img_comp[n].x + 7) >> 3;
					var h = (img_comp[n].y + 7) >> 3;
					for (j = 0; j < h; ++j)
						for (i = 0; i < w; ++i)
						{
							var ha = img_comp[n].ha;
							if (stbi__jpeg_decode_block(data, huff_dc[img_comp[n].hd], huff_ac[ha], fast_ac[ha], n,
									dequant[img_comp[n].tq]) == 0)
								return 0;
							idct_block_kernel(new FakePtr<byte>(img_comp[n].data, img_comp[n].w2 * j * 8 + i * 8),
								img_comp[n].w2,
								new FakePtr<short>(data));
							if (--todo <= 0)
							{
								if (code_bits < 24)
									stbi__grow_buffer_unsafe();
								if (!(marker >= 0xd0 && marker <= 0xd7))
									return 1;
								stbi__jpeg_reset();
							}
						}

					return 1;
				}
				else
				{
					var i = 0;
					var j = 0;
					var k = 0;
					var x = 0;
					var y = 0;
					var data = new short[64];
					for (j = 0; j < img_mcu_y; ++j)
						for (i = 0; i < img_mcu_x; ++i)
						{
							for (k = 0; k < scan_n; ++k)
							{
								var n = order[k];
								for (y = 0; y < img_comp[n].v; ++y)
									for (x = 0; x < img_comp[n].h; ++x)
									{
										var x2 = (i * img_comp[n].h + x) * 8;
										var y2 = (j * img_comp[n].v + y) * 8;
										var ha = img_comp[n].ha;
										if (stbi__jpeg_decode_block(data, huff_dc[img_comp[n].hd], huff_ac[ha], fast_ac[ha], n,
												dequant[img_comp[n].tq]) == 0)
											return 0;
										idct_block_kernel(new FakePtr<byte>(img_comp[n].data,
												img_comp[n].w2 * y2 + x2), img_comp[n].w2,
											new FakePtr<short>(data));
									}
							}

							if (--todo <= 0)
							{
								if (code_bits < 24)
									stbi__grow_buffer_unsafe();
								if (!(marker >= 0xd0 && marker <= 0xd7))
									return 1;
								stbi__jpeg_reset();
							}
						}

					return 1;
				}
			}

			if (scan_n == 1)
			{
				var i = 0;
				var j = 0;
				var n = order[0];
				var w = (img_comp[n].x + 7) >> 3;
				var h = (img_comp[n].y + 7) >> 3;
				for (j = 0; j < h; ++j)
					for (i = 0; i < w; ++i)
					{
						var data = new FakePtr<short>(img_comp[n].coeff, 64 * (i + j * img_comp[n].coeff_w));
						if (spec_start == 0)
						{
							if (stbi__jpeg_decode_block_prog_dc(data, huff_dc[img_comp[n].hd], n) == 0)
								return 0;
						}
						else
						{
							var ha = img_comp[n].ha;
							if (stbi__jpeg_decode_block_prog_ac(data, huff_ac[ha], fast_ac[ha]) == 0)
								return 0;
						}

						if (--todo <= 0)
						{
							if (code_bits < 24)
								stbi__grow_buffer_unsafe();
							if (!(marker >= 0xd0 && marker <= 0xd7))
								return 1;
							stbi__jpeg_reset();
						}
					}

				return 1;
			}
			else
			{
				var i = 0;
				var j = 0;
				var k = 0;
				var x = 0;
				var y = 0;
				for (j = 0; j < img_mcu_y; ++j)
					for (i = 0; i < img_mcu_x; ++i)
					{
						for (k = 0; k < scan_n; ++k)
						{
							var n = order[k];
							for (y = 0; y < img_comp[n].v; ++y)
								for (x = 0; x < img_comp[n].h; ++x)
								{
									var x2 = i * img_comp[n].h + x;
									var y2 = j * img_comp[n].v + y;
									var data = new FakePtr<short>(img_comp[n].coeff, 64 * (x2 + y2 * img_comp[n].coeff_w));
									if (stbi__jpeg_decode_block_prog_dc(data, huff_dc[img_comp[n].hd], n) == 0)
										return 0;
								}
						}

						if (--todo <= 0)
						{
							if (code_bits < 24)
								stbi__grow_buffer_unsafe();
							if (!(marker >= 0xd0 && marker <= 0xd7))
								return 1;
							stbi__jpeg_reset();
						}
					}

				return 1;
			}
		}

		private static void stbi__jpeg_dequantize(FakePtr<short> data, ushort[] dequant)
		{
			var i = 0;
			for (i = 0; i < 64; ++i) data[i] *= (short)dequant[i];
		}

		private void stbi__jpeg_finish()
		{
			if (progressive != 0)
			{
				var i = 0;
				var j = 0;
				var n = 0;
				for (n = 0; n < img_n; ++n)
				{
					var w = (img_comp[n].x + 7) >> 3;
					var h = (img_comp[n].y + 7) >> 3;
					for (j = 0; j < h; ++j)
						for (i = 0; i < w; ++i)
						{
							var data = new FakePtr<short>(img_comp[n].coeff, 64 * (i + j * img_comp[n].coeff_w));
							stbi__jpeg_dequantize(data, dequant[img_comp[n].tq]);
							idct_block_kernel(new FakePtr<byte>(img_comp[n].data, img_comp[n].w2 * j * 8 + i * 8),
								img_comp[n].w2,
								data);
						}
				}
			}
		}

		private int stbi__process_marker(int m)
		{
			var L = 0;
			switch (m)
			{
				case 0xff:
					stbi__err("expected marker");
					break;
				case 0xDD:
					if (stbi__get16be() != 4)
						stbi__err("bad DRI len");
					restart_interval = stbi__get16be();
					return 1;
				case 0xDB:
					L = stbi__get16be() - 2;
					while (L > 0)
					{
						var q = (int)stbi__get8();
						var p = q >> 4;
						var sixteen = p != 0 ? 1 : 0;
						var t = q & 15;
						var i = 0;
						if (p != 0 && p != 1)
							stbi__err("bad DQT type");
						if (t > 3)
							stbi__err("bad DQT table");
						for (i = 0; i < 64; ++i)
							dequant[t][stbi__jpeg_dezigzag[i]] =
								(ushort)(sixteen != 0 ? stbi__get16be() : stbi__get8());
						L -= sixteen != 0 ? 129 : 65;
					}

					return L == 0 ? 1 : 0;
				case 0xC4:
					L = stbi__get16be() - 2;
					while (L > 0)
					{
						byte[] v;
						var sizes = new int[16];
						var i = 0;
						var n = 0;
						var q = (int)stbi__get8();
						var tc = q >> 4;
						var th = q & 15;
						if (tc > 1 || th > 3)
							stbi__err("bad DHT header");
						for (i = 0; i < 16; ++i)
						{
							sizes[i] = stbi__get8();
							n += sizes[i];
						}

						L -= 17;
						if (tc == 0)
						{
							if (stbi__build_huffman(huff_dc[th], sizes) == 0)
								return 0;
							v = huff_dc[th].values;
						}
						else
						{
							if (stbi__build_huffman(huff_ac[th], sizes) == 0)
								return 0;
							v = huff_ac[th].values;
						}

						for (i = 0; i < n; ++i) v[i] = stbi__get8();
						if (tc != 0)
							stbi__build_fast_ac(fast_ac[th], huff_ac[th]);
						L -= n;
					}

					return L == 0 ? 1 : 0;
			}

			if (m >= 0xE0 && m <= 0xEF || m == 0xFE)
			{
				L = stbi__get16be();
				if (L < 2)
				{
					if (m == 0xFE)
						stbi__err("bad COM len");
					else
						stbi__err("bad APP len");
				}

				L -= 2;
				if (m == 0xE0 && L >= 5)
				{
					var tag = new byte[5];
					tag[0] = (byte)'J';
					tag[1] = (byte)'F';
					tag[2] = (byte)'I';
					tag[3] = (byte)'F';
					tag[4] = (byte)'\0';
					var ok = 1;
					var i = 0;
					for (i = 0; i < 5; ++i)
						if (stbi__get8() != tag[i])
							ok = 0;
					L -= 5;
					if (ok != 0)
						jfif = 1;
				}
				else if (m == 0xEE && L >= 12)
				{
					var tag = new byte[6];
					tag[0] = (byte)'A';
					tag[1] = (byte)'d';
					tag[2] = (byte)'o';
					tag[3] = (byte)'b';
					tag[4] = (byte)'e';
					tag[5] = (byte)'\0';
					var ok = 1;
					var i = 0;
					for (i = 0; i < 6; ++i)
						if (stbi__get8() != tag[i])
							ok = 0;
					L -= 6;
					if (ok != 0)
					{
						stbi__get8();
						stbi__get16be();
						stbi__get16be();
						app14_color_transform = stbi__get8();
						L -= 6;
					}
				}

				stbi__skip(L);
				return 1;
			}

			return 0;
		}

		private int stbi__process_scan_header()
		{
			var i = 0;
			var Ls = stbi__get16be();
			scan_n = stbi__get8();
			if (scan_n < 1 || scan_n > 4 || scan_n > img_n)
				stbi__err("bad SOS component count");
			if (Ls != 6 + 2 * scan_n)
				stbi__err("bad SOS len");
			for (i = 0; i < scan_n; ++i)
			{
				var id = (int)stbi__get8();
				var which = 0;
				var q = (int)stbi__get8();
				for (which = 0; which < img_n; ++which)
					if (img_comp[which].id == id)
						break;
				if (which == img_n)
					return 0;
				img_comp[which].hd = q >> 4;
				if (img_comp[which].hd > 3)
					stbi__err("bad DC huff");
				img_comp[which].ha = q & 15;
				if (img_comp[which].ha > 3)
					stbi__err("bad AC huff");
				order[i] = which;
			}

			{
				var aa = 0;
				spec_start = stbi__get8();
				spec_end = stbi__get8();
				aa = stbi__get8();
				succ_high = aa >> 4;
				succ_low = aa & 15;
				if (progressive != 0)
				{
					if (spec_start > 63 || spec_end > 63 || spec_start > spec_end || succ_high > 13 || succ_low > 13)
						stbi__err("bad SOS");
				}
				else
				{
					if (spec_start != 0)
						stbi__err("bad SOS");
					if (succ_high != 0 || succ_low != 0)
						stbi__err("bad SOS");
					spec_end = 63;
				}
			}

			return 1;
		}

		private int stbi__free_jpeg_components(int ncomp, int why)
		{
			var i = 0;
			for (i = 0; i < ncomp; ++i)
			{
				if (img_comp[i].raw_data != null)
				{
					img_comp[i].raw_data = null;
					img_comp[i].data = FakePtr<byte>.Null;
				}

				if (img_comp[i].raw_coeff != null)
				{
					img_comp[i].raw_coeff = null;
					img_comp[i].coeff = FakePtr<short>.Null;
				}

				if (img_comp[i].linebuf != null) img_comp[i].linebuf = null;
			}

			return why;
		}

		private int stbi__process_frame_header(int scan)
		{
			var Lf = 0;
			var p = 0;
			var i = 0;
			var q = 0;
			var h_max = 1;
			var v_max = 1;
			var c = 0;
			Lf = stbi__get16be();
			if (Lf < 11)
				stbi__err("bad SOF len");
			p = stbi__get8();
			if (p != 8)
				stbi__err("only 8-bit");
			img_y = stbi__get16be();
			if (img_y == 0)
				stbi__err("no header height");
			img_x = stbi__get16be();
			if (img_x == 0)
				stbi__err("0 width");
			c = stbi__get8();
			if (c != 3 && c != 1 && c != 4)
				stbi__err("bad component count");
			img_n = c;
			for (i = 0; i < c; ++i)
			{
				img_comp[i].data = FakePtr<byte>.Null;
				img_comp[i].linebuf = null;
			}

			if (Lf != 8 + 3 * img_n)
				stbi__err("bad SOF len");
			rgb = 0;
			for (i = 0; i < img_n; ++i)
			{
				var rgb = new byte[3];
				rgb[0] = (byte)'R';
				rgb[1] = (byte)'G';
				rgb[2] = (byte)'B';
				img_comp[i].id = stbi__get8();
				if (img_n == 3 && img_comp[i].id == rgb[i])
					++this.rgb;
				q = stbi__get8();
				img_comp[i].h = q >> 4;
				if (img_comp[i].h == 0 || img_comp[i].h > 4)
					stbi__err("bad H");
				img_comp[i].v = q & 15;
				if (img_comp[i].v == 0 || img_comp[i].v > 4)
					stbi__err("bad V");
				img_comp[i].tq = stbi__get8();
				if (img_comp[i].tq > 3)
					stbi__err("bad TQ");
			}

			if (scan != STBI__SCAN_load)
				return 1;
			for (i = 0; i < img_n; ++i)
			{
				if (img_comp[i].h > h_max)
					h_max = img_comp[i].h;
				if (img_comp[i].v > v_max)
					v_max = img_comp[i].v;
			}

			img_h_max = h_max;
			img_v_max = v_max;
			img_mcu_w = h_max * 8;
			img_mcu_h = v_max * 8;
			img_mcu_x = (img_x + img_mcu_w - 1) / img_mcu_w;
			img_mcu_y = (img_y + img_mcu_h - 1) / img_mcu_h;
			for (i = 0; i < img_n; ++i)
			{
				img_comp[i].x = (img_x * img_comp[i].h + h_max - 1) / h_max;
				img_comp[i].y = (img_y * img_comp[i].v + v_max - 1) / v_max;
				img_comp[i].w2 = img_mcu_x * img_comp[i].h * 8;
				img_comp[i].h2 = img_mcu_y * img_comp[i].v * 8;
				img_comp[i].coeff = FakePtr<short>.Null;
				img_comp[i].raw_coeff = null;
				img_comp[i].linebuf = null;
				img_comp[i].raw_data = new byte[img_comp[i].w2 * img_comp[i].h2 + 15];
				img_comp[i].data = new FakePtr<byte>(img_comp[i].raw_data);
				if (progressive != 0)
				{
					img_comp[i].coeff_w = img_comp[i].w2 / 8;
					img_comp[i].coeff_h = img_comp[i].h2 / 8;
					img_comp[i].raw_coeff = new short[img_comp[i].w2 * img_comp[i].h2 + 15];
					img_comp[i].coeff = new FakePtr<short>(img_comp[i].raw_coeff);
				}
			}

			return 1;
		}

		private bool stbi__decode_jpeg_header(int scan)
		{
			var m = 0;
			jfif = 0;
			app14_color_transform = -1;
			marker = 0xff;
			m = stbi__get_marker();
			if (!(m == 0xd8))
			{
				if (scan == STBI__SCAN_type)
					return false;
				stbi__err("no SOI");
			}

			if (scan == STBI__SCAN_type)
				return true;
			m = stbi__get_marker();
			while (!(m == 0xc0 || m == 0xc1 || m == 0xc2))
			{
				if (stbi__process_marker(m) == 0)
					return false;
				m = stbi__get_marker();
				while (m == 0xff)
				{
					if (stbi__at_eof())
						stbi__err("no SOF");
					m = stbi__get_marker();
				}
			}

			progressive = m == 0xc2 ? 1 : 0;
			if (stbi__process_frame_header(scan) == 0)
				return false;
			return true;
		}

		private int stbi__decode_jpeg_image()
		{
			var m = 0;
			for (m = 0; m < 4; m++)
			{
				img_comp[m].raw_data = null;
				img_comp[m].raw_coeff = null;
			}

			restart_interval = 0;
			if (!stbi__decode_jpeg_header(STBI__SCAN_load))
				return 0;
			m = stbi__get_marker();
			while (!(m == 0xd9))
			{
				if (m == 0xda)
				{
					if (stbi__process_scan_header() == 0)
						return 0;
					if (stbi__parse_entropy_coded_data() == 0)
						return 0;
					if (marker == 0xff)
						while (!stbi__at_eof())
						{
							var x = (int)stbi__get8();
							if (x == 255)
							{
								marker = stbi__get8();
								break;
							}
						}
				}
				else if (m == 0xdc)
				{
					var Ld = stbi__get16be();
					var NL = (uint)stbi__get16be();
					if (Ld != 4)
						stbi__err("bad DNL len");
					if (NL != img_y)
						stbi__err("bad DNL height");
				}
				else
				{
					if (stbi__process_marker(m) == 0)
						return 0;
				}

				m = stbi__get_marker();
			}

			if (progressive != 0)
				stbi__jpeg_finish();
			return 1;
		}

		private static FakePtr<byte> resample_row_1(FakePtr<byte> _out_, FakePtr<byte> in_near, FakePtr<byte> in_far,
			int w, int hs)
		{
			return in_near;
		}

		private static FakePtr<byte> stbi__resample_row_v_2(FakePtr<byte> _out_, FakePtr<byte> in_near,
			FakePtr<byte> in_far, int w, int hs)
		{
			var i = 0;
			for (i = 0; i < w; ++i) _out_[i] = (byte)((3 * in_near[i] + in_far[i] + 2) >> 2);
			return _out_;
		}

		private static FakePtr<byte> stbi__resample_row_h_2(FakePtr<byte> _out_, FakePtr<byte> in_near,
			FakePtr<byte> in_far, int w, int hs)
		{
			var i = 0;
			var input = in_near;
			if (w == 1)
			{
				_out_[0] = _out_[1] = input[0];
				return _out_;
			}

			_out_[0] = input[0];
			_out_[1] = (byte)((input[0] * 3 + input[1] + 2) >> 2);
			for (i = 1; i < w - 1; ++i)
			{
				var n = 3 * input[i] + 2;
				_out_[i * 2 + 0] = (byte)((n + input[i - 1]) >> 2);
				_out_[i * 2 + 1] = (byte)((n + input[i + 1]) >> 2);
			}

			_out_[i * 2 + 0] = (byte)((input[w - 2] * 3 + input[w - 1] + 2) >> 2);
			_out_[i * 2 + 1] = input[w - 1];
			return _out_;
		}

		private static FakePtr<byte> stbi__resample_row_hv_2(FakePtr<byte> _out_, FakePtr<byte> in_near,
			FakePtr<byte> in_far, int w, int hs)
		{
			var i = 0;
			var t0 = 0;
			var t1 = 0;
			if (w == 1)
			{
				_out_[0] = _out_[1] = (byte)((3 * in_near[0] + in_far[0] + 2) >> 2);
				return _out_;
			}

			t1 = 3 * in_near[0] + in_far[0];
			_out_[0] = (byte)((t1 + 2) >> 2);
			for (i = 1; i < w; ++i)
			{
				t0 = t1;
				t1 = 3 * in_near[i] + in_far[i];
				_out_[i * 2 - 1] = (byte)((3 * t0 + t1 + 8) >> 4);
				_out_[i * 2] = (byte)((3 * t1 + t0 + 8) >> 4);
			}

			_out_[w * 2 - 1] = (byte)((t1 + 2) >> 2);
			return _out_;
		}

		private static FakePtr<byte> stbi__resample_row_generic(FakePtr<byte> _out_, FakePtr<byte> in_near,
			FakePtr<byte> in_far, int w, int hs)
		{
			var i = 0;
			var j = 0;
			for (i = 0; i < w; ++i)
				for (j = 0; j < hs; ++j)
					_out_[i * hs + j] = in_near[i];
			return _out_;
		}

		private static void stbi__YCbCr_to_RGB_row(FakePtr<byte> _out_, FakePtr<byte> y, FakePtr<byte> pcb,
			FakePtr<byte> pcr, int count, int step)
		{
			var i = 0;
			for (i = 0; i < count; ++i)
			{
				var y_fixed = (y[i] << 20) + (1 << 19);
				var r = 0;
				var g = 0;
				var b = 0;
				var cr = pcr[i] - 128;
				var cb = pcb[i] - 128;
				r = y_fixed + cr * ((int)(1.40200f * 4096.0f + 0.5f) << 8);
				g = (int)(y_fixed + cr * -((int)(0.71414f * 4096.0f + 0.5f) << 8) +
						   ((cb * -((int)(0.34414f * 4096.0f + 0.5f) << 8)) & 0xffff0000));
				b = y_fixed + cb * ((int)(1.77200f * 4096.0f + 0.5f) << 8);
				r >>= 20;
				g >>= 20;
				b >>= 20;

				_out_[0] = stbi__clamp(r);
				_out_[1] = stbi__clamp(g);
				_out_[2] = stbi__clamp(b);
				_out_[3] = 255;
				_out_ += step;
			}
		}

		private void stbi__setup_jpeg()
		{
			idct_block_kernel = stbi__idct_block;
			YCbCr_to_RGB_kernel = stbi__YCbCr_to_RGB_row;
			resample_row_hv_2_kernel = stbi__resample_row_hv_2;
		}

		private void stbi__cleanup_jpeg()
		{
			stbi__free_jpeg_components(img_n, 0);
		}

		private static byte stbi__blinn_8x8(byte x, byte y)
		{
			var t = (uint)(x * y + 128);
			return (byte)((t + (t >> 8)) >> 8);
		}

		private byte[] load_jpeg_image(out int out_x, out int out_y, out int comp, int req_comp)
		{
			out_x = out_y = comp = 0;

			var n = 0;
			var decode_n = 0;
			var is_rgb = 0;
			img_n = 0;
			if (req_comp < 0 || req_comp > 4)
				stbi__err("bad req_comp");
			if (stbi__decode_jpeg_image() == 0)
			{
				stbi__cleanup_jpeg();
				return null;
			}

			n = req_comp != 0 ? req_comp : img_n >= 3 ? 3 : 1;
			is_rgb = img_n == 3 && (rgb == 3 || app14_color_transform == 0 && jfif == 0) ? 1 : 0;
			if (img_n == 3 && n < 3 && is_rgb == 0)
				decode_n = 1;
			else
				decode_n = img_n;
			{
				var k = 0;
				uint i = 0;
				uint j = 0;
				byte[] output;
				var coutput = new FakePtr<byte>[4];
				coutput[0] = FakePtr<byte>.Null;
				coutput[1] = FakePtr<byte>.Null;
				coutput[2] = FakePtr<byte>.Null;
				coutput[3] = FakePtr<byte>.Null;
				var res_comp = new stbi__resample[4];
				for (var kkk = 0; kkk < res_comp.Length; ++kkk)
					res_comp[kkk] = new stbi__resample();
				for (k = 0; k < decode_n; ++k)
				{
					var r = res_comp[k];
					img_comp[k].linebuf = new byte[img_x + 3];
					r.hs = img_h_max / img_comp[k].h;
					r.vs = img_v_max / img_comp[k].v;
					r.ystep = r.vs >> 1;
					r.w_lores = (img_x + r.hs - 1) / r.hs;
					r.ypos = 0;
					r.line0 = r.line1 = img_comp[k].data;
					if (r.hs == 1 && r.vs == 1)
						r.resample = resample_row_1;
					else if (r.hs == 1 && r.vs == 2)
						r.resample = stbi__resample_row_v_2;
					else if (r.hs == 2 && r.vs == 1)
						r.resample = stbi__resample_row_h_2;
					else if (r.hs == 2 && r.vs == 2)
						r.resample = resample_row_hv_2_kernel;
					else
						r.resample = stbi__resample_row_generic;
				}

				output = new byte[n * img_x * img_y];
				var ptr = new FakePtr<byte>(output);
				for (j = (uint)0; j < img_y; ++j)
				{
					var _out_ = ptr + n * img_x * j;
					for (k = 0; k < decode_n; ++k)
					{
						var r = res_comp[k];
						var y_bot = r.ystep >= r.vs >> 1 ? 1 : 0;
						coutput[k] = r.resample(new FakePtr<byte>(img_comp[k].linebuf),
							y_bot != 0 ? r.line1 : r.line0,
							y_bot != 0 ? r.line0 : r.line1,
							r.w_lores, r.hs);
						if (++r.ystep >= r.vs)
						{
							r.ystep = 0;
							r.line0 = r.line1;
							if (++r.ypos < img_comp[k].y)
								r.line1 += img_comp[k].w2;
						}
					}

					if (n >= 3)
					{
						var y = coutput[0];
						if (img_n == 3)
						{
							if (is_rgb != 0)
								for (i = (uint)0; i < img_x; ++i)
								{
									_out_[0] = y[i];
									_out_[1] = coutput[1][i];
									_out_[2] = coutput[2][i];
									_out_[3] = 255;
									_out_ += n;
								}
							else
								YCbCr_to_RGB_kernel(_out_, y, coutput[1], coutput[2], img_x, n);
						}
						else if (img_n == 4)
						{
							if (app14_color_transform == 0)
							{
								for (i = (uint)0; i < img_x; ++i)
								{
									var m = coutput[3][i];
									_out_[0] = stbi__blinn_8x8(coutput[0][i], m);
									_out_[1] = stbi__blinn_8x8(coutput[1][i], m);
									_out_[2] = stbi__blinn_8x8(coutput[2][i], m);
									_out_[3] = 255;
									_out_ += n;
								}
							}
							else if (app14_color_transform == 2)
							{
								YCbCr_to_RGB_kernel(_out_, y, coutput[1], coutput[2], img_x, n);
								for (i = (uint)0; i < img_x; ++i)
								{
									var m = coutput[3][i];
									_out_[0] = stbi__blinn_8x8((byte)(255 - _out_[0]), m);
									_out_[1] = stbi__blinn_8x8((byte)(255 - _out_[1]), m);
									_out_[2] = stbi__blinn_8x8((byte)(255 - _out_[2]), m);
									_out_ += n;
								}
							}
							else
							{
								YCbCr_to_RGB_kernel(_out_, y, coutput[1], coutput[2], img_x, n);
							}
						}
						else
						{
							for (i = (uint)0; i < img_x; ++i)
							{
								_out_[0] = _out_[1] = _out_[2] = y[i];
								_out_[3] = 255;
								_out_ += n;
							}
						}
					}
					else
					{
						if (is_rgb != 0)
						{
							if (n == 1)
								for (i = (uint)0; i < img_x; ++i)
								{
									_out_.Value =
										Conversion.stbi__compute_y(coutput[0][i], coutput[1][i], coutput[2][i]);
									_out_++;
								}
							else
								for (i = (uint)0; i < img_x; ++i, _out_ += 2)
								{
									_out_[0] = Conversion.stbi__compute_y(coutput[0][i], coutput[1][i], coutput[2][i]);
									_out_[1] = 255;
								}
						}
						else if (img_n == 4 && app14_color_transform == 0)
						{
							for (i = (uint)0; i < img_x; ++i)
							{
								var m = coutput[3][i];
								var r = stbi__blinn_8x8(coutput[0][i], m);
								var g = stbi__blinn_8x8(coutput[1][i], m);
								var b = stbi__blinn_8x8(coutput[2][i], m);
								_out_[0] = Conversion.stbi__compute_y(r, g, b);
								_out_[1] = 255;
								_out_ += n;
							}
						}
						else if (img_n == 4 && app14_color_transform == 2)
						{
							for (i = (uint)0; i < img_x; ++i)
							{
								_out_[0] = stbi__blinn_8x8((byte)(255 - coutput[0][i]), coutput[3][i]);
								_out_[1] = 255;
								_out_ += n;
							}
						}
						else
						{
							var y = coutput[0];
							if (n == 1)
								for (i = (uint)0; i < img_x; ++i)
									_out_[i] = y[i];
							else
								for (i = (uint)0; i < img_x; ++i)
								{
									_out_.Value = y[i];
									_out_++;
									_out_.Value = 255;
									_out_++;
								}
						}
					}
				}

				stbi__cleanup_jpeg();
				out_x = img_x;
				out_y = img_y;
				comp = img_n >= 3 ? 3 : 1;
				return output;
			}
		}

		private ImageResult InternalDecode(ColorComponents? requiredComponents)
		{
			stbi__setup_jpeg();

			int x, y, comp;
			var req_comp = requiredComponents == null ? 0 : (int)requiredComponents.Value;
			var result = load_jpeg_image(out x, out y, out comp, req_comp);

			return new ImageResult
			{
				Width = x,
				Height = y,
				SourceComponents = (ColorComponents)comp,
				ColorComponents = requiredComponents != null ? requiredComponents.Value : (ColorComponents)comp,
				BitsPerChannel = 8,
				Data = result
			};
		}

		public static bool Test(Stream stream)
		{
			var decoder = new JpgDecoder(stream);
			decoder.stbi__setup_jpeg();
			var r = decoder.stbi__decode_jpeg_header(STBI__SCAN_type);
			stream.Rewind();

			return r;
		}

		public static ImageInfo? Info(Stream stream)
		{
			var decoder = new JpgDecoder(stream);

			var r = decoder.stbi__decode_jpeg_header(STBI__SCAN_header);
			stream.Rewind();
			if (!r) return null;

			return new ImageInfo
			{
				Width = decoder.img_x,
				Height = decoder.img_y,
				ColorComponents = decoder.img_n >= 3 ? ColorComponents.RedGreenBlue : ColorComponents.Grey,
				BitsPerChannel = 8
			};
		}

		public static ImageResult Decode(Stream stream, ColorComponents? requiredComponents = null)
		{
			var decoder = new JpgDecoder(stream);
			return decoder.InternalDecode(requiredComponents);
		}
	}
}