using Microsoft.EntityFrameworkCore;
using System.Collections;

namespace ProDuck.Types
{
    public class PagedList<T> : IReadOnlyList<T>
    {
        private readonly IList<T> _subset;

        public PagedList(IEnumerable<T> items, int count, int pageNumber, int pageSize)
        {
            PageNumber = pageNumber;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            _subset = items as IList<T> ?? new List<T>();
            Count = count;
        }

        public int PageNumber { get; }
        public int TotalPages { get; }
        public int PageSize { get; }
        public bool IsFirstPage => PageNumber == 1;
        public bool IsLastPage => PageNumber == TotalPages;
        public int Count { get; }

        public T this[int index] => _subset[index];

        public IEnumerator<T> GetEnumerator() => _subset.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _subset.GetEnumerator();
    }
    public static class PagedListQueryableExtension
    {
        public static async Task<PagedList<T>> ToPagedListAsync<T>(this IQueryable<T> source, int page, int pageSize, CancellationToken token = default)
        {
            var count = await source.CountAsync(token);

            if (count > 0)
            {
                var items = await source
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(token);

                return new PagedList<T>(items, count, page, pageSize);
            }

            return new(Enumerable.Empty<T>(), 0, 0, 0);
        }
    }
}
