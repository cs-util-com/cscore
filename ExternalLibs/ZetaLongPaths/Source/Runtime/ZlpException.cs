namespace ZetaLongPaths
{
    [Serializable]
    [PublicAPI]
    public class ZlpException : Exception
    {
        public ZlpException()
        {
        }

        public ZlpException(string message) : base(message)
        {
        }

        public ZlpException(string message, Exception inner) : base(message, inner)
        {
        }

        protected ZlpException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}