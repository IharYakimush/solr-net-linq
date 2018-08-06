using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SolrNet.Commands.Parameters;

namespace SolrNet.Linq
{
    public static class SolrOperationsExtensions
    {
        public static SolrQuery<T> AsQueryable<T>(this ISolrBasicReadOnlyOperations<T> operations, Action<SolrNetLinqOptions> setupOptions = null)
        {
            SolrNetLinqOptions o = new SolrNetLinqOptions();
            setupOptions?.Invoke(o);
            return new SolrQuery<T>(new SolrQueryProvider<T>(operations, o));
        }

        public static SolrQueryResults<T> ToSolrQueryResults<T>(this IQueryable<T> queryable)
        {
            return queryable.Provider.Execute(queryable.Expression) as SolrQueryResults<T>;
        }

        public static Task<SolrQueryResults<T>> ToSolrQueryResultsAsync<T>(this IQueryable<T> queryable)
        {
            if (queryable.Provider is SolrQueryProvider<T> solrProvider)
            {
                return solrProvider.ExecuteAsync(queryable.Expression);
            }

            return Task.FromResult(ToSolrQueryResults(queryable));
        }
    }
}