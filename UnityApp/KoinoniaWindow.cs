using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace Koinonia
{
    public class KoinoniaWindow : EditorWindow, IBackgroundOperationDispatcher
    {
        private static KoinoniaApplication _manager;
        private RotatingIconWidget _loadingWidget;
        private string _loadingMessage;
        private TerminalWidget _terminalWidget;
        private UnityTerminalFrontend _unityTerminalFrontend;
        public Rect Bounds { get; set; }
        public RotatingIconWidget LoadingWidget
        {
            get
            {

                if (_loadingWidget == null)
                {
                    var tex = new Texture2D(128,128);
                    tex.LoadImage(Images.GearIconBytes);
                    _loadingWidget = new RotatingIconWidget();
                }
                return _loadingWidget;
            }
            set { _loadingWidget = value; }
        }

        public TerminalWidget TerminalWidget
        {
            get
            {
                return _terminalWidget ?? (_terminalWidget = new TerminalWidget(KoinoniaUnityCli,this));
            }
            set { _terminalWidget = value; }
        }

        public ITerminalFrontend KoinoniaUnityCli
        {
            get { return KoinoniaApplication.Instance.CliFrontend; }
        }


        static KoinoniaWindow()
        {
            ThreadingUtils.Initialize();
        }

        [MenuItem("Packages/Manage...")]
        static void InitMe()
        {
            var window = GetWindow(typeof(KoinoniaWindow),false,"Packages ",true) as KoinoniaWindow;
            window.minSize = new Vector2(800,600);
        }


        void OnGUI()
        {
            Bounds = new Rect(0,0,position.width,position.height);
            EditorGUI.DrawRect(Bounds, Colors.DarkPrimaryColor);
            var terminalBounds = Bounds;
            TerminalWidget.Draw(terminalBounds);

            if(Loading)
                LoadingWidget.Draw(terminalBounds.WithSize(64,64).AlignTopRight(terminalBounds).Translate(-16,16));
        }

        void Update()
        {
            Repaint();
        }

        public void Dispatch(string message, Action op)
        {
            KoinoniaUnityCli.Post(message);
        }

        public bool Loading
        {
            get { return KoinoniaApplication.Instance.CliFrontend.IsWorking; }
        }
    }
}