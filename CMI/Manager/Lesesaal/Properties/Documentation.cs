using CMI.Utilities.Common;

namespace CMI.Manager.Lesesaal.Properties
{
    public class Documentation : AbstractDocumentation
    {
        public override void LoadDescriptions()
        {
            AddDescription<DbConnectionSetting>(x => x.ConnectionString, "DB-Connectionstring zur Lesesaal DB");
            AddDescription<DbConnectionSetting>(x => x.ConnectionStringEF, "DB-Connectionstring zur Lesesaal DB im Entity-Framework Format");
        }
    }
}