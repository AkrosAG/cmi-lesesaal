using System.IO;

namespace CMI.Access.Common.Compiler
{
    public class CustomScriptLocator : IDynamicScriptLocator
    {
        public const string CustomScriptName = "CustomScript.cs";
        private readonly string rootPath;

        public CustomScriptLocator(string root)
        {
            rootPath = root;
        }
        
        public string LoadScriptByDefault()
        {
            var path = Path.Combine(rootPath, CustomScriptName);
            return File.ReadAllText(path);
        }
    }
}
