using UnityEngine;

namespace Koinonia
{
    public static class Textures
    {
        private static Texture2D _accentBackground;

        public static Texture2D AccentBackground
        {
            get
            {
                if (Ko.IsNull(_accentBackground))
                {
                    _accentBackground = new Texture2D(1,1);
                    _accentBackground.SetPixel(0,0,Colors.AccentColor);
                    _accentBackground.Apply();
                }
                return _accentBackground;
            }
            set { _accentBackground = value; }
        }

        private static Texture2D _accentHoverBackground;
        private static Texture2D _terminalTextfieldBackgroundNormal;

        public static Texture2D TerminalTextfieldBackgroundNormal
        {
            get
            {
                if (Ko.IsNull(_terminalTextfieldBackgroundNormal))
                {
                    _terminalTextfieldBackgroundNormal = GetBorderTexture(6, 6, Colors.LightPrimaryColor , Colors.LightPrimaryColor, 2);
                }
                return _terminalTextfieldBackgroundNormal;
            }
            set { _terminalTextfieldBackgroundNormal = value; }
        }

        public static Texture2D AccentHoverBackground
        {
            get
            {
                if (Ko.IsNull(_accentHoverBackground))
                {
                    _accentHoverBackground = new Texture2D(1, 1);
                    _accentHoverBackground.SetPixel(0, 0, Colors.DarkAccentColor);
                    _accentHoverBackground.Apply();
                }
                return _accentHoverBackground;
            }
            set { _accentHoverBackground = value; }
        }

        private static Texture2D _gearIcon;

        public static Texture2D GearIcon
        {
            get
            {
                if (Ko.IsNull(_gearIcon))
                {
                    _gearIcon = new Texture2D(1,1);
                    _gearIcon.LoadImage(Images.GearIconBytes);
                }
                return _gearIcon;
            }
        }


        public static Texture2D GetBorderTexture(int width, int height, Color inside, Color outside, int thickness)
        {
            var tex = new Texture2D(width,height,TextureFormat.RGBA32, false,true);

            var xybl = thickness - 1;
            var xtl = width - thickness ;
            var ytl = height - thickness ;
            var color = inside;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    color = inside;
                    if (x <= xybl || x >= xtl) color = outside;
                    if (y <= xybl || y >= ytl) color = outside;
                    tex.SetPixel(x,y,color);        
                }
            }

            tex.Apply();
            return tex;

        }

    }
}