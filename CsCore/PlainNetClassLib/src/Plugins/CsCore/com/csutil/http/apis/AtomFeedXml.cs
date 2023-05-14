using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using com.csutil.xml;

namespace com.csutil.http {

    [XmlRoot(ElementName = "feed", Namespace = "http://www.w3.org/2005/Atom")]
    public class AtomFeedXml {

        public static async Task<AtomFeedXml> Get(Uri atomUrl) {
            using (var resultStream = await atomUrl.SendGET().GetResult<Stream>()) {
                return resultStream.ParseAsXmlInto<AtomFeedXml>();
            }
        }

        [XmlElement(ElementName = "id")]
        public string Id;

        [XmlElement(ElementName = "title")]
        public string Title;

        [XmlElement(ElementName = "link")]
        public FeedLink Link;

        [XmlElement(ElementName = "updated")]
        public DateTime Updated;

        [XmlElement(ElementName = "entry")]
        public List<FeedEntry> Entry;

        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns;

        [XmlAttribute(AttributeName = "idx")]
        public string Idx;

        [XmlText]
        public string Text;

        [XmlRoot(ElementName = "entry")]
        public class FeedEntry {

            [XmlElement(ElementName = "id")]
            public string Id;

            [XmlElement(ElementName = "title")]
            public EntryTitle Title;

            [XmlElement(ElementName = "link")]
            public FeedLink Link;

            [XmlElement(ElementName = "published")]
            public DateTime Published;

            [XmlElement(ElementName = "updated")]
            public DateTime Updated;

            [XmlElement(ElementName = "content")]
            public EntryContent Content;

            [XmlElement(ElementName = "author")]
            public EntryAuthor Author;
        }

        [XmlRoot(ElementName = "link")]
        public class FeedLink {

            [XmlAttribute(AttributeName = "href")]
            public string Href;

            [XmlAttribute(AttributeName = "rel")]
            public string Rel;
        }

        [XmlRoot(ElementName = "title")]
        public class EntryTitle {

            [XmlAttribute(AttributeName = "type")]
            public string Type;

            [XmlText]
            public string Text;
        }

        [XmlRoot(ElementName = "content")]
        public class EntryContent {

            [XmlAttribute(AttributeName = "type")]
            public string Type;

            [XmlText]
            public string Text;
        }

        [XmlRoot(ElementName = "author")]
        public class EntryAuthor {

            [XmlElement(ElementName = "name")]
            public object Name;
        }

    }

}