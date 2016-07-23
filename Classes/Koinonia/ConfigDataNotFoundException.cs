using System;

namespace Koinonia
{
    public class ConfigDataNotFoundException : Exception
    {

        public ConfigDataNotFoundException(Downloadable target)
        {
            Target = target;
        }

        public Downloadable Target { get; set; }

        public override string Message
        {
            get { return "No config file found in downloadable: "+Target; }
        }
    }
}