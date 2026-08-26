using System.ComponentModel;
using CMI.Contract.Parameter;
using CMI.Contract.Parameter.Attributes;

namespace CMI.Engine.Asset.ParameterSettings
{
    public class TitelblattSettings : ISetting
    {
        [Description("HTML-Template für das Titelblatt bei AIS-Datei-Downloads (Mustache-Format). " +
                     "Verfügbare Platzhalter: {{logo_base64}}, {{titel}}, {{entstehungszeitraum}}, " +
                     "{{urheber}}, {{signatur}}, {{permanente_url}} sowie beliebige weitere Key-Value-Paare " +
                     "aus den Metadaten des Requests.")]
        [ReadDefaultFromResource]
        public string HtmlTemplate { get; set; }

        [Description("Logo für das Titelblatt als Base64-kodierter Data-URI " +
                     "(z.B. data:image/svg+xml;base64,...). Wird als {{logo_base64}} ins Template eingefügt.")]
        [ReadDefaultFromResource]
        public string LogoBase64 { get; set; }
    }
}
