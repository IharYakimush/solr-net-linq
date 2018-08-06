using System;
using System.Linq;
using System.Linq.Expressions;
using SolrNet.Commands.Parameters;
using SolrNet.Linq.Expressions;
using SolrNet.Linq.Expressions.Context;

namespace SolrNet.Linq
{
    public class SolrQueryTranslator<TEntity> : ExpressionVisitor
    {
        public SolrNetLinqOptions SolrNetLinqOptions { get; }
        public QueryOptions Options { get; } = new QueryOptions();

        public ISolrQuery Query { get; } = SolrQuery.All;

        public SolrQueryTranslator(SolrNetLinqOptions solrNetLinqOptions)
        {
            SolrNetLinqOptions = solrNetLinqOptions ?? throw new ArgumentNullException(nameof(solrNetLinqOptions));
        }

        public Tuple<ISolrQuery,QueryOptions> Translate<T>(SolrQueryProvider<T> provider, Expression expression)
        {
            this.Visit(expression);
            return new Tuple<ISolrQuery, QueryOptions>(Query, Options);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            this.Visit(node.Arguments[0]);

            bool result = node.TryVisitTake(this.Options);
            result |= node.TryVisitSkip(this.Options);
            MemberContext context = MemberContext.ForType<TEntity>();
            context.FieldSerializer = this.SolrNetLinqOptions.SolrFieldSerializer;
            context.MappingManager = this.SolrNetLinqOptions.MappingManager;

            result |= node.TryVisitSorting(this.Options, context);
            result |= node.TryVisitWhere(this.Options, context);

            if (!result && !(node.Method.DeclaringType == typeof(Queryable) && node.Method.Name == nameof(Queryable.OfType)))
            {
                throw new InvalidOperationException($"Method '{node.Method.Name}' not supported.");
            }

            return node;
        }        
    }
}