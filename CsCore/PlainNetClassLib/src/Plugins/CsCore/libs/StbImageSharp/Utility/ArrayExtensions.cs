using System;

namespace StbImageSharp.Utility
{
	internal static class ArrayExtensions
	{
		public static void Clear(this Array array)
		{
			Array.Clear(array, 0, array.Length);
		}

		public static void Set<T>(this T[] array, int index, int length, T value)
		{
			for(var i = index; i < index + length; ++i)
			{
				array[i] = value;
			}
		}

		public static void Set<T>(this T[] array, T value)
		{
			array.Set(0, array.Length, value);
		}

		public static void memcpy<T>(this T[] a, FakePtr<T> b, int count) where T: new()
		{
			Array.Copy(b._array, b.Offset, a, 0, count);
		}
	}
}
