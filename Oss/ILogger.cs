namespace Oss
{
    public interface ILogger
    {
        void Info(string message);
        void Error(string message);
        void Message(string message);
        void Warn(string message);
        void Noise();
    }
}