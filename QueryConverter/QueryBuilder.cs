using Microsoft.AspNetCore.Http;
using System.Linq.Dynamic.Core;

namespace QueryConverter;

public static class QueryBuilder
{
    public static QueryOutcome<T> Build<T>(this IQueryable<T> source, IQueryCollection query) where T : class
    {
        var outcome = new QueryOutcome<T>();
        SearchOptions options = SearchOptions.Create<T>(query);
        if (options.Success == false)
        {
            outcome.ErrorMessage = options.ErrorMessage;
            outcome.Success = options.Success;
            return outcome;
        }
        if (options.ShouldSearch)
        {
            source = source.Where(options.Search);
        }
        if (options.Sorting)
        {
            source = source.OrderBy(options.SortString);
        }
        if (options.Limit > 0)
        {
            source = source.Take(options.Limit);
        }
        if (options.Offset > 0)
        {
            source = source.Skip(options.Offset);
        }
        outcome.Query = source;
        return outcome;

    }
}
