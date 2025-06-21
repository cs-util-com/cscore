using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace com.csutil.xml {

    public static class XmlParser {

        public static T ParseAsXmlInto<T>(this Stream self) {
            AssertV3.IsTrue(self.CanSeek, () => "Cant reset stream position to beginning, XML parsing may fail");
            if (self.CanSeek) { self.ResetStreamCurserPositionToBeginning(); }
            using (var r = XmlReader.Create(self)) {
                return (T)new XmlSerializer(typeof(T)).Deserialize(r);
            }
        }

        public static T ParseAsXmlInto<T>(this TextReader self) {
            using (var r = XmlReader.Create(self)) {
                return (T)new XmlSerializer(typeof(T)).Deserialize(r);
            }
        }

        public static T ParseAsXmlInto<T>(string xmlString) {
            using (var reader = new StringReader(xmlString)) {
                return reader.ParseAsXmlInto<T>();
            }
        }

    }

}