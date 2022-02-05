using System;
using System.Threading;

namespace com.csutil {

    public static class GuidV2 {

        /// <summary> Can be set to override the default behavior of <see cref="GuidV2.NewGuid"/> </summary>
        public static Func<Guid> OnCreateGuidV2Request;

        private static long _counter = DateTimeV2.UtcNow.Ticks;
        // See idea from https://mrpmorris.blogspot.com/2020/07/generating-globally-unique-sequential.html
        private static class Counter {


            private static long _counter = DateTime.UtcNow.Ticks;

            public static long Increment() {
                long result;
                long ticksNow = DateTime.UtcNow.Ticks;
                do {
                    result = Interlocked.Increment(ref _counter);
                    if (result >= ticksNow) { return result; }
                } while (Interlocked.CompareExchange(ref _counter, ticksNow, result) != result);
                return result;
            }

        }

        /// <summary> 
        ///     <para>
        ///         Generates <b>sequential / parially ordered</b> <see cref="Guid" /> values which allows ordering and is 
        ///         optimized for use in databases (like Microsoft SQL server) clustered keys or indexes,
        ///         yielding better performance than random values. Because of this it is the default generator for
        ///         SQL Server <see cref="Guid" /> columns which are set to be generated on add.
        ///     </para>
        ///     <para>
        ///         See https://docs.microsoft.com/sql/t-sql/functions/newsequentialid-transact-sql.
        ///         Although this generator achieves the same goals as SQL Server's NEWSEQUENTIALID, the algorithm used
        ///         to generate the GUIDs is different.
        ///     </para>
        ///     <para>
        ///         The generated values are non-temporary, meaning they will be saved to the database.
        ///     </para>
        ///     Source: https://github.com/dotnet/efcore/blob/main/src/EFCore/ValueGeneration/SequentialGuidValueGenerator.cs
        /// </summary>
        public static Guid NewGuid() {

            if (OnCreateGuidV2Request != null) { return OnCreateGuidV2Request(); }

            var guidBytes = Guid.NewGuid().ToByteArray();
            var counterBytes = BitConverter.GetBytes(Counter.Increment());
            // var counterBytes = BitConverter.GetBytes(Interlocked.Increment(ref _counter));

            if (!BitConverter.IsLittleEndian) {
                Array.Reverse(counterBytes);
            }

            guidBytes[08] = counterBytes[1];
            guidBytes[09] = counterBytes[0];
            guidBytes[10] = counterBytes[7];
            guidBytes[11] = counterBytes[6];
            guidBytes[12] = counterBytes[5];
            guidBytes[13] = counterBytes[4];
            guidBytes[14] = counterBytes[3];
            guidBytes[15] = counterBytes[2];

            return new Guid(guidBytes);
        }

        /// <summary> See https://stackoverflow.com/a/49279802/165106 </summary>
        public static int CompareToV2(this Guid self, Guid other) {
            const int NUM_BYTES_IN_GUID = 16;
            byte byte1, byte2;

            byte[] xBytes = new byte[NUM_BYTES_IN_GUID];
            byte[] yBytes = new byte[NUM_BYTES_IN_GUID];

            self.ToByteArray().CopyTo(xBytes, 0);
            other.ToByteArray().CopyTo(yBytes, 0);

            // The correct order to be compared:
            int[] byteOrder = new int[16] // 16 Bytes = 128 Bit 
                {10, 11, 12, 13, 14, 15, 8, 9, 6, 7, 4, 5, 0, 1, 2, 3};

            //Swap to the correct order to be compared
            for (int i = 0; i < NUM_BYTES_IN_GUID; i++) {
                byte1 = xBytes[byteOrder[i]];
                byte2 = yBytes[byteOrder[i]];
                if (byte1 != byte2) { return (byte1 < byte2) ? -1 : 1; }
            }

            return 0;
        }

    }

}