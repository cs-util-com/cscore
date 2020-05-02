using System;

namespace UltraLiteDB
{
    /// <summary>
    /// Class to call method for convert BsonDocument to/from byte[] - based on http://bsonspec.org/spec.html
    /// </summary>
    public static class BsonSerializer
    {
        public static byte[] Serialize(BsonDocument doc)
        {
            if (doc == null) throw new ArgumentNullException(nameof(doc));


            return BsonWriter.Serialize(doc);
        }

        public static int SerializeTo(BsonDocument doc, byte[] array)
        {
            if (doc == null) throw new ArgumentNullException(nameof(doc));

            return BsonWriter.SerializeTo(doc, array);
        }

        public static BsonDocument Deserialize(byte[] bson, bool utcDate = true)
        {
            if (bson == null || bson.Length == 0) throw new ArgumentNullException(nameof(bson));

            return BsonReader.Deserialize(bson, utcDate);
        }
    }
}