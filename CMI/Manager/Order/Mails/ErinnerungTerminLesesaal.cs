using System.ComponentModel;
using CMI.Contract.Parameter.AdditionalParameterTypes;
using CMI.Contract.Parameter.Attributes;

namespace CMI.Manager.Order.Mails
{
    public class ErinnerungTerminLesesaal : EmailTemplate
    {
        [DefaultValue("archiv@library.ethz.ch")]
        public override string From { get; set; }

        [DefaultValue("")] public override string To { get; set; }

        [DefaultValue("")] public override string Cc { get; set; }

        [DefaultValue("archiv@library.ethz.ch")]
        public override string Bcc { get; set; }

        [DefaultValue("{{#Sprachen}}" +
                      "{{#IstDeutsch}}" +
                      "Erinnerung Bereitstellung" +
                      "{{/IstDeutsch}}" +
                      "{{#IstFranzösisch}}" +
                      "Rappel réservation de documents" +
                      "{{/IstFranzösisch}}" +
                      "{{#IstItalienisch}}" +
                      "Documenti pronti per la consultazione – promemoria" +
                      "{{/IstItalienisch}}" +
                      "{{#IstEnglisch}}" +
                      "Reminder: Documents ready" +
                      "{{/IstEnglisch}}" +
                      "{{/Sprachen}}")]
        public override string Subject { get; set; }

        [ReadDefaultFromResource] public override string Body { get; set; }
    }
}
