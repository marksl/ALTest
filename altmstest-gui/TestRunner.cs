using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AltMstestGui
{
    [Serializable]
    public class TestRunner : MarshalByRefObject
    {
        public void RunTests(string assembly)
        {
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;

            Assembly ass = Assembly.LoadFile(assembly);

            try
            {
                // Get all classes.. no abstract classes.
                var types = ass.GetTypes().Where(t => t.IsClass && !t.IsAbstract);
                foreach (var type in types)
                {
                    if (type != null)
                    {

                    }
                }
            }
            catch (Exception ex)
            {
                
                throw;
            }
            
        }

        private Assembly AssemblyResolve(object sender, ResolveEventArgs args)
        {
            int firstComma = args.Name.IndexOf(',');

            string dll = args.Name.Substring(0, firstComma) + ".dll";

            var fileInfo = new FileInfo(args.RequestingAssembly.Location);

            var fullPath = Path.Combine(fileInfo.DirectoryName, dll);
            return Assembly.LoadFile(fullPath);
        }
    }
}
