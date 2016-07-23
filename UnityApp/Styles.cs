using UnityEditor;
using UnityEngine;

namespace Koinonia
{
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
                if (Ko.IsNull(_labelStyle))
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
                if (Ko.IsNull(_primaryButtonStyle))
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
                if (Ko.IsNull(_terminalTextfieldStyle))
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
                if (Ko.IsNull(_terminalLineStyle))
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
}