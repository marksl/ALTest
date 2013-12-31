using System.Collections.Generic;

namespace AltMstestGui
{
    public interface ISyncedDestination
    {
        IList<string> AssembliesWithFullPath { get; }
    }
}