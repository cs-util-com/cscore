using System;

namespace UltraLiteDB
{
    public sealed partial class UltraLiteCollection<T>
    {
        private string _name;
        private LazyLoad<UltraLiteEngine> _engine;
        private BsonMapper _mapper;
        private readonly EntityMapper _entity;
        private Logger _log;
        private MemberMapper _id;
        private BsonAutoId _autoId;

        /// <summary>
        /// Get collection name
        /// </summary>
        public string Name { get { return _name; } }

        /// <summary>
        /// Getting entity mapper from current collection. Returns null if collection are BsonDocument type
        /// </summary>
        public EntityMapper EntityMapper => _entity;

        public UltraLiteCollection(string name, BsonAutoId autoId, LazyLoad<UltraLiteEngine> engine, BsonMapper mapper, Logger log)
        {
            _name = name ?? mapper.ResolveCollectionName(typeof(T));
            _engine = engine;
            _mapper = mapper;
            _log = log;

            // if strong typed collection, get _id member mapped (if exists)
            if (typeof(T) == typeof(BsonDocument))
            {
                _entity = null;
                _id = null;
                _autoId = autoId;
            }
            else
            {
                _entity = mapper.GetEntityMapper(typeof(T));
                _id = _entity.Id;

                if (_id != null && _id.AutoId)
                {
                    _autoId =
                        _id.DataType == typeof(Int32) || _id.DataType == typeof(Int32?) ? BsonAutoId.Int32 :
                        _id.DataType == typeof(Int64) || _id.DataType == typeof(Int64?) ? BsonAutoId.Int64 :
                        _id.DataType == typeof(Guid) || _id.DataType == typeof(Guid?) ? BsonAutoId.Guid :
                        BsonAutoId.ObjectId;
                }
                else
                {
                    _autoId = BsonAutoId.ObjectId;
                }
            }
        }
    }
}