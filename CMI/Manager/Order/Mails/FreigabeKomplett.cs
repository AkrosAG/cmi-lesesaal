using System.ComponentModel;
using CMI.Contract.Parameter.AdditionalParameterTypes;
using CMI.Contract.Parameter.Attributes;

namespace CMI.Manager.Order.Mails
{
    public class FreigabeKomplett : EmailTemplate
    {
        [DefaultValue("{{From}}")] public override string From { get; set; }

        [DefaultValue("{{To}}")] public override string To { get; set; }

        [DefaultValue("")] public override string Cc { get; set; }

        [DefaultValue("archiv@library.ethz.ch")]
        public override string Bcc { get; set; }

        [ReadDefaultFromResource] public override string Subject { get; set; }

        [ReadDefaultFromResource] public override string Body { get; set; }
    }
}