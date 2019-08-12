
namespace AssembledNet
{
    public interface ILogger
    {
        void LogInfo(string log, params object[] args);
        void LogErr(string log, params object[] args);
    }
}
