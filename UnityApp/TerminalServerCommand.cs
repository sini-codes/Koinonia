using System;

namespace Koinonia
{
    public class TerminalServerCommand
    {
        public string CommandCode { get; set; }
        public string Help { get; set; }
        public Action<string[]> Action { get; set; } 
    }
}