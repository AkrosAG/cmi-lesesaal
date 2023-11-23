using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using CMI.Contract.Parameter;
using Castle.Components.DictionaryAdapter.Xml;
using CMI.Contract.Common;

namespace CMI.Web.Frontend.ParameterSettings
{
    public class FrontendDynamicTextSettings : ISetting
    {

        #region Lieferart Auswahl 'Digital erhalten'

        [Description("Definiert den Text in der Lieferart Auswahl 'Digital erhalten'. Sprache: DE")]
        [DefaultValue("digital erhalten und stelle einen Digitalisierungsantrag.")]
        public string DeliveryTypeDigitalDE { get; set; }

        [Description("Definiert den Text in der Lieferart Auswahl 'Digital erhalten'. Sprache: FR")]
        [DefaultValue("Livraison <strong>numérique</strong>. Vous recevrez le dossier numérisé dans un délai d'environ 30 jours. Vous trouverez toutes les informations concernant la numérisation dans la rubrique <a href=\"https://www.recherche.bar.admin.ch/recherche/#/fr/informations/commande-et-consultation\" target=\"_blank\" rel=\"noopener noreferrer\">Commande et consultation</a>.")]
        public string DeliveryTypeDigitalFR { get; set; }

        [Description("Definiert den Text in der Lieferart Auswahl 'Digital erhalten'. Sprache: IT")]
        [DefaultValue("in forma <strong>digitale</strong>. Riceverai il dossier digitalizzato tra circa 30 giorni. Tutte le informazioni riguardanti la digitalizzazione le trovi alla voce <a href=\"https://www.recherche.bar.admin.ch/recherche/#/it/informazioni/ordinazione-e-consultazione\" target=\"_blank\" rel=\"noopener noreferrer\">Ordinazione e consultazione</a>.")]
        public string DeliveryTypeDigitalIT { get; set; }

        [Description("Definiert den Text in der Lieferart Auswahl 'Digital erhalten'. Sprache: EN")]
        [DefaultValue("receive scans of these documents and submit a digitisation request.")]
        public string DeliveryTypeDigitalEN { get; set; }

        [Description("Definiert die Warnmeldung die ausgegeben wird, wenn der Benutzer die Option 'Digitalisierungsauftrag' wählt. Sprache: IT")]
        [DefaultValue("<h4 class=\"alert-heading\">Please note</h4><p>Digitization orders are chargeable.</p>")]
        public string DigitalOrderWarningTextIT { get; set; }

        [Description("Definiert die Warnmeldung die ausgegeben wird, wenn der Benutzer die Option 'Digitalisierungsauftrag' wählt. Sprache: FR")]
        [DefaultValue("<h4 class=\"alert-heading\">Please note</h4><p>Digitization orders are chargeable.</p>")]
        public string DigitalOrderWarningTextFR { get; set; }

        [Description("Definiert die Warnmeldung die ausgegeben wird, wenn der Benutzer die Option 'Digitalisierungsauftrag' wählt. Sprache: EN")]
        [DefaultValue("Requests for digitisation may be rejected by the archive. The digitisation of documents is subject to a fee <a href =\"https://ethz.ch/content/dam/ethz/associates/ethlibrary-dam/documents/Benutzungsbestimmungen/f_library_gebuehrenblatt_2020_en.pdf\" target =\"_blank\" rel=\"noopener noreferrer\">(ETH Library fees)</a>. You will receive a price calculation in advance.")]
        public string DigitalOrderWarningTextEN { get; set; }

        [Description("Definiert die Warnmeldung die ausgegeben wird, wenn der Benutzer die Option 'Digitalisierungsauftrag' wählt. Sprache: DE")]
        [DefaultValue("Ein Digitalisierungsantrag kann durch das Archiv abgelehnt werden. Die Digitalisierung von Unterlagen ist kostenpflichtig <a href=\"https://ethz.ch/content/dam/ethz/associates/ethlibrary-dam/documents/Benutzungsbestimmungen/f_library_gebuehrenblatt_2020_de.pdf\" target =\"_blank\" rel=\"noopener noreferrer\">(Gebührenordnung der ETH-Bibliothek)</a>. Sie bekommen vorgängig eine Offerte zugestellt.")]
        public string DigitalOrderWarningTextDE { get; set; }

        #endregion


        #region Lieferart Auswahl 'In den Lesesaal bestellen'

        [Description("Definiert den Text in der Lieferart Auswahl 'In den Lesesaal bestellen'. Sprache: DE")]
        [DefaultValue("zur Konsultation in den <strong>Lesesaal</strong> bestellen (Bestellungen müssen mindestens 2 Arbeitstage im Voraus erfolgen).")]
        public string DeliveryTypeReadingRoomDE { get; set; }

        [Description("Definiert den Text in der Lieferart Auswahl 'In den Lesesaal bestellen'. Sprache: FR")]
        [DefaultValue("Consultation en <strong>salle de lecture</strong>. Prière de commander 24h à l’avance pour recevoir les documents le jour désiré (mardi, mercredi et jeudi).")]
        public string DeliveryTypeReadingRoomFR { get; set; }

        [Description("Definiert den Text in der Lieferart Auswahl 'In den Lesesaal bestellen'. Sprache: IT")]
        [DefaultValue("per consultazione nelle <strong>sale di lettura</strong>. Se la richiesta è effettuata 24 ore prima, i documenti sono disponibili il giorno desiderato nelle sale di lettura (martedì, mercoledì e giovedì).")]
        public string DeliveryTypeReadingRoomIT { get; set; }

        [Description("Definiert den Text in der Lieferart Auswahl 'In den Lesesaal bestellen'. Sprache: EN")]
        [DefaultValue("order these documents for consultation in the <strong>reading room</strong>. (Orders must be placed at least 2 workdays in advance)")]
        public string DeliveryTypeReadingRoomEN { get; set; }

        #endregion


        #region Lieferart Auswahl 'Ins Amt bestellen'

        [Description("Definiert den Text in der Lieferart Auswahl 'Ins Amt bestellen'. Sprache: DE")]
        [DefaultValue("ins <strong>Amt</strong> bestellen (Lieferfrist: ein bis zwei Arbeitstage)")]
        public string DeliveryTypeCommissionDE { get; set; }

        [Description("Definiert den Text in der Lieferart Auswahl 'Ins Amt bestellen'. Sprache: FR")]
        [DefaultValue("Livraison dans votre <strong>service</strong> (délai de livraison: un à deux jours ouvrés)")]
        public string DeliveryTypeCommissionFR { get; set; }

        [Description("Definiert den Text in der Lieferart Auswahl 'Ins Amt bestellen'. Sprache: IT")]
        [DefaultValue("per consultazione nell'<strong>unità amministrativa</strong> (termine di consegna: 1-2 giorni lavorativi)")]
        public string DeliveryTypeCommissionIT { get; set; }

        [Description("Definiert den Text in der Lieferart Auswahl 'Ins Amt bestellen'. Sprache: EN")]
        [DefaultValue("for delivery to the <strong>office</strong> (delivery takes one to two working days)")]
        public string DeliveryTypeCommissionEN { get; set; }

        #endregion
    }
}
