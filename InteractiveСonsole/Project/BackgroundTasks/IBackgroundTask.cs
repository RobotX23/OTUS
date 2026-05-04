namespace InteractiveСonsole
{
    public interface IBackgroundTask
    {
        Task Start(CancellationToken ct);
    }
}
