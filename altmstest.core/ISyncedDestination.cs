using System.Collections.Generic;
using AltMstest.Core.Configuration;

namespace AltMstest.Core
{
    public interface ISyncedDestination
    {
        IList<AssemblyInfo> AssembliesWithFullPath { get; }
    }
}