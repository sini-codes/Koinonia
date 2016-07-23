using UnityEngine;

namespace Koinonia
{
    public static class Fonts
    {
        private static Font _lucidaConsole;

        public static Font TerminalFont
        {
            get
            {
                if (Ko.IsNull(_lucidaConsole))
                {
                    _lucidaConsole = Font.CreateDynamicFontFromOSFont("Courier New", 13);
                }
                return _lucidaConsole;
            }
            set { _lucidaConsole = value; }
        }
    }
}