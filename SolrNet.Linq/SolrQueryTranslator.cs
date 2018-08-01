using System;
using System.Linq.Expressions;
using SolrNet.Commands.Parameters;

namespace SolrNet.Linq
{
    public static class SolrQueryTranslator
    {
        public static Tuple<ISolrQuery,QueryOptions> Translate<T>(SolrQueryProvider<T> provider, Expression expression, ISolrQuery solrQuery, QueryOptions options)
        {
            return new Tuple<ISolrQuery, QueryOptions>(solrQuery, options);
        }
    }
}