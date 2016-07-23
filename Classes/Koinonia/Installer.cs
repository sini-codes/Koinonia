namespace Koinonia
{
    public abstract class Installer
    {
        protected Installer(ITerminalFrontend terminal)
        {
            Terminal = terminal;
        }

        public ITerminalFrontend Terminal { get; set; }

        public virtual bool PostInstall(Install install)
        {
            return true;
        }
    }
}