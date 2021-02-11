namespace StbImageSharp.Utility
{
	internal static class MathExtensions
	{
		public static int stbi__bitreverse16(int n)
		{
			n = (int)(((n & 0xAAAA) >> 1) | ((n & 0x5555) << 1));
			n = (int)(((n & 0xCCCC) >> 2) | ((n & 0x3333) << 2));
			n = (int)(((n & 0xF0F0) >> 4) | ((n & 0x0F0F) << 4));
			n = (int)(((n & 0xFF00) >> 8) | ((n & 0x00FF) << 8));
			return (int)(n);
		}

		public static int stbi__bit_reverse(int v, int bits)
		{
			return (int)(stbi__bitreverse16((int)(v)) >> (16 - bits));
		}

		public static uint _lrotl(uint x, int y)
		{
			return (x << y) | (x >> (32 - y));
		}

		public static int ToReqComp(this ColorComponents? requiredComponents)
		{
			return requiredComponents != null ? (int)requiredComponents.Value : 0;
		}
	}
}