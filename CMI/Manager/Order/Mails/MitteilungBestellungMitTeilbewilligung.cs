using System.ComponentModel;
using CMI.Contract.Parameter.AdditionalParameterTypes;
using CMI.Contract.Parameter.Attributes;

namespace CMI.Manager.Order.Mails
{
    public class MitteilungBestellungMitTeilbewilligung : EmailTemplate
    {
        [DefaultValue("")] public override string From { get; set; }

        [DefaultValue("archiv@library.ethz.ch")]
        public override string To { get; set; }

        [DefaultValue("")] public override string Cc { get; set; }

        [DefaultValue("")] public override string Bcc { get; set; }

        [DefaultValue("Auftrag abgebrochen: Teilbewilligung {{User.Vorname}} {{User.Name}}")]
        public override string Subject { get; set; }

        [ReadDefaultFromResource] public override string Body { get; set; }
    }
}