using System.IO;

namespace CMI.Manager.Index.Compiler
{
    public class CustomScriptLocator : IDynamicScriptLocator
    {
        public const string CustomScriptName = "CustomScript.cs";
        
        public string LoadScriptByDefault()
        {
            return File.ReadAllText(CustomScriptName);
        }
    }
}
