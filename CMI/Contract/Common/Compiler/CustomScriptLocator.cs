using System.IO;

namespace CMI.Contract.Common.Compiler
{
    public class CustomScriptLocator : IDynamicScriptLocator
    {
        private readonly string path;

        public CustomScriptLocator(string path)
        {
            this.path = path;
        }

        public string GetCustomScript()
        {
            return File.ReadAllText(path);
        }
    }
}
