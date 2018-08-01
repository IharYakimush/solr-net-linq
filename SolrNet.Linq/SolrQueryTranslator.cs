using System;
using System.Linq;
using System.Linq.Expressions;
using SolrNet.Commands.Parameters;
using SolrNet.Linq.Expressions;

namespace SolrNet.Linq
{
    public class SolrQueryTranslator : ExpressionVisitor
    {
        public ISolrQuery SolrQuery { get; }
        public QueryOptions Options { get; }

        public SolrQueryTranslator(ISolrQuery solrQuery, QueryOptions options)
        {
            SolrQuery = solrQuery ?? throw new ArgumentNullException(nameof(solrQuery));
            Options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public Tuple<ISolrQuery,QueryOptions> Translate<T>(SolrQueryProvider<T> provider, Expression expression)
        {
            this.Visit(expression);
            return new Tuple<ISolrQuery, QueryOptions>(SolrQuery, Options);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            this.Visit(node.Arguments[0]);

            bool result = node.TryVisitTake(this.Options);
            result |= node.TryVisitSkip(this.Options);
            result |= node.TryVisitSorting(this.Options);
            
            if (!result && !(node.Method.DeclaringType == typeof(Queryable) && node.Method.Name == nameof(Queryable.OfType)))
            {
                throw new InvalidOperationException($"Method '{node.Method.Name}' not supported.");
            }

            return node;
        }        
    }
}