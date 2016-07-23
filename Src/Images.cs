using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Koinonia
{
    public static class Images
    {
        private static byte[] _gearIconBytes;

        public const string GearIconCode =
            "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";

        public static byte[] GearIconBytes
        {
            get { return _gearIconBytes ?? (_gearIconBytes = Convert.FromBase64String(GearIconCode)); }
        }
    }

    public static class Colors
    {
        public static Color DarkPrimaryColor = FromHex("#455A64");
        public static Color PrimaryColor = FromHex("#607D8B");
        public static Color LightPrimaryColor = new Color32(187,222,251,255); //#BBDEFB
        public static Color TextIconsColor = new Color32(255,255,255,255); //#FFFFFF
        public static Color AccentColor = new Color32(76,175,80,255); //#4CAF50
        public static Color DarkAccentColor = new Color32(66,165,70,255); //#4CAF50

        public static Color SecondaryText = new Color32(114,114,114,255); //#727272
        public static Color AlmostPrimaryTest = new Color32(66,66,66,255); 
        public static Color PrimaryText = new Color32(33,33,33,255); //#212121
        public static Color DividerColor = new Color32(182, 182, 182, 255); //#B6B6B6


        public static Color FromHex(string hex)
        {
            hex = hex.Replace("0x", "");//in case the string is formatted 0xFFFFFF
            hex = hex.Replace("#", "");//in case the string is formatted #FFFFFF
            byte a = 255;//assume fully visible unless specified in hex
            byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            //Only use alpha if the string has enough characters
            if (hex.Length == 8)
            {
                a = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            }
            return new Color32(r, g, b, a);
        }
    }

    public static class Styles
    {
        private static GUIStyle _labelStyle;
        private static GUIStyle _primaryButtonStyle;
        private static GUIStyle _terminalTextfieldStyle;
        private static GUIStyle _terminalLineStyle;

        public static GUIStyle PrimaryLabelStyle
        {
            get
            {
                if (KO.IsNull(_labelStyle))
                {
                    _labelStyle = new GUIStyle(EditorStyles.largeLabel)
                    {
                        normal = {textColor = Colors.TextIconsColor},
                        alignment = TextAnchor.MiddleCenter,
                        fontSize = 32,
                        fontStyle = FontStyle.Bold
                    };
                }
                return _labelStyle;
            }
            set { _labelStyle = value; }
        }

        public static GUIStyle PrimaryButtonStyle
        {
            get
            {
                if (KO.IsNull(_primaryButtonStyle))
                {
                    _primaryButtonStyle = new GUIStyle(EditorStyles.largeLabel)
                    {
                        normal = { textColor = Colors.TextIconsColor,background = Textures.AccentBackground },
                        hover = { textColor = Colors.TextIconsColor,background = Textures.AccentHoverBackground },
                        alignment = TextAnchor.MiddleCenter,
                        fontSize = 16,
                        fontStyle = FontStyle.Bold,
                        padding = new RectOffset(8,8,4,4)
                    };
                }
                return _primaryButtonStyle;
            }
            set { _primaryButtonStyle = value; }
        }

        public static GUIStyle TerminalTextfieldStyle
        {
            get
            {
                if (KO.IsNull(_terminalTextfieldStyle))
                {
                    _terminalTextfieldStyle = new GUIStyle(EditorStyles.largeLabel)
                    {
                        normal = { textColor = Colors.AlmostPrimaryTest, background = Textures.TerminalTextfieldBackgroundNormal},
                        hover = { textColor = Colors.AlmostPrimaryTest, background = Textures.TerminalTextfieldBackgroundNormal },
                        alignment = TextAnchor.MiddleLeft,
                        fontSize = 12,
                        fontStyle = FontStyle.Bold,
                        font = Fonts.TerminalFont,

                        padding = new RectOffset(4, 4, 4, 4),
                        border = new RectOffset(3,3,3,3),
                    };
                }
                return _terminalTextfieldStyle;
            }
            set { _terminalTextfieldStyle = value; }
        }

        public static GUIStyle TerminalLineStyle
        {
            get
            {
                if (KO.IsNull(_terminalLineStyle))
                {
                    _terminalLineStyle = new GUIStyle(EditorStyles.largeLabel)
                    {
                        normal = { textColor = Colors.TextIconsColor},
                        fontSize = 13,
                        font = Fonts.TerminalFont,
                        fontStyle = FontStyle.Bold,
                        padding = new RectOffset(4, 4, 0, 0),
                    };
                }
                return _terminalLineStyle;
            }
            set { _terminalLineStyle = value; }
        }
    }

    public static class Textures
    {
        private static Texture2D _accentBackground;

        public static Texture2D AccentBackground
        {
            get
            {
                if (KO.IsNull(_accentBackground))
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
                if (KO.IsNull(_terminalTextfieldBackgroundNormal))
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
                if (KO.IsNull(_accentHoverBackground))
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
                if (KO.IsNull(_gearIcon))
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

    public static class Fonts
    {
        private static Font _lucidaConsole;

        public static Font TerminalFont
        {
            get
            {
                if (KO.IsNull(_lucidaConsole))
                {
                    _lucidaConsole = Font.CreateDynamicFontFromOSFont("Courier New", 13);
                }
                return _lucidaConsole;
            }
            set { _lucidaConsole = value; }
        }
    }

    public static class KO
    {
        public static bool IsNull<T>(T obj)
        {
            return EqualityComparer<T>.Default.Equals(obj, default(T));
        }
    }


}