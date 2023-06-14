using CMI.Web.Common.Auth.Sibboleth.Types;
using System.Xml.Serialization;

namespace CMI.Web.Common.Auth.Sibboleth.Types
{
    /// <summary>
    /// concrete implementation of <see cref="IShibbolethAttribute"/>
    /// </summary>
    [XmlRoot("Attribute")]
    public class ShibbolethAttribute : IShibbolethAttribute
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("id")]
        public string Id { get; set; }
    }
}
