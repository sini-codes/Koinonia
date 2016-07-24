using System;
using System.Collections.Generic;
using System.Threading;

namespace Koinonia
{
    public interface ITerminalFrontend : IKoinoniaLogger
    {
        string Read();
        event Action LinesUpdated;
        IEnumerable<string> Lines { get; }
        IEnumerable<TerminalServerCommand> Commands { get; }
        IEnumerable<KeyValuePair<string, string>> Aliases { get; }
        Thread Post(string msg);
        bool IsWorking { get;  }
    }
}