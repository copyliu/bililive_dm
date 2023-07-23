using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BiliDMLib
{
    public static class utils
    {
        public static async Task ReadBAsync(this Stream stream, byte[] buffer, int offset, int count,
            CancellationToken ct)
        {
            if (offset + count > buffer.Length)
                throw new ArgumentException();
            var read = 0;
            while (read < count)
            {
                var available = await stream.ReadAsync(buffer, offset, count - read, ct);
                if (available == 0) throw new ObjectDisposedException(null);
                //                if (available != count)
//                {
//                    throw new NotSupportedException();
//                }
                read += available;
                offset += available;
            }
        }
    }
}