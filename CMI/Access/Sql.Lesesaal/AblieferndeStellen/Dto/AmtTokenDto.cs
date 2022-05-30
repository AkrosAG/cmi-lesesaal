using System.Collections.Generic;

namespace CMI.Access.Sql.Lesesaal.AblieferndeStellen.Dto
{
    public class AmtTokenDto : AblieferndeStelleTokenDto
    {
        public List<AblieferndeStelleDto> AblieferndeStelleList { get; set; } = new List<AblieferndeStelleDto>();
    }
}