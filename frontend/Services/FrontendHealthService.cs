using frontend.Services;

namespace frontend.Services;

public interface IFrontendHealthService
{
    bool IsReady { get; }
    DateTime StartTime { get; }
}

public class FrontendHealthService : IFrontendHealthService
{
    public bool IsReady { get; private set; } = true;
    public DateTime StartTime { get; private set; } = DateTime.UtcNow;

    public FrontendHealthService()
    {
        IsReady = true;
        StartTime = DateTime.UtcNow;
    }
}