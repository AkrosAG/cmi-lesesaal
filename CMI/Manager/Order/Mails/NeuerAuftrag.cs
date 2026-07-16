using System.ComponentModel;
using CMI.Contract.Parameter.AdditionalParameterTypes;
using CMI.Contract.Parameter.Attributes;

namespace CMI.Manager.Order.Mails
{
    public class NeuerAuftrag : EmailTemplate
    {
        [DefaultValue("")] public override string From { get; set; }

        [DefaultValue("archiv@library.ethz.ch")]
        public override string To { get; set; }

        [DefaultValue("")] public override string Cc { get; set; }

        [DefaultValue("")] public override string Bcc { get; set; }

        [DefaultValue("Neuer Auftrag: {{User.Vorname}} {{User.Name}}," +
                " {{#HasOrganization}}{{User.Organisation}}, {{/HasOrganization}}{{User.Ort}} / {{Bestellung.ErfassungsdatumMitUhrzeit}} / {{Bestellung.OrderId}}")]

        public override string Subject { get; set; }

        [ReadDefaultFromResource] public override string Body { get; set; }
    }
}