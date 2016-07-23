using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace Koinonia
{

    public class CLIAlias : Attribute
    {
        public CLIAlias(string code)
        {
            Code = code;
        }

        public string Code { get; set; }
    }

    public class CLICommand : Attribute
    {
        public CLICommand(string code)
        {
            Code = code;
        }

        public CLICommand(string code, string help)
        {
            Code = code;
            Help = help;
        }

        public string Code { get; set; }
        public string Help { get; set; }
    }

    public class KoinoniaWindow : EditorWindow, IBackgroundOperationDispatcher
    {
        private static KoinoniaApplication _manager;
        private static IPackageConfigManager _packageConfigManager;
        private RotatingIconWidget _loadingWidget;
        private string _loadingMessage;
        private TerminalWidget _terminalWidget;
        private KoinoniaUnityCli _koinoniaUnityCli;
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

    public interface IBackgroundOperationDispatcher
    {
        void Dispatch(string message, Action op);
    }

    public class DownloadablesHostsListViewModel
    {
        private List<DownloadablesHost> _hosts;

        public List<DownloadablesHost> Hosts
        {
            get { return _hosts ?? (_hosts = new List<DownloadablesHost>()); }
            set { _hosts = value; }
        }
    }

    public class DownloadablesHostsListView
    {
        public DownloadablesHostsListViewModel ViewModel { get; set; }

        public void Draw(Rect bounds)
        {
            
        }

    }

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

    public abstract class Installer
    {
        protected Installer(ITerminalFrontend terminal)
        {
            Terminal = terminal;
        }

        public ITerminalFrontend Terminal { get; set; }

        public virtual bool PostInstall(Install install)
        {
            return true;
        }
    }

    public interface ITerminalFrontend : IKoinoniaLogger
    {
        string Read();
        event Action LinesUpdated;
        IEnumerable<string> Lines { get; }
        Thread Post(string msg);
        IEnumerable<TerminalServerCommand> Commands { get; }
        bool IsWorking { get;  }
    }

    public class TerminalServerCommand
    {
        public string CommandCode { get; set; }
        public string Help { get; set; }
        public Action<string[]> Action { get; set; } 
    }


}