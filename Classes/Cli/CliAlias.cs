using System;

namespace Koinonia
{
    public class CliAlias : Attribute
    {
        public CliAlias(string code)
        {
            Code = code;
        }

        public string Code { get; set; }
    }
}