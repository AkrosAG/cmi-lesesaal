using System.IO;

namespace CMI.Contract.Common.Compiler
{
    public class CustomScriptLocator : IDynamicScriptLocator
    {
        public const string CustomScriptName = "CustomScript.cs";
        private readonly string rootPath;

        public CustomScriptLocator(string path)
        {
            rootPath = path;
        }


        public string LoadScriptByDefault()
        {
            var path = Path.Combine(rootPath, CustomScriptName);
            return File.ReadAllText(path);
        }
    }
}
