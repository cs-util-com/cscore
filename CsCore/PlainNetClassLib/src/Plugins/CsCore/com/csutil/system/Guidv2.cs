using System;
using System.Threading;

namespace com.csutil {

    public static class Guidv2 {

        private static long _counter = DateTimeV2.UtcNow.Ticks;

        /// <summary> 
        ///     <para>
        ///         Generates sequential <see cref="Guid" /> values optimized for use in Microsoft SQL server clustered
        ///         keys or indexes, yielding better performance than random values. This is the default generator for
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
        public static Guid NewGuidSequentialAndPartiallyOrdered() {
            var guidBytes = Guid.NewGuid().ToByteArray();
            var counterBytes = BitConverter.GetBytes(Interlocked.Increment(ref _counter));

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

    }

}