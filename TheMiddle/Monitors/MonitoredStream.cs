using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace TheMiddle.Monitors
{
    public class MonitoredStream : Stream
    {
        public MonitoredStream(Stream baseStream, IStreamMonitor streamMonitor)
        {
            BaseStream = baseStream ?? throw new ArgumentNullException(nameof(baseStream));
            StreamMonitor = streamMonitor ?? throw new ArgumentNullException(nameof(streamMonitor));
        }

        public Stream BaseStream { get; }

        public IStreamMonitor StreamMonitor { get; }

        /// <inheritdoc />
        public override void Flush()
        {
            BaseStream.Flush();
        }

        /// <inheritdoc />
        public override int Read(byte[] buffer, int offset, int count)
        {
            var readCount = BaseStream.Read(buffer, offset, count);
            if (readCount > 0)
            {
                StreamMonitor.OnRead(new ArraySegment<byte>(buffer, offset, readCount));
            }
            return readCount;
        }

        /// <inheritdoc />
        public override long Seek(long offset, SeekOrigin origin)
        {
            var pos = BaseStream.Seek(offset, origin);
            StreamMonitor.OnSeek(pos);
            return pos;
        }

        /// <inheritdoc />
        public override void SetLength(long value)
        {
            BaseStream.SetLength(value);
        }

        /// <inheritdoc />
        public override void Write(byte[] buffer, int offset, int count)
        {
            BaseStream.Write(buffer, offset, count);
            if (count > 0)
            {
                StreamMonitor.OnWrite(new ArraySegment<byte>(buffer, offset, count));
            }
        }

        /// <inheritdoc />
        public override bool CanRead => BaseStream.CanRead;

        /// <inheritdoc />
        public override bool CanSeek => BaseStream.CanWrite;

        /// <inheritdoc />
        public override bool CanWrite => BaseStream.CanWrite;

        /// <inheritdoc />
        public override long Length => BaseStream.Length;

        /// <inheritdoc />
        public override long Position
        {
            get { return BaseStream.Position; }
            set
            {
                BaseStream.Position = value;
                StreamMonitor.OnSeek(value);
            }
        }

        /// <inheritdoc />
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            var readCount = await BaseStream.ReadAsync(buffer, offset, count, cancellationToken);
            if (readCount > 0)
            {
                StreamMonitor.OnRead(new ArraySegment<byte>(buffer, offset, readCount));
            }
            return readCount;
        }

        /// <inheritdoc />
        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            await BaseStream.WriteAsync(buffer, offset, count, cancellationToken);
            if (count > 0)
            {
                StreamMonitor.OnWrite(new ArraySegment<byte>(buffer, offset, count));
            }
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            return BaseStream.FlushAsync(cancellationToken);
        }
    }
}
