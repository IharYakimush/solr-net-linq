using System;
using System.Linq;
using System.Linq.Expressions;
using SolrNet.Commands.Parameters;

namespace SolrNet.Linq.Expressions
{
    public static class TakeMethod
    {
        public const string Name = nameof(Queryable.Take);
        public static void Visit(MethodCallExpression methodCallExpression, QueryOptions options)
        {
            if (options.Rows.HasValue)
            {
                throw new InvalidOperationException($"Unable to call {Name} method more that 1 time");
            }

            Expression arg = methodCallExpression.Arguments[1];

            int take = (int)Expression.Lambda(arg).Compile().DynamicInvoke();

            options.Rows = options.Rows.HasValue ? Math.Min(options.Rows.Value, take) : take;
        }
    }
}