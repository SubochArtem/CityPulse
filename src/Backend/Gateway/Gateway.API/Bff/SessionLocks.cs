using Gateway.API.Constants;

namespace Gateway.API.Bff;

public sealed class SessionLocks : IDisposable
{
    private readonly SemaphoreSlim[] _locks = Enumerable.Range(0, BffDefaults.SessionLockCount)
        .Select(_ => new SemaphoreSlim(1, 1))
        .ToArray();

    public SemaphoreSlim For(string sessionKey) =>
        _locks[(uint)StringComparer.Ordinal.GetHashCode(sessionKey) % (uint)_locks.Length];

    public void Dispose()
    {
        foreach (var semaphore in _locks)
        {
            semaphore.Dispose();
        }
    }
}
