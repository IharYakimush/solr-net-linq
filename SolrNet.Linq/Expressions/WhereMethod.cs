using System;
using System.Linq;
using System.Linq.Expressions;
using SolrNet.Commands.Parameters;

namespace SolrNet.Linq.Expressions
{
    public static class WhereMethod
    {
        public const string Where = nameof(Queryable.Where);

        public static bool TryVisitWhere(this MethodCallExpression node, QueryOptions options)
        {
            bool result = node.Method.DeclaringType == typeof(Queryable) && node.Method.Name == Where;
            if (result)
            {
                Expression arg = node.Arguments[1];

                if (arg.NodeType == ExpressionType.Quote)
                {
                    LambdaExpression lambda = (LambdaExpression)node.Arguments[1].StripQuotes();
                    Expression whereMember = lambda.Body;

                    ISolrQuery filter = whereMember.GetSolrFilterQuery();
                    options.FilterQueries.Add(filter);
                }
                else
                {
                    throw new InvalidOperationException($"Unable to translate '{Where}' method. Unexpected node type {arg.NodeType}");
                }
            }

            return result;
        }
    }
}