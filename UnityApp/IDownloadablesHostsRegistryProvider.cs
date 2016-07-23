using System.Collections.Generic;

namespace Koinonia
{
    public interface IDownloadablesHostsRegistryProvider
    {
        IEnumerable<DownloadablesHost> DownloadablesHosts { get; } 
    }
}