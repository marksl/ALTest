using System.Collections.Generic;
using ALTest.Core.Configuration;

namespace ALTest.Core.FileSynchronization
{
    public interface ISyncedDestination
    {
        IList<AssemblyInfo> AssembliesWithFullPath { get; }
    }
}