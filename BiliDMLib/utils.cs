using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace BiliDMLib
{
    public static class utils
    {
        /// <summary>Creates an <see cref="IAsyncEnumerable{T}"/> that enables receiving all of the data from the source.</summary>
        /// <typeparam name="TOutput">Specifies the type of data contained in the source.</typeparam>
        /// <param name="source">The source from which to asynchronously receive.</param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken"/> which may be used to cancel the receive operation.</param>
        /// <returns>The created async enumerable.</returns>
        /// <exception cref="System.ArgumentNullException">The <paramref name="source"/> is null (Nothing in Visual Basic).</exception>
        /// 
        public static IAsyncEnumerable<TOutput> ReceiveAllAsync<TOutput>(this IReceivableSourceBlock<TOutput> source, CancellationToken cancellationToken = default)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return Core(source, cancellationToken);

            static async IAsyncEnumerable<TOutput> Core(IReceivableSourceBlock<TOutput> source, [EnumeratorCancellation] CancellationToken cancellationToken)
            {
                while (await source.OutputAvailableAsync(cancellationToken).ConfigureAwait(false))
                {
                    while (source.TryReceive(out TOutput? item))
                    {
                        yield return item;
                    }
                }
            }
        }
        public static async Task ReadBAsync(this Stream stream, byte[] buffer, int offset, int count,CancellationToken ct)
        {
            if (offset + count > buffer.Length)
                throw new ArgumentException();
            int read = 0;
            while (read < count)
            {
                var available = await stream.ReadAsync(buffer, offset, count - read, ct);
                if (available == 0)
                {
                    throw new ObjectDisposedException(null);
                }
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