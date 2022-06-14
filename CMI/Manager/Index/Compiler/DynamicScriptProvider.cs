using System;
using System.Linq;
using System.CodeDom.Compiler;
using System.IO;

using Microsoft.CSharp;

namespace CMI.Manager.Index.Compiler
{
    public class DynamicScriptProvider : IDynamicScriptProvider
    {
        private readonly IDynamicScriptLocator scriptLocator;

        public static string[] References => AppDomain.CurrentDomain.GetAssemblies()
                                                                     .Where(a => !a.IsDynamic)
                                                                     .Select(a => $"{Path.GetFileName(a.Location)}").ToArray();

        public DynamicScriptProvider(IDynamicScriptLocator scriptLocator)
        {
            this.scriptLocator = scriptLocator;
        }

        public T GetInstanceByType<T>() 
        {
            string script = scriptLocator.LoadScriptByDefault();

            using (var compiler = new CSharpCodeProvider())
            {
                var options = new CompilerParameters() { GenerateInMemory = true };
                options.ReferencedAssemblies.AddRange(DynamicScriptProvider.References);
                
                var result = compiler.CompileAssemblyFromSource(options, script);
                EnsureResult(result);

                var targetType = result.CompiledAssembly.GetTypes()
                                                    .Where(t => typeof(T).IsAssignableFrom(t))
                                                    .FirstOrDefault();

                return (T) Activator.CreateInstance(targetType); 
            }
        }

        private void EnsureResult(CompilerResults result)
        {
            if(result.Errors.Count > 0)
            {
                var messages = result.Errors.Cast<CompilerError>().Select(e => $"{e.Line}:{e.ErrorNumber}|{e.ErrorText}");
                throw new Exception(String.Join(";",messages));
            }
        }
    }
}