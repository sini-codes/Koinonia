using System.Collections.Generic;
using System.IO;

namespace Koinonia
{
    public class Uninstallation
    {
        public Uninstallation(IDownloadablesHostsRegistryProvider hostsRegistry, IInstallRegistryProvider installRegistry, IGithubApi githubApi, IKoinoniaLogger logger)
        {
            _hostsRegistry = hostsRegistry;
            _installsRegistry = installRegistry;
            _githubApi = githubApi;
            _logger = logger;
        }

        private IDownloadablesHostsRegistryProvider _hostsRegistry;

        private void Add(Install toUninstall)
        {
            _logger.Log("Adding " + toUninstall.ToShortString() + " to the deinstall queue");

            UninstallPlan.Add(toUninstall);

        }

        public List<Install> UninstallPlan
        {
            get { return _uninstallPlan ?? (_uninstallPlan = new List<Install>()); }
            set { _uninstallPlan = value; }
        }

        private IInstallRegistryProvider _installsRegistry;
        private IGithubApi _githubApi;
        private IKoinoniaLogger _logger;
        private List<Install> _uninstallPlan;

        public InstallationResult Uninstall(Install install)
        {
            try
            {
                Install_Internal(install);
            }
            catch (ConfigDataNotFoundException ex)
            {
                _logger.LogProblem(ex.Message);
                return new InstallationResult()
                {
                    Success = false
                };
            }
            return new InstallationResult()
            {
                Success = true
            };
        }


        private void Install_Internal(Install install)
        {

            Add(install);

            foreach (var entry in UninstallPlan.ToArray())
            {
                Commit(entry);
            }

            foreach (var entry in UninstallPlan.ToArray())
            {
                _installsRegistry.RemoveInstall(entry);
            }

            _installsRegistry.Commit();

            foreach (var entry in UninstallPlan)
            {
                _logger.Log("Unnstalled " + entry.RepositoryName + " @ " + entry.Name);
            }

        }

        private void Commit(Install install)
        {

            //DANCE, BABY, DANCE!

            if (Directory.Exists(install.FullMappings.Default))
            {
                Directory.Delete(install.FullMappings.Default,true);
            }

            if (Directory.Exists(install.FullMappings.Root))
            {
                Directory.Delete(install.FullMappings.Root, true);
            }


        }
    }
}