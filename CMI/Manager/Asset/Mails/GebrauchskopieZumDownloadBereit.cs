using System.ComponentModel;
using CMI.Contract.Parameter.AdditionalParameterTypes;
using CMI.Contract.Parameter.Attributes;

namespace CMI.Manager.Asset.Mails
{
    public class GebrauchskopieZumDownloadBereit : EmailTemplate
    {
        [DefaultValue("{{#User.IstDeutsch}}" +
                      "archiv@library.ethz.ch" +
                      "{{/User.IstDeutsch}}" +
                      "{{^User.IstDeutsch}}" +
                      "archiv@library.ethz.ch" +
                      "{{/User.IstDeutsch}}")]
        public override string From { get; set; }

        [DefaultValue("{{User.EmailAddress}}")]
        public override string To { get; set; }

        [DefaultValue("")] public override string Cc { get; set; }

        [DefaultValue("")] public override string Bcc { get; set; }

        [DefaultValue("{{#User.IstDeutsch}}" +
                      "Unterlagen für Download bereit" +
                      "{{/User.IstDeutsch}}" +
                      "{{#User.IstFranzösisch}}" +
                      "Vos documents sont prêts à être téléchargés" +
                      "{{/User.IstFranzösisch}}" +
                      "{{#User.IstItalienisch}}" +
                      "Documenti pronti per il download" +
                      "{{/User.IstItalienisch}}" +
                      "{{#User.IstEnglisch}}" +
                      "Documents ready for download" +
                      "{{/User.IstEnglisch}}" +
                      " ({{Ve.Signatur}})")]
        public override string Subject { get; set; }

        [ReadDefaultFromResource] public override string Body { get; set; }
    }
}