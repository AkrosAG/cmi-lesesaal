namespace CMI.Access.Harvest.CMIAIS.Schemas
{

    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.cmiag.ch/cdws/searchDetailResponse")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.cmiag.ch/cdws/searchDetailResponse", IsNullable = false)]
    public partial class SearchDetailResponse
    {

        private SearchDetailResponseHit hitField;

        private byte iDXSEQField;

        private string qField;

        private string lField;

        private byte sField;

        private ushort mField;

        private byte numHitsField;

        private string indexNameField;

        /// <remarks/>
        public SearchDetailResponseHit Hit
        {
            get
            {
                return this.hitField;
            }
            set
            {
                this.hitField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte IDXSEQ
        {
            get
            {
                return this.iDXSEQField;
            }
            set
            {
                this.iDXSEQField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string q
        {
            get
            {
                return this.qField;
            }
            set
            {
                this.qField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string l
        {
            get
            {
                return this.lField;
            }
            set
            {
                this.lField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte s
        {
            get
            {
                return this.sField;
            }
            set
            {
                this.sField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ushort m
        {
            get
            {
                return this.mField;
            }
            set
            {
                this.mField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte numHits
        {
            get
            {
                return this.numHitsField;
            }
            set
            {
                this.numHitsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string indexName
        {
            get
            {
                return this.indexNameField;
            }
            set
            {
                this.indexNameField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.cmiag.ch/cdws/searchDetailResponse")]
    public partial class SearchDetailResponseHit
    {

        private SearchDetailResponseHitSnippet snippetField;

        private Verzeichnungseinheit verzeichnungseinheitField;

        private string guidField;

        private byte sEQField;

        private decimal relevanceField;

        /// <remarks/>
        public SearchDetailResponseHitSnippet Snippet
        {
            get
            {
                return this.snippetField;
            }
            set
            {
                this.snippetField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.cmiag.ch/cdws/ArchiveRecord")]
        public Verzeichnungseinheit Verzeichnungseinheit
        {
            get
            {
                return this.verzeichnungseinheitField;
            }
            set
            {
                this.verzeichnungseinheitField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Guid
        {
            get
            {
                return this.guidField;
            }
            set
            {
                this.guidField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte SEQ
        {
            get
            {
                return this.sEQField;
            }
            set
            {
                this.sEQField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal Relevance
        {
            get
            {
                return this.relevanceField;
            }
            set
            {
                this.relevanceField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.cmiag.ch/cdws/searchDetailResponse")]
    public partial class SearchDetailResponseHitSnippet
    {

        private string emField;

        /// <remarks/>
        public string EM
        {
            get
            {
                return this.emField;
            }
            set
            {
                this.emField = value;
            }
        }
    }
}
