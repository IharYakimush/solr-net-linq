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

            Type declaringType = node.Method.DeclaringType;
            string methodName = node.Method.Name;

            if (declaringType == typeof(Queryable) && methodName == TakeMethod.Name)
            {
                TakeMethod.Visit(node, this.Options);
            }
            else if (declaringType == typeof(Queryable) && methodName == SkipMethod.Name)
            {
                SkipMethod.Visit(node, this.Options);
            }
            else if (!(declaringType == typeof(Queryable) && methodName == nameof(Queryable.OfType)))
            {
                throw new InvalidOperationException($"Method '{methodName}' not supported.");
            }

            return node;
        }

        private static Expression StripQuotes(Expression expression)
        {
            while (expression.NodeType == ExpressionType.Quote)
            {
                expression = ((UnaryExpression)expression).Operand;
            }

            return expression;
        }
    }
}