using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Toore.ImageEncoders.Core
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> items, int chunkSize)
        {
            return new ChunkHelper<T>(items, chunkSize);
        }

        private sealed class ChunkHelper<T> : IEnumerable<IEnumerable<T>>
        {
            private readonly IEnumerable<T> _items;
            private readonly int _chunkSize;
            private bool _hasMoreItems;

            internal ChunkHelper(IEnumerable<T> items, int chunkSize)
            {
                _items = items;
                _chunkSize = chunkSize;
            }

            public IEnumerator<IEnumerable<T>> GetEnumerator()
            {
                using (var enumerator = _items.GetEnumerator())
                {
                    _hasMoreItems = enumerator.MoveNext();
                    while (_hasMoreItems)
                    {
                        yield return GetNextChunk(enumerator).ToList();
                    }
                }
            }

            private IEnumerable<T> GetNextChunk(IEnumerator<T> enumerator)
            {
                for (var i = 0; i < _chunkSize; ++i)
                {
                    yield return enumerator.Current;
                    _hasMoreItems = enumerator.MoveNext();
                    if (!_hasMoreItems)
                    {
                        yield break;
                    }
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}