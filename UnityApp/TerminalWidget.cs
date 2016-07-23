using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Koinonia
{
    public class TerminalWidget
    {
        private ITerminalFrontend _frontend;
        private IBackgroundOperationDispatcher _dispatcher;

        public TerminalWidget(ITerminalFrontend provider, IBackgroundOperationDispatcher dispatcher)
        {
            _frontend = provider;
            _dispatcher = dispatcher;
            _frontend.LinesUpdated += () =>
            {

            };
        }

        public string CurrentInput { get; set; }
        public Vector2 ScrollPosition { get; set; }
        public void Draw(Rect bounds)
        {
            bounds = bounds.PadSides(4);

            var inputBounds = bounds.WithHeight(20).InnerAlignWithBottomCenter(bounds);
            var linesBounds = bounds.Above(inputBounds).Clip(bounds);

            // EditorGUI.DrawRect(linesBounds, Colors.PrimaryColor);

            GUI.SetNextControlName("TerminalInput");
            var gtBounds = inputBounds.WithWidth(18);
            GUI.Label(gtBounds, ">", Styles.TerminalTextfieldStyle);
            CurrentInput = EditorGUI.TextField(inputBounds.RightOf(gtBounds).Clip(inputBounds), CurrentInput,Styles.TerminalTextfieldStyle);

            var lines = _frontend.Lines.ToArray();
            var height = 17 ;
            var lineItemTemplateBounds = linesBounds.WithHeight(height);
            var totalHeight = height*lines.Length;

            ScrollPosition = GUI.BeginScrollView(linesBounds.AddHeight(-(int)inputBounds.height), ScrollPosition, linesBounds.WithHeight(totalHeight).AddWidth(-20));

            foreach (var line in lines)
            {
                EditorGUI.SelectableLabel(lineItemTemplateBounds,line, Styles.TerminalLineStyle);
                lineItemTemplateBounds = lineItemTemplateBounds.Below(lineItemTemplateBounds);
            }
            GUI.EndScrollView(true);

            if (Event.current.isKey)
            {
                if (Event.current.keyCode == KeyCode.Return && !string.IsNullOrEmpty(CurrentInput))
                {
                    var cache = CurrentInput;
                    _frontend.Post(cache);
                    CurrentInput = null;
                    EditorGUI.FocusTextInControl("TerminalInput");
                }
            }
        }


    }
}