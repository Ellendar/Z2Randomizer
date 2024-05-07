namespace CrossPlatformUI.Services;

public interface ISuspendSyncService
{
    object LoadState();
    void SaveState(object state);
    void InvalidateState();
}