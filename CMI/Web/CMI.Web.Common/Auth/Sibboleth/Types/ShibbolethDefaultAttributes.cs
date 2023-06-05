using Aspose.Cells.Charts;
using CMI.Web.Common.Auth.Sibboleth.Types.Xml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;

namespace CMI.Web.Common.Auth.Sibboleth.Types
{
    /// <summary>
    /// Creates a list of <see cref="IShibbolethAttribute"/> based on the included attribute-map.xml
    /// </summary>
    public static class ShibbolethDefaultAttributes
    {
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public static IList<IShibbolethAttribute> GetAttributeMapping()
        {

            // get the default XML file
            var serializer = new XmlSerializer(typeof(AttributesRoot));

            AttributesRoot attrib_root;

            var configFile = Path.Combine(AppDomain.CurrentDomain.GetData("DataDirectory").ToString(), "sibboleth_attribute-map.xml");

            var resourceName = "CMI.Web.Common.Auth.Sibboleth.Types.Xml.attribute-map.xml";
            
            using (Stream resourceStream = File.Exists(configFile) ? File.Open(configFile, FileMode.Open, FileAccess.Read) : Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                using (StreamReader sr = new StreamReader(resourceStream))
                {
                    attrib_root = (AttributesRoot)serializer.Deserialize(sr);
                }

                return new List<IShibbolethAttribute>(attrib_root.Attribute);
            }
        }
    }
}
