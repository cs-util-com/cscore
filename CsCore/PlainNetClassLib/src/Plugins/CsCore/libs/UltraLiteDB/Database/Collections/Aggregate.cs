using System;

namespace UltraLiteDB
{
    public partial class UltraLiteCollection<T>
    {
        #region Count

        /// <summary>
        /// Get document count using property on collection.
        /// </summary>
        public int Count()
        {
            // do not use indexes - collections has DocumentCount property
            return (int)_engine.Value.Count(_name, null);
        }

        /// <summary>
        /// Count documents matching a query. This method does not deserialize any document. Needs indexes on query expression
        /// </summary>
        public int Count(Query query)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));

            return (int)_engine.Value.Count(_name, query);
        }


        #endregion

        #region LongCount

        /// <summary>
        /// Get document count using property on collection.
        /// </summary>
        public long LongCount()
        {
            // do not use indexes - collections has DocumentCount property
            return _engine.Value.Count(_name, null);
        }

        /// <summary>
        /// Count documents matching a query. This method does not deserialize any documents. Needs indexes on query expression
        /// </summary>
        public long LongCount(Query query)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));

            return _engine.Value.Count(_name, query);
        }


        #endregion

        #region Exists

        /// <summary>
        /// Returns true if query returns any document. This method does not deserialize any document. Needs indexes on query expression
        /// </summary>
        public bool Exists(Query query)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));

            return _engine.Value.Exists(_name, query);
        }


        #endregion

        #region Min/Max

        /// <summary>
        /// Returns the first/min value from a index field
        /// </summary>
        public BsonValue Min(string field)
        {
            if (string.IsNullOrEmpty(field)) throw new ArgumentNullException(nameof(field));

            return _engine.Value.Min(_name, field);
        }


        /// <summary>
        /// Returns the last/max value from a index field
        /// </summary>
        public BsonValue Max(string field)
        {
            if (string.IsNullOrEmpty(field)) throw new ArgumentNullException(nameof(field));

            return _engine.Value.Max(_name, field);
        }



        #endregion
    }
}