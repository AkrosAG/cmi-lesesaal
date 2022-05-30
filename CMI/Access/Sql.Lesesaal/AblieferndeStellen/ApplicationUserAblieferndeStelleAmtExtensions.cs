using System.Data.SqlClient;
using CMI.Access.Sql.Lesesaal.AblieferndeStellen.Dto;

namespace CMI.Access.Sql.Lesesaal.AblieferndeStellen
{
    public static class ApplicationUserAblieferndeStelleAmtExtensions
    {
        public static ApplicationUserAblieferndeStelleAmtDto ToApplicationUserAblieferndeStelleAmt(
            this SqlDataReader reader)
        {
            var ablieferndeStelle = new ApplicationUserAblieferndeStelleAmtDto();
            reader.PopulateProperties(ablieferndeStelle);

            return ablieferndeStelle;
        }
    }
}