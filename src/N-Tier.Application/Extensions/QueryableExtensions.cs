using Microsoft.EntityFrameworkCore;
using N_Tier.Application.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace N_Tier.Application.Extensions;

public static class QueryableExtensions
{
    /// <summary>
    /// Paginates an IQueryable query and returns a PagedResponse.
    /// </summary>
    public static async Task<PagedResponse<T>> ToPagedResponseAsync<T>(
        this IQueryable<T> query,
        int page,
        int size)
    {
        var total = await query.CountAsync();
        var results = await query
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        return new PagedResponse<T>(results, total, page, size);
    }

    /// <summary>
    /// Paginates an IQueryable query and maps the results to a target model using Mapster before returning a PagedResponse.
    /// </summary>
    public static async Task<PagedResponse<TResult>> ToPagedResponseAsync<TSource, TResult>(
        this IQueryable<TSource> query,
        int page,
        int size)
    {
        var total = await query.CountAsync();
        var sourceResults = await query
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        var mappedResults = Mapster.TypeAdapter.Adapt<List<TResult>>(sourceResults);
        return new PagedResponse<TResult>(mappedResults, total, page, size);
    }
}
