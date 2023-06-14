using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMI.Web.Common.Auth.Sibboleth.Types
{
    /// <summary>
    /// Interface for defining a Shibboleth Attribute
    /// </summary>
    public interface IShibbolethAttribute
    {
        string Name { get; set; }
        string Id { get; set; }
    }
}
