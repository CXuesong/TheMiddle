using System;
using System.Diagnostics;

namespace TheMiddle.Monitors
{
    public abstract class StreamMonitorProvider
    {

        public abstract IStreamMonitor CreateStreamMonitor(string name);

    }

    public abstract class InterleavingStreamMonitorProvider : StreamMonitorProvider
    {

        /// <inheritdoc />
        public override IStreamMonitor CreateStreamMonitor(string name)
        {
            return new StreamMonitor(this, name, null);
        }

        protected virtual void OnNotifyRead(StreamMonitor monitor, ArraySegment<byte> data)
        {
            Debug.Assert(monitor != null);
            Debug.Assert(data.Count > 0);
        }

        protected virtual void OnNotifyWrite(StreamMonitor monitor, ArraySegment<byte> data)
        {
            Debug.Assert(monitor != null);
            Debug.Assert(data.Count > 0);
        }

        protected virtual void OnNotifySeek(StreamMonitor monitor, long position)
        {
            Debug.Assert(monitor != null);
            Debug.Assert(position >= 0);
        }

        protected sealed class StreamMonitor : IStreamMonitor
        {

            private readonly InterleavingStreamMonitorProvider owner;

            public StreamMonitor(InterleavingStreamMonitorProvider owner, string name, object tag)
            {
                Debug.Assert(owner != null);
                this.owner = owner;
                Name = name;
                Tag = tag;
            }

            public string Name { get; }

            public object Tag { get; set; }

            /// <inheritdoc />
            public void OnRead(ArraySegment<byte> data)
            {
                owner.OnNotifyRead(this, data);
            }

            /// <inheritdoc />
            public void OnWrite(ArraySegment<byte> data)
            {
                owner.OnNotifyWrite(this, data);
            }

            /// <inheritdoc />
            public void OnSeek(long position)
            {
                owner.OnNotifySeek(this, position);
            }
        }

    }

    internal enum StreamAction
    {
        Read = 0,
        Write = 1,
        Seek = 2,
    }

}
