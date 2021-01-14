using System;

namespace com.csutil {

    public static class Guidv2 {

		private static readonly long BaseDateTicks = new DateTime(1900, 1, 1).Ticks;

		/// <summary> 
		/// Generate a new <see cref="Guid"/> using the comb algorithm. 
		/// 
		/// The <c>comb</c> algorithm is designed to make the use of GUIDs as Primary Keys, Foreign Keys, 
		/// and Indexes nearly as efficient as ints.
		/// 
		/// The <c>comb</c> algorithm is designed to make the use of GUIDs as Primary Keys, Foreign Keys, 
		/// and Indexes nearly as efficient as ints.
		/// 
		/// This code was suggested in Jimmy Nilsson's 
		/// <a href="http://www.informit.com/articles/article.asp?p=25862">article</a>
		/// on <a href="http://www.informit.com">informit.com</a> and contributed by Donald Mull.
		/// 
		/// Source: https://stackoverflow.com/a/25472825/165106
		/// </summary>
		public static Guid NewGuidSequentialAndPartiallyOrdered() {
			byte[] guidArray = Guid.NewGuid().ToByteArray();

			DateTime now = DateTimeV2.UtcNow;

			// Get the days and milliseconds which will be used to build the byte string 
			TimeSpan days = new TimeSpan(now.Ticks - BaseDateTicks);
			TimeSpan msecs = now.TimeOfDay;

			// Convert to a byte array 
			// Note that SQL Server is accurate to 1/300th of a millisecond so we divide by 3.333333 
			byte[] daysArray = BitConverter.GetBytes(days.Days);
			byte[] msecsArray = BitConverter.GetBytes((long)(msecs.TotalMilliseconds / 3.333333));

			// Reverse the bytes to match SQL Servers ordering 
			Array.Reverse(daysArray);
			Array.Reverse(msecsArray);

			// Copy the bytes into the guid 
			Array.Copy(daysArray, daysArray.Length - 2, guidArray, guidArray.Length - 6, 2);
			Array.Copy(msecsArray, msecsArray.Length - 4, guidArray, guidArray.Length - 4, 4);

			return new Guid(guidArray);
		}

	}

}