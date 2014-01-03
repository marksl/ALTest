using System.Collections.Generic;

namespace AltMstest.Core
{
    public interface ISyncedDestination
    {
        IList<string> AssemblyNames { get; }
        IList<string> AssembliesWithFullPath { get; }

        IList<string> GetAssembliesWithFullPath(IList<string> assemblyNames);
    }
}