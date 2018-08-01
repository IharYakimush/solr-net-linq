using System;
using System.Linq;
using System.Linq.Expressions;
using SolrNet.Commands.Parameters;

namespace SolrNet.Linq.Expressions
{
    public static class TakeMethod
    {
        public const string Take = nameof(Queryable.Take);
        public static bool TryVisitTake(this MethodCallExpression node, QueryOptions options)
        {
            bool result = node.Method.DeclaringType == typeof(Queryable) && node.Method.Name == Take;
            if (result)
            {
                if (options.Rows.HasValue)
                {
                    throw new InvalidOperationException($"Unable to call {Take} method more that 1 time");
                }

                Expression arg = node.Arguments[1];

                int take = (int)Expression.Lambda(arg).Compile().DynamicInvoke();

                options.Rows = options.Rows.HasValue ? Math.Min(options.Rows.Value, take) : take;
            }

            return result;
        }
    }
}