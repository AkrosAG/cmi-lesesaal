using CMI.Contract.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMI.Access.Harvest
{
    public interface IArchiveRecordProcessHandler
    {
        Task ProcessArchiveRecord(ArchiveRecord record);
    }
}
