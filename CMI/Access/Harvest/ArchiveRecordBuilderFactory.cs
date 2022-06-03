using System;

namespace CMI.Access.Harvest
{
    public class ArchiveRecordBuilderFactory : IArchiveRecordBuilderFactory
    {
        private readonly IAISDataProvider aisDataProvider;
        private readonly LanguageSettings languageSettings;
        private readonly ApplicationSettings applicationSettings;
        private readonly CachedLookupData cachedLookupData;
        private readonly IArchiveRecordProcessHandler processHandler;
        
        public ArchiveRecordBuilderFactory(IAISDataProvider aisDataProvider,
                                           IArchiveRecordProcessHandler processHandler,
                                           LanguageSettings languageSettings,
                                           ApplicationSettings applicationSettings,
                                           CachedLookupData cachedLookupData)
        {
            this.aisDataProvider = aisDataProvider;
            this.languageSettings = languageSettings;
            this.applicationSettings = applicationSettings;
            this.processHandler = processHandler;
            this.cachedLookupData = cachedLookupData;
        }

        public IArchiveRecordBuilder Create()
        {
            IArchiveRecordBuilder archiveRecordBuilder;
            switch (Properties.Settings.Default.AisProvider.ToLowerInvariant())
            {
                case "cmiais":
                    archiveRecordBuilder = new CMIAIS.CMIAISArchiveRecordBuilder((CMIAIS.CMIAISDataProvider)aisDataProvider, (CMIAIS.CMIAISDataProvider)aisDataProvider, languageSettings, processHandler);
                    break;
                case "scopeais":
                    archiveRecordBuilder = new ScopeArchiv.ScopeArchiveRecordBuilder((ScopeArchiv.ScopeAISDataProvider) aisDataProvider, languageSettings, applicationSettings, cachedLookupData);
                    break;
                default:
                    throw new NotImplementedException($"{Properties.Settings.Default.AisProvider} is not a supported AisProvider");
            }

            return archiveRecordBuilder;
        }
    }
}