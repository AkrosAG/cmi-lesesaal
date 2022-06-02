using System;

namespace CMI.Access.Harvest
{
    public class ArchiveRecordBuilderFactory : IArchiveRecordBuilderFactory
    {
        private readonly IAISDataProvider aisDataProvider;
        private readonly LanguageSettings languageSettings;
        private readonly ApplicationSettings applicationSettings;
        private readonly CachedLookupData cachedLookupData;
        private readonly IArchiveRecordSecurityProvider securityProvider;
        private readonly IArchiveRecordDisplayProvider displayProvider;

        public ArchiveRecordBuilderFactory(IAISDataProvider aisDataProvider,
                                           IArchiveRecordSecurityProvider securityProvider,
                                           IArchiveRecordDisplayProvider displayProvider,
                                           LanguageSettings languageSettings,
                                           ApplicationSettings applicationSettings,
                                           CachedLookupData cachedLookupData)
        {
            this.aisDataProvider = aisDataProvider;
            this.languageSettings = languageSettings;
            this.applicationSettings = applicationSettings;
            this.securityProvider = securityProvider;
            this.displayProvider = displayProvider;
            this.cachedLookupData = cachedLookupData;
        }

        public IArchiveRecordBuilder Create()
        {
            IArchiveRecordBuilder archiveRecordBuilder;
            switch (Properties.Settings.Default.AisProvider.ToLowerInvariant())
            {
                case "cmiais":
                    archiveRecordBuilder = new CMIAIS.CMIAISArchiveRecordBuilder((CMIAIS.CMIAISDataProvider)aisDataProvider, securityProvider, displayProvider, languageSettings);
                    break;
                case "scopeais":
                    archiveRecordBuilder = new ScopeArchiv.ScopeArchiveRecordBuilder((ScopeArchiv.ScopeAISDataProvider) aisDataProvider, securityProvider, languageSettings, applicationSettings, cachedLookupData);
                    break;
                default:
                    throw new NotImplementedException($"{Properties.Settings.Default.AisProvider} is not a supported AisProvider");
            }

            return archiveRecordBuilder;
        }
    }
}