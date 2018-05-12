using System;
using System.IO;
using System.Text;

namespace TheMiddle.Monitors
{
    public class DumpedStreamMonitorProvider : InterleavingStreamMonitorProvider
    {

        private StreamMonitor lastStreamMonitor;
        private StreamAction lastStreamAction;
        private static readonly byte[] newLine = Encoding.UTF8.GetBytes(Environment.NewLine);
        private readonly object streamLock = new object();

        public DumpedStreamMonitorProvider(Stream dumpStream)
        {
            DumpStream = dumpStream ?? throw new ArgumentNullException(nameof(dumpStream));
        }

        /// <summary>
        /// The stream that will receive the dump of the read/written content of the monitored streams.
        /// </summary>
        public Stream DumpStream { get; }

        /// <inheritdoc />
        public override IStreamMonitor CreateStreamMonitor(string name)
        {
            var actionTags = new byte[3][];
            actionTags[(int) StreamAction.Read] = Encoding.UTF8.GetBytes("[" + name + "|R]");
            actionTags[(int) StreamAction.Write] = Encoding.UTF8.GetBytes("[" + name + "|W]");
            actionTags[(int) StreamAction.Seek] = Encoding.UTF8.GetBytes("[" + name + "|S]");
            return new StreamMonitor(this, name, actionTags);
        }

        private byte[] GetStreamTag(StreamMonitor monitor, StreamAction action)
        {
            return ((byte[][]) monitor.Tag)[(int) action];
        }

        private void WriteLine()
        {
            DumpStream.Write(newLine, 0, newLine.Length);
            DumpStream.Flush();
        }

        /// <inheritdoc />
        protected override void OnNotifyRead(StreamMonitor monitor, ArraySegment<byte> data)
        {
            lock (streamLock)
            {
                if (lastStreamMonitor != monitor || lastStreamAction != StreamAction.Read)
                {
                    var tag = GetStreamTag(monitor, StreamAction.Read);
                    //WriteLine();
                    DumpStream.Write(tag, 0, tag.Length);
                    lastStreamMonitor = monitor;
                    lastStreamAction = StreamAction.Read;
                }
                DumpStream.Write(data.Array, data.Offset, data.Count);
            }
        }

        /// <inheritdoc />
        protected override void OnNotifyWrite(StreamMonitor monitor, ArraySegment<byte> data)
        {
            lock (streamLock)
            {
                if (lastStreamMonitor != monitor || lastStreamAction != StreamAction.Write)
                {
                    var tag = GetStreamTag(monitor, StreamAction.Write);
                    //WriteLine();
                    DumpStream.Write(tag, 0, tag.Length);
                    lastStreamMonitor = monitor;
                    lastStreamAction = StreamAction.Write;
                }
                DumpStream.Write(data.Array, data.Offset, data.Count);
            }
        }

        /// <inheritdoc />
        protected override void OnNotifySeek(StreamMonitor monitor, long position)
        {
            lock (streamLock)
            {
                //WriteLine();
                lastStreamMonitor = monitor;
                lastStreamAction = StreamAction.Seek;
                var content = Encoding.UTF8.GetBytes(position.ToString());
                DumpStream.Write(content, 0, content.Length);
            }
        }

    }
}
