using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TheMiddle.Monitors;

namespace TheMiddle.Interceptors
{
    internal class StdIOInterceptor : Interceptor
    {

        private const int bufferSize = 1024 * 4;

        private BufferedStream bufferedOut;
        private BufferedStream bufferedErr;
        private Process process;
        private TaskCompletionSource<bool> processExitTcs;

        public StdIOInterceptor(string applicationPath, string applicationArguments,
            Stream inStream, Stream outStream, Stream errStream)
        {
            ApplicationPath = applicationPath;
            ApplicationArguments = applicationArguments;
            InStream = inStream;
            OutStream = outStream;
            ErrStream = errStream;
        }

        public string ApplicationPath { get; }

        public string ApplicationArguments { get;}

        public Stream InStream { get; }

        public Stream OutStream { get; }

        public Stream ErrStream { get; }

        public Task ProcessExitTask
        {
            get
            {
                var localTcs = processExitTcs;
                if (processExitTcs == null)
                {
                    var localProc = process;
                    if (localProc == null) return Task.CompletedTask;
                    var newTcs = new TaskCompletionSource<bool>();
                    localTcs = Interlocked.CompareExchange(ref processExitTcs, newTcs, null);
                    if (localTcs == null)
                    {
                        localProc.Exited += (sender, e) => localTcs.TrySetResult(true);
                        localTcs = newTcs;
                    }
                }
                return localTcs.Task;
            }
        }

        /// <inheritdoc />
        protected override Task OnStartingAsync()
        {
            var startInfo = new ProcessStartInfo(ApplicationPath, ApplicationArguments)
            {
                CreateNoWindow = false,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            process = Process.Start(startInfo);
            process.EnableRaisingEvents = true;
            bufferedOut = new BufferedStream(process.StandardOutput.BaseStream);
            bufferedErr = new BufferedStream(process.StandardError.BaseStream);
            // Long running tasks
            var t1 = WrapExceptionHandler(CopyStreamAsync(InStream, process.StandardInput.BaseStream, StoppingToken));
            var t2 = WrapExceptionHandler(CopyStreamAsync(bufferedOut, OutStream, StoppingToken));
            var t3 = WrapExceptionHandler(CopyStreamAsync(bufferedErr, ErrStream, StoppingToken));
            return Task.CompletedTask;
        }

        private async Task CopyStreamAsync(Stream source, Stream dest, CancellationToken ct)
        {
            Debug.Assert(source != null);
            Debug.Assert(dest != null);
            var buffer = new byte[bufferSize];
            int count;
            while ((count = await source.ReadAsync(buffer, 0, bufferSize, ct)) > 0)
            {
                await dest.WriteAsync(buffer, 0, count, ct);
                await dest.FlushAsync(ct);
            }
        }

        /// <inheritdoc />
        protected override Task OnStoppingAsync()
        {
            if (process != null && !process.WaitForExit(100)) process.Kill();
            return Task.CompletedTask;
        }

        private async Task WrapExceptionHandler(Task inner)
        {
            try
            {
                await inner;
            }
            catch (Exception e)
            {
                //TODO output error to appropriate channel.
                Debug.WriteLine(e);
                throw;
            }
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                bufferedOut?.Dispose();
                bufferedErr?.Dispose();
                process?.Dispose();
                process = null;
                bufferedOut = null;
                bufferedErr = null;
            }
            base.Dispose(disposing);
        }
    }
}
