namespace ZetaLongPaths
{
    [Serializable]
    public class ZlpSimpleFileAccessProtectorException :
        Exception
    {
        private const string MagicKey = @"a08c7921-3b9f-4d48-bd82-280636202e40";

        [PublicAPI]
        public ZlpSimpleFileAccessProtectorException()
        {
            setMagicKey();
        }

        [PublicAPI]
        public ZlpSimpleFileAccessProtectorException(string message) : base(message)
        {
            setMagicKey();
        }

        [PublicAPI]
        public ZlpSimpleFileAccessProtectorException(string message, Exception inner) : base(message, inner)
        {
            setMagicKey();
        }

        [PublicAPI]
        protected ZlpSimpleFileAccessProtectorException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
            setMagicKey();
        }

        /// <summary>
        /// Sometimes, e.g. when serializing to JSON and later deserializing,
        /// one might loose the exact type. Therefore another marker is set
        /// through the Data property. Use this method to query whether a given
        /// exception has that marker set to identify as a ZlpSimpleFileAccessProtectorException
        /// even if it is not of the exact type.
        /// </summary>
        public static bool IsSimpleFileAccessProtectorException(Exception x)
        {
            return x switch
            {
                null => false,
                ZlpSimpleFileAccessProtectorException => true,
                _ => hasKey(x, MagicKey)
            };
        }

        public static void MarkAsSimpleFileAccessProtectorException(Exception x)
        {
            if (x != null) x.Data[MagicKey] = MagicKey;
        }

        private void setMagicKey()
        {
            MarkAsSimpleFileAccessProtectorException(this);
        }

        private static bool hasKey(Exception exception, string key)
        {
            if (exception == null || string.IsNullOrEmpty(key)) return false;

            return exception.Data.Keys.Cast<object>().Any(d => d != null && d.ToString().EqualsNoCase(key));
        }
    }
}