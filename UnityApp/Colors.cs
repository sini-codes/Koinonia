using UnityEngine;

namespace Koinonia
{
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
}