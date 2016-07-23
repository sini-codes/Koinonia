namespace Koinonia
{
    public interface IKoinoniaLogger
    {
        void Log(string str);
        void LogProblem(string error);
        void LogWarning(string warning);
    }
}