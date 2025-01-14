using Serilog;

namespace Core.CrossCuttingConcerns.SeriLog;

public abstract class LoggerServiceBase
{
    protected ILogger Logger { get; set; }
    protected LoggerServiceBase()
    {
        Logger = Log.Logger;
    }
    public LoggerServiceBase(ILogger logger)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void Verbose(string message) => Logger.Verbose(message);
    public void Debug(string message) => Logger.Debug(message);
    public void Information(string message) => Logger.Information(message);
    public void Warning(string message) => Logger.Warning(message);
    public void Error(string message) => Logger.Error(message);
    public void Fatal(string message) => Logger.Fatal(message);

}
