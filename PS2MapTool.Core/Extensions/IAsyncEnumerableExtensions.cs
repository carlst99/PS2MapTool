using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PS2MapTool.Core.Extensions
{
    public static class IAsyncEnumerableExtensions
    {
        public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> items, CancellationToken ct = default)
        {
            var results = new List<T>();

            await foreach (var item in items.WithCancellation(ct).ConfigureAwait(false))
                results.Add(item);

            return results;
        }
    }
}
