namespace Game.Core.Logger
{
    public interface ILoggerService
    {
        void Log(string message);
        void LogWarning(string message);
        void LogError(string message);
    }
}