using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using SolrNet.Commands.Parameters;
using SolrNet.Linq.Expressions.Context;

namespace SolrNet.Linq.Expressions
{
    public static class EnumerateMethod
    {
        public const string FirstOrDefault = nameof(Queryable.FirstOrDefault);
        public const string First = nameof(Queryable.First);
        public const string Single = nameof(Queryable.Single);
        public const string SingleOrDefault = nameof(Queryable.SingleOrDefault);

        public static EnumeratedResult TryVisitEnumerate(this MethodCallExpression node, QueryOptions options, MemberContext context)
        {
            EnumeratedResult result = EnumeratedResult.None;

            if (node.Method.Name == FirstOrDefault)
            {
                result = EnumeratedResult.FirstOrDefault;
            }
            if (node.Method.Name == First)
            {
                result = EnumeratedResult.First;
            }
            if (node.Method.Name == SingleOrDefault)
            {
                result = EnumeratedResult.SingleOrDefault;
            }
            if (node.Method.Name == Single)
            {
                result = EnumeratedResult.Single;
            }

            if (node.Method.DeclaringType == typeof(Queryable) && result != EnumeratedResult.None)
            {
                if (node.Arguments.Count == 2)
                {
                    Expression arg = node.Arguments[1];

                    if (arg.NodeType == ExpressionType.Quote)
                    {
                        LambdaExpression lambda = (LambdaExpression)node.Arguments[1].StripQuotes();
                        Expression whereMember = lambda.Body;

                        ISolrQuery filter = whereMember.GetSolrFilterQuery(context);
                        options.FilterQueries.Add(filter);
                    }
                    else
                    {
                        throw new InvalidOperationException($"Unable to translate '{node.Method.Name}' method filter. Unexpected node type {arg.NodeType}");
                    }
                }

                options.Rows = 2;

                if (result == EnumeratedResult.First || result == EnumeratedResult.FirstOrDefault)
                {
                    options.Rows = 1;
                }
            }

            return result;
        }
    }
}