using System;

namespace Koinonia
{
    public class CliCommand : Attribute
    {
        public CliCommand(string code)
        {
            Code = code;
        }

        public CliCommand(string code, string help)
        {
            Code = code;
            Help = help;
        }

        public string Code { get; set; }
        public string Help { get; set; }
    }
}