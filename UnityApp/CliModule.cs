namespace Koinonia
{
    public class CliModule
    {
        public CliModule(ITerminalFrontend terminal)
        {
            Terminal = terminal;
        }

        public ITerminalFrontend Terminal { get; set; }
    }
}