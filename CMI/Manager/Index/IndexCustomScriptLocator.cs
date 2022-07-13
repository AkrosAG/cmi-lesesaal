using CMI.Contract.Common.Compiler;
using System.IO;

namespace CMI.Manager.Index
{
    public class IndexCustomScriptLocator : IDynamicScriptLocator
    {
        public const string CustomScriptName = "IndexCustomScript.cs";
        private readonly string rootPath;

        public IndexCustomScriptLocator(string path)
        {
            rootPath = path;
        }

        public string GetCustomScript()
        {
            var path = Path.Combine(rootPath, CustomScriptName);
            return File.ReadAllText(path);
        }
    }
}
