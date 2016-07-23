using System;

namespace Koinonia
{
    public interface IBackgroundOperationDispatcher
    {
        void Dispatch(string message, Action op);
    }
}