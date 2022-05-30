using System.Data.SqlClient;
using CMI.Access.Sql.Lesesaal.AblieferndeStellen.Dto;

namespace CMI.Access.Sql.Lesesaal.AblieferndeStellen
{
    public static class AblieferndeStelleExtensions
    {
        public static T ToAblieferndeStelle<T>(this SqlDataReader reader) where T : AblieferndeStelleDto, new()
        {
            var ablieferndeStelle = new T();

            reader.PopulateProperties(ablieferndeStelle);


            return ablieferndeStelle;
        }
    }
}