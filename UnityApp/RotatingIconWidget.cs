using UnityEngine;

namespace Koinonia
{
    public class RotatingIconWidget
    {
        public float Angle { get; set; }

        public Texture2D Icon
        {
            get
            {
                return Textures.GearIcon;
            }
        }

        private GUIStyle _labelStyle;
        private Texture2D _icon;

        public void Draw(Rect bounds)
        {


            bounds = bounds.PadSides(4);

            var cacheMtx = GUI.matrix;
            var iconBounds = bounds;

            GUIUtility.RotateAroundPivot(Angle, bounds.center);
            GUI.DrawTexture(iconBounds,Icon,ScaleMode.ScaleToFit);
            GUI.matrix = cacheMtx;
            Angle += 0.5f;



        }

    }
}