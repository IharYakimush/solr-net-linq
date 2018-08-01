using System;
using System.Linq;
using System.Linq.Expressions;
using SolrNet.Commands.Parameters;

namespace SolrNet.Linq.Expressions
{
    public static class SkipMethod
    {
        public const string Skip = nameof(Queryable.Skip);

        public static bool TryVisitSkip(this MethodCallExpression node, QueryOptions options)
        {
            bool result = node.Method.DeclaringType == typeof(Queryable) && node.Method.Name == Skip;
            if (result)
            {
                if (options.StartOrCursor == null)
                {
                    Expression arg = node.Arguments[1];

                    int skip = (int) Expression.Lambda(arg).Compile().DynamicInvoke();

                    options.StartOrCursor = new StartOrCursor.Start(skip);
                }
                else
                {
                    throw new InvalidOperationException($"Unable to call {Skip} method more that 1 time");
                }
            }

            return result;
        }
    }
}