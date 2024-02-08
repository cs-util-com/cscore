using System.IO;
using LiteDB;

namespace UltraLiteDB {
    
    public class UltraLiteDatabase : LiteDatabase {
        
        private readonly bool _disposeStream;
        private readonly Stream _stream;

        public UltraLiteDatabase(Stream stream, BsonMapper mapper = null, bool disposeStream = false, Stream logStream = null) : base(stream, mapper, logStream) {
            _disposeStream = disposeStream;
            _stream = stream;
        }

        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);
            if (disposing && _disposeStream) {
                _stream.Dispose();
            }
        }

    }
    
}