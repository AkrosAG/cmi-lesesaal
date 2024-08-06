using CMI.Utilities.Common;

namespace CMI.Access.Common.Properties
{
    public class Documentation : AbstractDocumentation
    {
        public override void LoadDescriptions()
        {
            AddDescription<Settings>(x => x.ElasticSearchUrl, "URL zum Elastic Search");
            AddDescription<Settings>(x => x.ElasticIndexName, "Name Elastic Index");
            AddDescription<Settings>(x => x.LogstashName, "Name Logstash Name");
        }
    }
}