using System.ComponentModel;
using CMI.Contract.Parameter;

namespace CMI.Manager.Lesesaal
{
    public class CollectionSettings : ISetting
    {
        [DefaultValue("{{#IstDeutsch}}" +
                      "## Sammlungen" +
                      "{{/IstDeutsch}}" +
                      "{{#IstFranzösisch}}" +
                      "## Collections" +
                      "{{/IstFranzösisch}}" +
                      "{{#IstItalienisch}}" +
                      "## Collezioni" +
                      "{{/IstItalienisch}}" +
                      "{{#IstEnglisch}}" +
                      "## Collections" +
                      "{{/IstEnglisch}}")]
        public string CollectionHeader { get; set; }
    }
}
