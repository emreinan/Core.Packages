using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Persistence.Paging;

public static class IQueryablePaginateExtensions
{
	public static async Task<Paginate<T>> ToPaginateAsync<T>(
		this IQueryable<T> source,
		int index,
		int size,
		CancellationToken cancellationToken = default)
	{
		var count = await source.CountAsync(cancellationToken).ConfigureAwait(false);
		var items = await source.Skip(index * size).Take(size).ToListAsync(cancellationToken).ConfigureAwait(false);

		return new Paginate<T>
		{
			Index = index,
			Size = size,
			Count = count,
			Items = items,
			Pages = (int)Math.Ceiling(count / (double)size)
		};
	}
	public static Paginate<T> ToPaginate<T>(this IQueryable<T> source, int index, int size)
	{
		int count = source.Count();
		var items = source.Skip(index * size).Take(size).ToList();

		Paginate<T> list =
			new()
			{
				Index = index,
				Size = size,
				Count = count,
				Items = items,
				Pages = (int)Math.Ceiling(count / (double)size)
			};
		return list;
	}
}
