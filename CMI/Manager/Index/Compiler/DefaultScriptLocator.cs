using System.IO;

namespace CMI.Manager.Index.Compiler
{
    public class DefaultScriptLocator : IDynamicScriptLocator
    {
        public const string DefaultScriptName = "CustomScript.cs";
        
        public string LoadScriptByDefault()
        {
            return File.ReadAllText(DefaultScriptName);
        }
    }
}
