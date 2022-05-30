using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace CMI.Access.Harvest
{
    public static class XMLConvert
    {
        public static T FromXML<T>(string xmlStrig) where T : class
        {
            var xmlSerializer = new XmlSerializer(typeof(T));
            using var sReader = new StringReader(xmlStrig);
            return (T)xmlSerializer.Deserialize(sReader);
        }

        public static string ToXml<T>(T obj) where T: class
        {
            if (obj == null)
                return string.Empty;
            
            var xmlserializer = new XmlSerializer(typeof(T));
            var stringWriter = new StringWriter();
            using var writer = XmlWriter.Create(stringWriter);
            xmlserializer.Serialize(writer, obj);
            return stringWriter.ToString();
        }
    }
}