using System;
using System.Linq;
using System.CodeDom.Compiler;
using System.IO;

using Microsoft.CSharp;

namespace CMI.Contract.Common.Compiler
{
    public class DynamicScriptProvider : IDynamicScriptProvider
    {
        private readonly IDynamicScriptLocator scriptLocator;
        private readonly CSharpCodeProvider compiler;

        public static string[] References => AppDomain.CurrentDomain.GetAssemblies()
                                                                     .Where(a => !a.IsDynamic)
                                                                     .Select(a => $"{a.Location}").ToArray();

        public DynamicScriptProvider(CSharpCodeProvider compilerInstance, IDynamicScriptLocator scriptLocatorInstance)
        {
            scriptLocator = scriptLocatorInstance;
            compiler = compilerInstance;
        }

        public T GetInstanceByType<T>() 
        {
            string script = scriptLocator.GetCustomScript();
            var options = new CompilerParameters() { GenerateInMemory = true };
            foreach(var reference in DynamicScriptProvider.References)
            {
                options.ReferencedAssemblies.Add(reference);
            }
                
            var result = compiler.CompileAssemblyFromSource(options, script);
            EnsureResult(result);

            var targetType = result.CompiledAssembly.GetTypes()
                                                .Where(t => typeof(T).IsAssignableFrom(t))
                                                .FirstOrDefault();

            return (T) Activator.CreateInstance(targetType); 
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
    public class EmptyScriptLocator : IDynamicScriptLocator
    {
        public string GetCustomScript()
        {
            return @"using System.Collections.Generic;
                    namespace CMI.Contract.Common.Compiler
                    {
                        public class DefaultCustomScript : IDynamicScript
                        {
                            public void PostProcessArchiveRecord(ArchiveRecord archiveRecord){}

                            public void PostProcessElasticArchiveRecord(ElasticArchiveRecord elasticArchiveRecord, ArchiveRecord archiveRecord){}
                            }
                    }";
        }
    }
}