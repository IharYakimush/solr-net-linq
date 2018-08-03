using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using SolrNet.Linq.Expressions.Context;

namespace SolrNet.Linq.Expressions.NodeTypeHelpers
{
    public static class MethodCallHelper
    {
        public static ISolrQuery HandleMethodCall(this MethodCallExpression mce, MemberContext context)
        {
            if ((mce.Method.DeclaringType == typeof(Queryable) || mce.Method.DeclaringType == typeof(Enumerable)) &&
                mce.Method.Name == nameof(Queryable.Any))
            {
                string field = context.GetSolrMemberProduct(mce.Arguments[0]);

                if (mce.Arguments.Count == 1)
                {
                    return new SolrHasValueQuery(field);
                }

                if (mce.Arguments.Count == 2)
                {
                    LambdaExpression lambda = (LambdaExpression) mce.Arguments[1];

                    return lambda.Body.GetSolrFilterQuery(MemberContext.ForLambda(lambda, field));
                }
            }

            if (mce.Method.Name == nameof(Queryable.Contains))
            {               
                // IEnumerable Contains or own e.g. for List<T>
                if (mce.Arguments.Count >= 2 || (mce.Arguments.Count == 1 && mce.Object != null))
                {
                    Expression obj = mce.Object ?? mce.Arguments[0];
                    Expression arg2 = mce.Object == null ? mce.Arguments[1] : mce.Arguments[0];

                    // Consider array member equal
                    if (context.HasMemberAccess(obj))
                    {
                        return new SolrQueryByField(context.GetSolrMemberProduct(obj, true),
                            context.GetSolrMemberProduct(arg2, true));
                    }

                    // Consider in list query
                    IEnumerable list;
                    try
                    {
                        list = (IEnumerable)Expression.Lambda(obj).Compile().DynamicInvoke();
                    }
                    catch (Exception e)
                    {
                        throw new InvalidOperationException(
                            $"Unable to get IEnumerable from '{obj}' expression.", e);
                    }

                    return new SolrQueryInList(context.GetSolrMemberProduct(arg2, true),
                        list.OfType<object>().Select(o => o.SerializeToSolr()));
                }
            }

            throw new NotImplementedException();
        }
    }
}