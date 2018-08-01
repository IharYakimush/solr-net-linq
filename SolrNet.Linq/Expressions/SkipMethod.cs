using System;
using System.Linq;
using System.Linq.Expressions;
using SolrNet.Commands.Parameters;

namespace SolrNet.Linq.Expressions
{
    public static class SkipMethod
    {
        public const string Name = nameof(Queryable.Skip);

        public static void Visit(MethodCallExpression methodCallExpression, QueryOptions options)
        {
            if (options.StartOrCursor == null)
            {
                Expression arg = methodCallExpression.Arguments[1];

                int skip = (int)Expression.Lambda(arg).Compile().DynamicInvoke();

                options.StartOrCursor = new StartOrCursor.Start(skip);
            }
            else
            {
                throw new InvalidOperationException($"Unable to call {Name} method more that 1 time");
            }            
        }
    }
}