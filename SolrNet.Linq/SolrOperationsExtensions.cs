using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SolrNet.Commands.Parameters;
using SolrNet.Linq.Impl;

namespace SolrNet.Linq
{
    public static class SolrOperationsExtensions
    {
        public static IQueryable<T> AsQueryable<T>(this ISolrBasicReadOnlyOperations<T> operations, Action<SolrNetLinqOptions> setupOptions = null)
        {
            SolrNetLinqOptions options = new SolrNetLinqOptions();
            setupOptions?.Invoke(options);
            return new SolrQuery<T>(new SolrQueryProvider<T>(
                new SolrQueryExecuterWrapperBasicOperations<T>(operations),
                options, null));
        }       

        public static SolrQueryResults<T> ToSolrQueryResults<T>(this IQueryable<T> queryable)
        {
            return (SolrQueryResults<T>)queryable.Provider.Execute(queryable.Expression);
        }

        public static Task<SolrQueryResults<T>> ToSolrQueryResultsAsync<T>(this IQueryable<T> queryable)
        {            
            if (queryable.Provider is IAsyncProvider<T> solrProvider)
            {
                return solrProvider.ExecuteAsync<SolrQueryResults<T>>(queryable.Expression);
            }

            return Task.FromResult(ToSolrQueryResults(queryable));
        }        
    }
}