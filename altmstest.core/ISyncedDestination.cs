using System.Collections.Generic;

namespace AltMstest.Core
{
    public interface ISyncedDestination
    {
        IList<string> AssembliesWithFullPath { get; }
    }
}