using System;

namespace StbImageSharp.Utility
{
	internal struct FakePtr<T> where T: new()
	{
		public static FakePtr<T> Null = new FakePtr<T>(null);

		public readonly T[] _array;

		public int Offset;

		public bool IsNull
		{
			get
			{
				return _array == null;
			}
		}

		public T this[int index]
		{
			get
			{
				return _array[Offset + index];
			}

			set
			{
				_array[Offset + index] = value;
			}
		}

		public T this[long index]
		{
			get
			{
				return _array[Offset + index];
			}

			set
			{
				_array[Offset + index] = value;
			}
		}

		public T Value
		{
			get
			{
				return this[0];
			}

			set
			{
				this[0] = value;
			}
		}

		public FakePtr(FakePtr<T> ptr, int offset)
		{
			Offset = ptr.Offset + offset;
			_array = ptr._array;
		}

		public FakePtr(T[] data, int offset)
		{
			Offset = offset;
			_array = data;
		}

		public FakePtr(T[] data): this(data, 0)
		{
		}

		public FakePtr(T value)
		{
			Offset = 0;
			_array = new T[1];
			_array[0] = value;
		}

		public void Clear(int count)
		{
			Array.Clear(_array, Offset, count);
		}

		public T GetAndIncrease()
		{
			var result = _array[Offset];
			++Offset;

			return result;
		}

		public void SetAndIncrease(T value)
		{
			_array[Offset] = value;
			++Offset;
		}

		public void Set(T value)
		{
			_array[Offset] = value;
		}

		public static FakePtr<T> operator +(FakePtr<T> p, int offset)
		{
			return new FakePtr<T>(p._array) { Offset = p.Offset + offset };
		}

		public static FakePtr<T> operator -(FakePtr<T> p, int offset)
		{
			return p + -offset;
		}

		public static FakePtr<T> operator +(FakePtr<T> p, uint offset)
		{
			return p + (int)offset;
		}

		public static FakePtr<T> operator -(FakePtr<T> p, uint offset)
		{
			return p - (int)offset;
		}

		public static FakePtr<T> operator +(FakePtr<T> p, long offset)
		{
			return p + (int)offset;
		}

		public static FakePtr<T> operator -(FakePtr<T> p, long offset)
		{
			return p - (int)offset;
		}

		public static FakePtr<T> operator ++(FakePtr<T> p)
		{
			return p + 1;
		}

		public static FakePtr<T> CreateWithSize(int size)
		{
			var result = new FakePtr<T>(new T[size]);

			for (int i = 0; i < size; ++i)
			{
				result[i] = new T();
			}

			return result;
		}

		public static FakePtr<T> CreateWithSize(long size)
		{
			return CreateWithSize((int)size);
		}

		public static FakePtr<T> Create()
		{
			return CreateWithSize(1);
		}

		public void memset(T value, int count)
		{
			_array.Set(Offset, count, value);
		}

		public void memcpy(FakePtr<T> b, int count)
		{
			Array.Copy(b._array, b.Offset, _array, Offset, count);
		}

		public void memcpy(T[] b, int count)
		{
			Array.Copy(b, 0, _array, Offset, count);
		}
	}
}