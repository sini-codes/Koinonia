using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using Microsoft.CSharp;
using UnityEngine;


namespace Koinonia
{
    public class UnityTerminalFrontend : ITerminalFrontend
    {

        private List<string> _linesContainer;
        private Dictionary<string, TerminalServerCommand> _cliCommands;
        private List<TerminalServerCommand> _helpCommands;
        private Dictionary<string, string> _cliAliases;

        EventWaitHandle s;

        string response = null;

        private List<Thread> _workers;
        private IEnumerable<CliAlias> _aliases;
        private List<CliModule> _plugins;

        public string Read()
        {
            s = new ManualResetEvent(false);
            s.WaitOne();
            s.Close();
            s = null;
            return response;
        }

        public List<CliModule> Plugins
        {
            get { return _plugins ?? (_plugins = new List<CliModule>()); }
            set { _plugins = value; }
        }

        public UnityTerminalFrontend()
        {

            var modules = typeof (CliModule).FindImplementations();

            foreach (var module in modules)
            {

                var pluginInstance = Activator.CreateInstance(module, this) as CliModule;

                Plugins.Add(pluginInstance);

                var methods = module.GetMethods().Where(m => m.IsDefined(typeof(CliCommand), true));

                foreach (var methodInfo in methods)
                {
                    var c = methodInfo.GetCustomAttributes(typeof(CliCommand), true).FirstOrDefault() as CliCommand;
                    var info = methodInfo;

                    Action<string[]> handler = str =>
                    {
                        info.Invoke(pluginInstance, new object[] { str });
                    };

                    CliCommands.Add(c.Code, new TerminalServerCommand()
                    {
                        Action = handler,
                        CommandCode = c.Code,
                        Help = c.Help
                    });

                    var aliases = methodInfo.GetCustomAttributes(typeof(CliAlias), true).Cast<CliAlias>();

                    foreach (var alias in aliases)
                    {
                        CliAliases.Add(alias.Code, c.Code);
                    }

                }

            }


     

            Log("Hello, I am Koinonia!");
            Log("Type \"help\" or \"h\" for a list of available commands.");
            Log("");
            Log("");
         
        }

        public Dictionary<string, TerminalServerCommand> CliCommands
        {
            get { return _cliCommands ?? (_cliCommands = new Dictionary<string, TerminalServerCommand>()); }
            set { _cliCommands = value; }
        }
        

        public Dictionary<string, string> CliAliases
        {
            get { return _cliAliases ?? (_cliAliases = new Dictionary<string, string>()); }
            set { _cliAliases = value; }
        }

        public IEnumerable<KeyValuePair<string,string>> Aliases
        {
            get { return CliAliases; }
        }

        public List<string> LinesContainer
        {
            get { return _linesContainer ?? (_linesContainer = new List<string>()); }
            set { _linesContainer = value; }
        }

        public event Action LinesUpdated;

        public IEnumerable<string> Lines
        {
            get
            {
                return LinesContainer;
            }
        }

        public Thread Post(string msg)
        {
            Log(">  "+msg);

            Thread thread = null;
            thread = new Thread(() =>
            {
                try
                {
                    Process(msg);
                }
                catch (TargetInvocationException ex)
                {
                    ExceptionProcessor(ex.InnerException);
                }
                catch (Exception ex)
                {
                    ExceptionProcessor(ex);
                }
                finally
                {
                    Workers.Remove(thread);
                }
            });
            Workers.Add(thread);
            thread.Start();

            return thread;
        }

        public void ExceptionProcessor(Exception ex)
        {
            if (ex is WebException)
            {
                LogProblem("Connection Problem: "+ex.Message);
            } else 
            {
                Debug.LogError(ex);
            }
        }

        public IEnumerable<TerminalServerCommand> Commands
        {
            get
            {
                return CliCommands.Values;
            }
        }

        public bool IsWorking
        {
            get { return Workers.Any(); }
        }

        public List<Thread> Workers
        {
            get { return _workers ?? (_workers = new List<Thread>()); }
            set { _workers = value; }
        }

        private void Process(string msg)
        {

            if (s != null)
            {
                response = msg;
                s.Set();
                return; //Intercepterd by running process 
                //TODO: introduce read queue later?
            }

            var args = msg.Split(new char[] {' ', '\t'}, StringSplitOptions.RemoveEmptyEntries);
            var key = args[0];

            if (CliCommands.ContainsKey(key))
            {
                var handler = CliCommands[key];
                handler.Action(args);
            } else if (CliAliases.ContainsKey(key))
            {
                var handler = CliCommands[CliAliases[key]];
                handler.Action(args);
            }
            else
            {
                LogProblem("No Such Command");
            }

        }

        public void LogProblem(string error)
        {
            Log("Error: "+error);
        }

        public void LogWarning(string warning)
        {
            throw new NotImplementedException();
        }

        public void Log(string msg)
        {
            LinesContainer.Add(msg);
            if (LinesUpdated != null) LinesUpdated();
        }

    


    }

}