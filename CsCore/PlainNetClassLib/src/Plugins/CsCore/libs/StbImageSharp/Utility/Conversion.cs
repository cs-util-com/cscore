using StbImageSharp.Decoding;
using StbImageSharp.Utility;
using System;

namespace StbImageSharp
{
	internal static class Conversion
	{
		public static byte stbi__compute_y(int r, int g, int b)
		{
			return (byte)(((r * 77) + (g * 150) + (29 * b)) >> 8);
		}

		public static ushort stbi__compute_y_16(int r, int g, int b)
		{
			return (ushort)(((r * 77) + (g * 150) + (29 * b)) >> 8);
		}

		public static byte[] stbi__convert_format16(byte[] data, int img_n, int req_comp, uint x, uint y)
		{
			throw new NotImplementedException();
/*			int i = 0;
			int j = 0;
			if ((req_comp) == (img_n))
				return data;

			var good = new byte[req_comp * x * y * 2];
			FakePtr<byte> dataPtr = new FakePtr<byte>(data);
			FakePtr<byte> goodPtr = new FakePtr<byte>(good);
			for (j = (int)(0); (j) < ((int)(y)); ++j)
			{
				ushort* src = (ushort*)dataPtr + j * x * img_n;
				ushort* dest = (ushort*)goodPtr + j * x * req_comp;
				switch (((img_n) * 8 + (req_comp)))
				{
					case ((1) * 8 + (2)):
						for (i = (int)(x - 1); (i) >= (0); --i, src += 1, dest += 2)
						{
							dest[0] = (ushort)(src[0]);
							dest[1] = (ushort)(0xffff);
						}
						break;
					case ((1) * 8 + (3)):
						for (i = (int)(x - 1); (i) >= (0); --i, src += 1, dest += 3)
						{
							dest[0] = (ushort)(dest[1] = (ushort)(dest[2] = (ushort)(src[0])));
						}
						break;
					case ((1) * 8 + (4)):
						for (i = (int)(x - 1); (i) >= (0); --i, src += 1, dest += 4)
						{
							dest[0] = (ushort)(dest[1] = (ushort)(dest[2] = (ushort)(src[0])));
							dest[3] = (ushort)(0xffff);
						}
						break;
					case ((2) * 8 + (1)):
						for (i = (int)(x - 1); (i) >= (0); --i, src += 2, dest += 1)
						{
							dest[0] = (ushort)(src[0]);
						}
						break;
					case ((2) * 8 + (3)):
						for (i = (int)(x - 1); (i) >= (0); --i, src += 2, dest += 3)
						{
							dest[0] = (ushort)(dest[1] = (ushort)(dest[2] = (ushort)(src[0])));
						}
						break;
					case ((2) * 8 + (4)):
						for (i = (int)(x - 1); (i) >= (0); --i, src += 2, dest += 4)
						{
							dest[0] = (ushort)(dest[1] = (ushort)(dest[2] = (ushort)(src[0])));
							dest[3] = (ushort)(src[1]);
						}
						break;
					case ((3) * 8 + (4)):
						for (i = (int)(x - 1); (i) >= (0); --i, src += 3, dest += 4)
						{
							dest[0] = (ushort)(src[0]);
							dest[1] = (ushort)(src[1]);
							dest[2] = (ushort)(src[2]);
							dest[3] = (ushort)(0xffff);
						}
						break;
					case ((3) * 8 + (1)):
						for (i = (int)(x - 1); (i) >= (0); --i, src += 3, dest += 1)
						{
							dest[0] = (ushort)(stbi__compute_y_16((int)(src[0]), (int)(src[1]), (int)(src[2])));
						}
						break;
					case ((3) * 8 + (2)):
						for (i = (int)(x - 1); (i) >= (0); --i, src += 3, dest += 2)
						{
							dest[0] = (ushort)(stbi__compute_y_16((int)(src[0]), (int)(src[1]), (int)(src[2])));
							dest[1] = (ushort)(0xffff);
						}
						break;
					case ((4) * 8 + (1)):
						for (i = (int)(x - 1); (i) >= (0); --i, src += 4, dest += 1)
						{
							dest[0] = (ushort)(stbi__compute_y_16((int)(src[0]), (int)(src[1]), (int)(src[2])));
						}
						break;
					case ((4) * 8 + (2)):
						for (i = (int)(x - 1); (i) >= (0); --i, src += 4, dest += 2)
						{
							dest[0] = (ushort)(stbi__compute_y_16((int)(src[0]), (int)(src[1]), (int)(src[2])));
							dest[1] = (ushort)(src[3]);
						}
						break;
					case ((4) * 8 + (3)):
						for (i = (int)(x - 1); (i) >= (0); --i, src += 4, dest += 3)
						{
							dest[0] = (ushort)(src[0]);
							dest[1] = (ushort)(src[1]);
							dest[2] = (ushort)(src[2]);
						}
						break;
					default:
						Decoder.stbi__err("0");
						break;
				}
			}

			return good;*/
		}

