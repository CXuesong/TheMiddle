using System;

namespace TheMiddle.Monitors
{
    public interface IStreamMonitor
    {

        void OnRead(ArraySegment<byte> data);

        void OnWrite(ArraySegment<byte> data);

        void OnSeek(long position);

    }
}
