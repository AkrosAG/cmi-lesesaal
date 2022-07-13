using CMI.Contract.Common.Compiler;
using System.IO;

namespace CMI.Manager.Harvest
{
    public class HarvestCustomScriptLocator : IDynamicScriptLocator
    {
        public const string CustomScriptName = "HarvestCustomScript.cs";
        private readonly string rootPath;

        public HarvestCustomScriptLocator(string path)
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
