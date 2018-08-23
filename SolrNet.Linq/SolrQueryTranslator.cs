using System;
using System.Linq;
using System.Linq.Expressions;
using SolrNet.Commands.Parameters;
using SolrNet.Linq.Expressions;
using SolrNet.Linq.Expressions.Context;
using SolrNet.Linq.Expressions.NodeTypeHelpers;
using SolrNet.Linq.Impl;

namespace SolrNet.Linq
{
    public class SolrQueryTranslator : ExpressionVisitor
    {
        public SolrNetLinqOptions SolrNetLinqOptions { get; }
        public MemberContext Context { get; private set; }
        public SelectExpressionsCollection SelectExpressions { get; }
        public QueryOptions Options { get; }

        public ISolrQuery Query { get; } = SolrQuery.All;

        public EnumeratedResult EnumeratedResult = EnumeratedResult.None;

        public SolrQueryTranslator(SolrNetLinqOptions solrNetLinqOptions, MemberContext context, SelectExpressionsCollection selectExpressions)
        {
            SolrNetLinqOptions = solrNetLinqOptions ?? throw new ArgumentNullException(nameof(solrNetLinqOptions));
            Context = context ?? throw new ArgumentNullException(nameof(context));
            SelectExpressions = selectExpressions ?? throw new ArgumentNullException(nameof(selectExpressions));
            Options = selectExpressions.QueryOptions;
        }

        public Tuple<ISolrQuery,QueryOptions, EnumeratedResult> Translate<TEntity>(SolrQueryProvider<TEntity> provider, Expression expression)
        {
            this.Visit(expression);
            return new Tuple<ISolrQuery, QueryOptions, EnumeratedResult>(Query, Options, this.EnumeratedResult);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            this.Visit(node.Arguments[0]);

            bool result = node.TryVisitTake(this.Options);
            result = result || node.TryVisitSkip(this.Options);

            result = result || node.TryVisitSorting(this.Options, this.Context);
            result = result || node.TryVisitWhere(this.Options, this.Context);
            if (!result)
            {
                result = node.TryVisitSelect(this.SelectExpressions, this.Context, out var newContext);
                if (result)
                {
                    this.Context = newContext;
                }                
            }
            
            if (!result)
            {
                this.EnumeratedResult = node.TryVisitEnumerate(this.Options, this.Context);

                result = this.EnumeratedResult != EnumeratedResult.None;
            }

            if (!result)
            {
                result = node.Method.DeclaringType == typeof(Queryable) && node.Method.Name == nameof(Queryable.Cast);
            }

            if (!result)
            {
                result = node.Method.DeclaringType == typeof(Queryable) && node.Method.Name == nameof(Queryable.OfType);
            }

            if (!result)
            {
                throw new InvalidOperationException($"Method '{node.Method.Name}' not supported.");
            }

            return node;
        }        
    }
}