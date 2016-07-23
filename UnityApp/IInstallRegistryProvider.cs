using System.Collections.Generic;

namespace Koinonia
{
    public interface IInstallRegistryProvider
    {
        IEnumerable<Install> Installs { get; }
        void AddInstall(Install inst);
        void RemoveInstall(Install inst);
        void Commit();
    }
}