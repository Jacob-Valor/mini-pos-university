using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace mini_pos;

sealed class DbusSafeSynchronizationContext : SynchronizationContext
{
    private readonly SynchronizationContext _inner;

    private DbusSafeSynchronizationContext(SynchronizationContext inner)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    }

    public static void InstallIfNeeded()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return;
        }

        SynchronizationContext? current = Current;
        if (current is null || current is DbusSafeSynchronizationContext)
        {
            return;
        }

        SetSynchronizationContext(new DbusSafeSynchronizationContext(current));
    }

    public override void Send(SendOrPostCallback d, object? state)
    {
        try
        {
            _inner.Send(d, state);
        }
        catch (TaskCanceledException)
        {
        }
    }

    public override void Post(SendOrPostCallback d, object? state) => _inner.Post(d, state);

    public override void OperationStarted() => _inner.OperationStarted();

    public override void OperationCompleted() => _inner.OperationCompleted();

    public override int Wait(IntPtr[] waitHandles, bool waitAll, int millisecondsTimeout)
        => _inner.Wait(waitHandles, waitAll, millisecondsTimeout);

    public override SynchronizationContext CreateCopy() => new DbusSafeSynchronizationContext(_inner.CreateCopy());
}
