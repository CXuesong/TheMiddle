using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TheMiddle.Monitors;

namespace TheMiddle.Interceptors
{

    public enum InterceptorState
    {
        Initialized = 0,
        Starting,
        Running,
        Stopping,
        Stopped
    }
    
    public abstract class Interceptor : IDisposable
    {

        private CancellationTokenSource stoppingCts;

        public Interceptor()
        {
            State = InterceptorState.Initialized;
        }

        public InterceptorState State { get; private set; }

        public CancellationToken StoppingToken
        {
            get
            {
                if (stoppingCts == null)
                {
                    Interlocked.CompareExchange(ref stoppingCts, new CancellationTokenSource(), null);
                }
                return stoppingCts.Token;
            }
        }

        public async Task StartAsync()
        {
            if (State != InterceptorState.Initialized) throw new InvalidOperationException();
            State = InterceptorState.Starting;
            await OnStartingAsync().ConfigureAwait(false);
            State = InterceptorState.Running;
        }

        public async Task StopAsync()
        {
            if (State != InterceptorState.Running) throw new InvalidOperationException();
            State = InterceptorState.Stopping;
            stoppingCts?.Cancel();
            await OnStoppingAsync().ConfigureAwait(false);
            State = InterceptorState.Stopped;
        }

        protected abstract Task OnStartingAsync();

        protected abstract Task OnStoppingAsync();

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (State == InterceptorState.Running)
                {
                    var task = StopAsync();
                    if (!task.Wait(1000))
                    {
                        Debug.WriteLine("Did not wait for StopAsync to return in 1000ms.");
                    }
                } else if (State == InterceptorState.Stopping)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        Thread.Sleep(100);
                        if (State == InterceptorState.Stopped) break;
                    }
                    if (State != InterceptorState.Stopped)
                        Debug.WriteLine("Did not wait for StopAsync to return in 1000ms.");
                }
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        ~Interceptor()
        {
            Dispose(false);
        }
    }
}
