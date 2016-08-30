using System;
using System.Linq;

namespace Koinonia
{
    public class Core : CliModule
    {
        public Core(ITerminalFrontend terminal) : base(terminal)
        {
        }

        [CliCommand("github_token", "Echo or set github token")]
        [CliAlias("ghtoken")]
        public void GithubAccessToken(string[] args)
        {
            if (args.Length > 1 && !string.IsNullOrEmpty(args[1]))
                KoinoniaApplication.AccessToken = args[1];
            Terminal.Log("Github Access Token: " + KoinoniaApplication.AccessToken);
        }

        [CliCommand("update", "Update all packages or a given package")]
        [CliAlias("up")]
        public void Update(string[] args)
        {
            if (args.Length > 1 && !string.IsNullOrEmpty(args[1]))
            {
                var installScheme = GithubSchemeDecoder.DecodeShort(args[1]);
                var install = KoinoniaApplication.Instance.FetchInstallsRegistry().Installs.FirstOrDefault(_ => _.RepositoryName == installScheme.Name && _.AuthorName == installScheme.Owner);

                KoinoniaApplication.Instance.Update(install);
            }
        }

        [CliCommand("finalize", "Manually finish all the installs when needed")]
        public void Finalize(string[] args)
        {
            KoinoniaApplication.Instance.FinalizeInstalls();
        }

        [CliCommand("list_downloadables", "List downloadables for a host")]
        public void ListDownloadables(string[] args)
        {

            var shc = GithubSchemeDecoder.DecodeShort(args[1]);

            var host = new DownloadablesHost()
            {
                AuthorName = shc.Owner,
                RepositoryName = shc.Name
            };

            host.FetchDownloadables();

            foreach (var downloadable in host.Downloadables)
            {
                Terminal.Log(downloadable.ToString());
            }

        }

        [CliCommand("test_installer")]
        public void TestInstaller(string[] args)
        {
            KoinoniaApplication.Instance.TestInstaller(args[1], args[2]);
        }

        [CliCommand("help", "Show list of available commands")]
        [CliAlias("h")]
        public void Help(string[] args)
        {
            Terminal.Log("Commands:");
            foreach (var cliCommand in Terminal.Commands)
            {
                Terminal.Log("   " + cliCommand.CommandCode);
                if (!string.IsNullOrEmpty(cliCommand.Help))
                    Terminal.Log("       " + cliCommand.Help);

                var aliases = Terminal.Aliases.Where(v => v.Value == cliCommand.CommandCode).ToArray();
                if (aliases.Any())
                {
                    Terminal.Log("       Aliases:");
                    foreach (var alias in aliases)
                    {
                        Terminal.Log("           " + alias.Key);
                    }
                }
            }
        }

        [CliCommand("list", "Show list of installed packages")]
        [CliAlias("l")]
        public void List(string[] args)
        {

            KoinoniaApplication.Instance.FetchInstallsRegistry(true);
            foreach (var install in KoinoniaApplication.Instance.InstallsRegistry)
            {
                Terminal.Log(install.ToString());
            }

        }

        [CliCommand("uninstall", "Uninstall package")]
        [CliAlias("u")]
        public void Uninstall(string[] args)
        {
            var installScheme = GithubSchemeDecoder.DecodeShort(args[1]);
            var install = KoinoniaApplication.Instance.FetchInstallsRegistry().Installs.FirstOrDefault(_ => _.RepositoryName == installScheme.Name && _.AuthorName == installScheme.Owner);

            if (install != null)
            {
                KoinoniaApplication.Instance.UninstallNode(install);
            }


        }



        [CliCommand("install", "Install package")]
        [CliAlias("i")]
        public void Install(string[] args)
        {

            GithubRepositoryEntry dec = GithubSchemeDecoder.Decode(args[1]);

            Terminal.Log(string.Format("Will install {0} from {1} at {2}", dec.Name, dec.Owner, dec.Tag));

            var downloadableHost = new DownloadablesHost()
            {
                AuthorName = dec.Owner,
                RepositoryName = dec.Name
            };

            Terminal.Log("Fetching downloadables...");

            downloadableHost.FetchDownloadables();

            Terminal.Log(string.Format("{0}/{1} contains {2} downloadable entries...", dec.Owner, dec.Name, downloadableHost.Downloadables.Count));


            Downloadable d = downloadableHost.Downloadables.FirstOrDefault();

            if (!string.IsNullOrEmpty(dec.Tag))
            {
                d = downloadableHost.Downloadables.FirstOrDefault(_ => _.Name == dec.Tag);
            }

            if (d == null) throw new Exception("Downloadable Entry not found");

            Terminal.Log(string.Format("Installing {0}/{1} @ {2} # {3}...", d.AuthorName, d.RepositoryName, d.Name, d.CommitSha));


            KoinoniaApplication.Instance.InstallNode(d);

        }
    }
}