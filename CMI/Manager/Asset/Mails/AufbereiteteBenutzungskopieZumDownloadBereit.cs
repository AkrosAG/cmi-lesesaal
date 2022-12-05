using System.ComponentModel;
using CMI.Contract.Parameter.AdditionalParameterTypes;
using CMI.Contract.Parameter.Attributes;

namespace CMI.Manager.Asset.Mails
{
    /// <summary>
    ///     Vorlage 059
    /// </summary>
    public class AufbereiteteBenutzungskopieZumDownloadBereit : EmailTemplate
    {
        [DefaultValue("archiv@library.ethz.ch")] 
        public override string From { get; set; }

        [DefaultValue("archiv@library.ethz.ch")]
        public override string To { get; set; }

        [DefaultValue("")] public override string Cc { get; set; }

        [DefaultValue("")] public override string Bcc { get; set; }

        [DefaultValue(
            "Gebrauchskopie zum Download bereit{{#Aufträge}}{{#BestellteVe.Signatur}} ({{BestellteVe.Signatur}}){{/BestellteVe.Signatur}}{{/Aufträge}}")]
        public override string Subject { get; set; }

        [ReadDefaultFromResource] public override string Body { get; set; }
    }
}