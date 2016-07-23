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
        Thread Post(string msg);
        IEnumerable<TerminalServerCommand> Commands { get; }
        bool IsWorking { get;  }
    }
}