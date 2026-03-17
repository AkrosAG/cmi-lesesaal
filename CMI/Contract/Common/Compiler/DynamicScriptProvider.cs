using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Linq;
using System.Reflection;

namespace CMI.Contract.Common.Compiler
{
    public class DynamicScriptProvider : IDynamicScriptProvider
    {
        private readonly IDynamicScriptLocator scriptLocator;
        private readonly CSharpCodeProvider compiler;

        private Assembly cachedAssembly;
        private string cachedScriptHash;
        private readonly object @lock = new object();

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

            string scriptHash = ComputeHash(script);

            lock (@lock)
            {
                // Nur neu kompilieren, wenn sich das Script geändert hat
                if (cachedAssembly == null || cachedScriptHash != scriptHash)
                {
                    var options = new CompilerParameters() { GenerateInMemory = true };
                    foreach (var reference in References)
                    {
                        options.ReferencedAssemblies.Add(reference);
                    }

                    var result = compiler.CompileAssemblyFromSource(options, script);
                    EnsureResult(result);

                    cachedAssembly = result.CompiledAssembly;
                    cachedScriptHash = scriptHash;
                }
            }

            var targetType = cachedAssembly
                .GetTypes()
                .FirstOrDefault(t => typeof(T).IsAssignableFrom(t));

            return (T) Activator.CreateInstance(targetType); 
        }

        private void EnsureResult(CompilerResults result)
        {
            if(result.Errors.Count > 0)
            {
                var messages = result.Errors.Cast<CompilerError>().Select(e => $"{e.ToString()}");
                throw new Exception(String.Join(Environment.NewLine, messages));
            }
        }

        private static string ComputeHash(string input)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            var bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
            return Convert.ToBase64String(bytes);
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