using CMI.Contract.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMI.Access.Harvest.CMIAIS
{
    public class CMIAISArchiveRecordDisplayProvider : IArchiveRecordDisplayProvider
    {
        public async Task<ArchiveRecordDisplay> GetDisplayData(string recordId)
        {
            // TODO: Review
            return await Task.FromResult(new ArchiveRecordDisplay());
        }
    }
}
