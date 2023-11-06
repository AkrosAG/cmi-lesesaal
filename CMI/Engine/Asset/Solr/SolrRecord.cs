using SolrNet.Attributes;

namespace CMI.Engine.Asset.Solr
{
    public class SolrRecord
    {
        [SolrUniqueKey("id")]
        public string Id { get; set; }

        [SolrField("source")]
        public string Source { get; set; }

        [SolrField("ocr_text")] 
        public string OCRText { get; set; }

        [SolrField("title")]
        public string Title { get; set; }
    }
}