		public static byte[] stbi__convert_format(byte[] data, int img_n, int req_comp, uint x, uint y)
		{
			int i = 0;
			int j = 0;
			if ((req_comp) == (img_n))
				return data;

			var good = new byte[req_comp * x * y];
			for (j = (int)(0); (j) < ((int)(y)); ++j)
			{
				FakePtr<byte> src = new FakePtr<byte>(data, (int)(j * x * img_n));
				FakePtr<byte> dest = new FakePtr<byte>(good, (int)(j * x * req_comp));
				switch (((img_n) * 8 + (req_comp)))
				{
					case ((1) * 8 + (2)):
						for (i = (int)(x - 1); (i) >= (0); --i, src += 1, dest += 2)
						{
							dest[0] = (byte)(src[0]);
							dest[1] = (byte)(255);
						}
						break;
					case ((1) * 8 + (3)):
						for (i = (int)(x - 1); (i) >= (0); --i, src += 1, dest += 3)
						{
							dest[0] = (byte)(dest[1] = (byte)(dest[2] = (byte)(src[0])));
						}
						break;
					case ((1) * 8 + (4)):
						for (i = (int)(x - 1); (i) >= (0); --i, src += 1, dest += 4)
						{
							dest[0] = (byte)(dest[1] = (byte)(dest[2] = (byte)(src[0])));
							dest[3] = (byte)(255);
						}
						break;
					case ((2) * 8 + (1)):
						for (i = (int)(x - 1); (i) >= (0); --i, src += 2, dest += 1)
						{
							dest[0] = (byte)(src[0]);
						}
						break;
					case ((2) * 8 + (3)):
						for (i = (int)(x - 1); (i) >= (0); --i, src += 2, dest += 3)
						{
							dest[0] = (byte)(dest[1] = (byte)(dest[2] = (byte)(src[0])));
						}
						break;
					case ((2) * 8 + (4)):
						for (i = (int)(x - 1); (i) >= (0); --i, src += 2, dest += 4)
						{
							dest[0] = (byte)(dest[1] = (byte)(dest[2] = (byte)(src[0])));
							dest[3] = (byte)(src[1]);
						}
						break;
					case ((3) * 8 + (4)):
						for (i = (int)(x - 1); (i) >= (0); --i, src += 3, dest += 4)
						{
							dest[0] = (byte)(src[0]);
							dest[1] = (byte)(src[1]);
							dest[2] = (byte)(src[2]);
							dest[3] = (byte)(255);
						}
						break;
					case ((3) * 8 + (1)):
						for (i = (int)(x - 1); (i) >= (0); --i, src += 3, dest += 1)
						{
							dest[0] = (byte)(stbi__compute_y((int)(src[0]), (int)(src[1]), (int)(src[2])));
						}
						break;
					case ((3) * 8 + (2)):
						for (i = (int)(x - 1); (i) >= (0); --i, src += 3, dest += 2)
						{
							dest[0] = (byte)(stbi__compute_y((int)(src[0]), (int)(src[1]), (int)(src[2])));
							dest[1] = (byte)(255);
						}
						break;
					case ((4) * 8 + (1)):
						for (i = (int)(x - 1); (i) >= (0); --i, src += 4, dest += 1)
						{
							dest[0] = (byte)(stbi__compute_y((int)(src[0]), (int)(src[1]), (int)(src[2])));
						}
						break;
					case ((4) * 8 + (2)):
						for (i = (int)(x - 1); (i) >= (0); --i, src += 4, dest += 2)
						{
							dest[0] = (byte)(stbi__compute_y((int)(src[0]), (int)(src[1]), (int)(src[2])));
							dest[1] = (byte)(src[3]);
						}
						break;
					case ((4) * 8 + (3)):
						for (i = (int)(x - 1); (i) >= (0); --i, src += 4, dest += 3)
						{
							dest[0] = (byte)(src[0]);
							dest[1] = (byte)(src[1]);
							dest[2] = (byte)(src[2]);
						}
						break;
					default:
						Decoder.stbi__err("0");
						break;
				}
			}

			return good;
		}

		public static byte[] stbi__convert_16_to_8(byte[] orig, int w, int h, int channels)
		{
			throw new NotImplementedException();

/*			int i = 0;
			int img_len = (int)(w * h * channels);
			var reduced = new byte[img_len];

			fixed (byte* ptr2 = &orig[0])
			{
				ushort* ptr = (ushort*)ptr2;
				for (i = (int)(0); (i) < (img_len); ++i)
				{
					reduced[i] = ((byte)((ptr[i] >> 8) & 0xFF));
				}
			}
			return reduced;*/
		}

		public static ushort[] stbi__convert_8_to_16(byte[] orig, int w, int h, int channels)
		{
			int i = 0;
			int img_len = (int)(w * h * channels);
			var enlarged = new ushort[img_len];
			for (i = (int)(0); (i) < (img_len); ++i)
			{
				enlarged[i] = ((ushort)((orig[i] << 8) + orig[i]));
			}

			return enlarged;
		}

		public static void stbi__vertical_flip(byte[] image, int w, int h, int bytes_per_pixel)
		{
			int row = 0;
			int bytes_per_row = w * bytes_per_pixel;
			byte[] temp = new byte[2048];
			for (row = (int)(0); (row) < (h >> 1); row++)
			{
				FakePtr<byte> row0 = new FakePtr<byte>(image, (int)(row * bytes_per_row));
				FakePtr<byte> row1 = new FakePtr<byte>(image, (int)((h - row - 1) * bytes_per_row));
				int bytes_left = bytes_per_row;
				while ((bytes_left) != 0)
				{
					int bytes_copy = (((bytes_left) < (2048)) ? bytes_left : 2048);
					temp.memcpy(row0, bytes_copy);
					row0.memcpy(row1, bytes_copy);
					row1.memcpy(temp, bytes_copy);
					row0 += bytes_copy;
					row1 += bytes_copy;
					bytes_left -= bytes_copy;
				}
			}
		}
	}
}
