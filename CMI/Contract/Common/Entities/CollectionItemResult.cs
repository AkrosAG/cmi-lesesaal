using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CMI.Contract.Common.Entities
{
    public class CollectionItemResult
    {
        public CollectionDto Item { get; set; }
        public Dictionary<int, string> Breadcrumb { get; set; }
    }
}