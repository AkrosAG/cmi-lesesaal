using CMI.Utilities.Common;

namespace CMI.Manager.Harvest.Properties
{
    public class Documentation : AbstractDocumentation
    {
        public override void LoadDescriptions()
        {
            AddDescription<Settings>(x => x.CustomScriptPath, "Vollständiger Pfad für CustomScript");
            AddDescription<Settings>(x => x.CustomScriptPath, "Vollständiger Pfad für CustomScript");
            AddDescription<DbConnectionSetting>(x => x.ConnectionStringEF, "DB-Connectionstring zur Lesesaal DB im Entity-Framework Format");
        }
    }
}