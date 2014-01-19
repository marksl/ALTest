using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ALTest.Core
{
    internal static class TestFactoryLoader
    {
        public static ITestFactory Load(string assembly)
        {
            // This is where I should do the error checking and display a helpful warning message.

            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), assembly);

            try
            {
                StdOut.WriteLine("Loading...");
                Assembly ass = Assembly.LoadFile(fullPath);
                if (ass == null)
                    throw new InvalidDataException(string.Format("Assembly [{0}] does not exist.", assembly));

                var testFactory = ass.GetTypes().FirstOrDefault(x => typeof (ITestFactory).IsAssignableFrom(x));
                if (testFactory == null)
                    throw new InvalidDataException("Failed to find any classes inheritting from ITestFactory.");

                return (ITestFactory) Activator.CreateInstance(testFactory);
            }
            catch(ReflectionTypeLoadException ex)
            {
                StdOut.WriteLine(ex.ToString());
                StdOut.WriteLine("Loader Exceptions:");
                foreach (var e in ex.LoaderExceptions)
                {
                    StdOut.WriteLine(e.ToString());    
                }
                
                throw;
            }
            
        }
    }
}
