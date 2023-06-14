using CMI.Utilities.Common;

namespace CMI.Access.Harvest.Properties
{
    public class Documentation : AbstractDocumentation
    {
        public override void LoadDescriptions()
        {
            AddDescription<Settings>(x => x.DefaultLanguage, "Default Sprache des Connection String zur Oracle DB des AIS");
            AddDescription<Settings>(x => x.DigitalRepositoryElementIdentifier, "ElementIdentifier des Connection String zur Oracle DB des AIS");
            AddDescription<Settings>(x => x.OracleHost, "Host des Connection Strings zur Oracle DB des AIS");
            AddDescription<Settings>(x => x.OraclePassword, "Passwort des Connection Strings zur Oracle DB des AIS");
            AddDescription<Settings>(x => x.OraclePort, "Port des Connection Strings zur Oracle DB des AIS");
            AddDescription<Settings>(x => x.OracleSchemaName, "Schema des Connection Strings zur Oracle DB des AIS");
            AddDescription<Settings>(x => x.OracleSID,
                "SID des Connection Strings zur Oracle DB des AIS.  Es muss entweder die SID oder der Service Name spezifiziert sein.");
            AddDescription<Settings>(x => x.OracleServiceName,
                "Service Name des Connection Strings zur Oracle DB des AIS. Es muss entweder die SID oder der Service Name spezifiziert sein.");
            AddDescription<Settings>(x => x.OracleUser, "Benutzer des Connection Strings zur Oracle DB des AIS");
            AddDescription<Settings>(x => x.OutputSQLExecutionTimes, "SQL Execution Time Einstellung des Connection Strings zur Oracle DB des AIS");
            AddDescription<Settings>(x => x.SupportedLanguages, "Unterstützte Sprachen des Connection Strings zur Oracle DB des AIS");
            AddDescription<Settings>(x => x.AisProvider, "The type of AIS that should be used. Either CMIAIS or scopeAIS");
            AddDescription<Settings>(x => x.CdwsEndpoint, "The url of the CDWS Endpoint eg http://<url>:10003/cdws/Index/");
            AddDescription<Settings>(x => x.CdwsIndexName, "Name des CDWS Daten Indexs");
            AddDescription<Settings>(x => x.CdwsTectonicIndexName, "Name des CDWS Tectonic Indexs");
        }
    }
}