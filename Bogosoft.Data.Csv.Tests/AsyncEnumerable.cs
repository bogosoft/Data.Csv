using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bogosoft.Data.Csv.Tests
{
    static class AsyncEnumerable<T>
    {
        class EmptyAsyncEnumerable : IAsyncEnumerable<T>
        {
            class Enumerator : IAsyncEnumerator<T>
            {
                public T Current => throw new InvalidOperationException();

                public ValueTask DisposeAsync() => new ValueTask();

                public ValueTask<bool> MoveNextAsync() => new ValueTask<bool>(false);
            }

            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            {
                return new Enumerator();
            }
        }

        internal static IAsyncEnumerable<T> Empty => new EmptyAsyncEnumerable();
    }
}