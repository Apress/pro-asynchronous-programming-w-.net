using System;

namespace RSKSchedulers
{
    public interface IIdleTrigger : IDisposable
    {
        event EventHandler CheckIdle;
        TimeSpan IdleTimeout { get; }
    }
}