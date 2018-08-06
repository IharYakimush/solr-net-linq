using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using SolrNet.Commands.Parameters;
using SolrNet.Linq.Expressions.Context;

namespace SolrNet.Linq.Expressions
{
    public static class OrderByMethods
    {
        public const string OrderBy = nameof(Queryable.OrderBy);
        public const string OrderByDescending = nameof(Queryable.OrderByDescending);
        public const string ThenBy = nameof(Queryable.ThenBy);
        public const string ThenByDescending = nameof(Queryable.ThenByDescending);

        public static bool TryVisitSorting(this MethodCallExpression node, QueryOptions options, MemberContext context)
        {
            bool asc = node.Method.Name == OrderBy || node.Method.Name == ThenBy;
            bool desc = node.Method.Name == OrderByDescending || node.Method.Name == ThenByDescending;

            bool result = node.Method.DeclaringType == typeof(Queryable) && (asc || desc);

            if (result)
            {
                Visit(node, options, asc ? Order.ASC : Order.DESC, context);
            }

            return result;
        }

        private static void Visit(MethodCallExpression node, QueryOptions options, Order order, MemberContext context)
        {
            if (options.OrderBy.Any() && (node.Method.Name == OrderBy || node.Method.Name == OrderByDescending))
            {
                throw new InvalidOperationException(
                    $"Unable to apply {node.Method.Name} because ordering already applied. Try to use {ThenBy} or {ThenByDescending} instead");
            }

            Expression arg = node.Arguments[1];

            if (arg.NodeType == ExpressionType.Quote)
            {
                LambdaExpression lambda = (LambdaExpression)node.Arguments[1].StripQuotes();
                Expression orderingMember = lambda.Body;

                string solrExpression = context.GetSolrMemberProduct(orderingMember);
                options.OrderBy.Add(new SortOrder(solrExpression, order));
            }
            else
            {
                throw new InvalidOperationException($"Unable to translate '{node.Method.Name}' method. Unexpected node type {arg.NodeType}");
            }
        }                
    }
}