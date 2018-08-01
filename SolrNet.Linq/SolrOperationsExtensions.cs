using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolrNet.Linq
{
    public static class SolrOperationsExtensions
    {
        public static SolrQuery<T> AsQuerable<T>(this ISolrBasicReadOnlyOperations<T> operations)
        {
            return new SolrQuery<T>(new SolrQueryProvider<T>(operations));
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