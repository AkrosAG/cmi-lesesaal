using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMI.Manager.Repository.Systems.Rosetta.Schema
{
    public static class DivTypeExtension
    {
        public static bool IsFileNode(this DivType node)
        {
            return string.Equals(node.TYPE, "FILE", StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool IsFolderNode(this DivType node)
        {
            return string.Equals(node.TYPE, "FOLDER", StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool IsEmptyTypeNode(this DivType node)
        {
            return string.IsNullOrEmpty(node.TYPE);
        }

        public static bool HasSubNodes(this DivType node)
        {
            return node.Div.Any();
        }
    }
}
