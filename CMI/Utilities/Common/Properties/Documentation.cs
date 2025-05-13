namespace CMI.Utilities.Common.Properties
{
    public class Documentation : AbstractDocumentation
    {
        public override void LoadDescriptions()
        {
            AddDescription<Settings>(x => x.AsposeLicense, "License key used by Aspose components for document processing.");
        }
    }
}
