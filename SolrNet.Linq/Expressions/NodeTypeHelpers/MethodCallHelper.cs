using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;

namespace SolrNet.Linq.Expressions.NodeTypeHelpers
{
    public static class MethodCallHelper
    {
        public static ISolrQuery HandleMethodCall(this MethodCallExpression mce, Type type)
        {
            if ((mce.Method.DeclaringType == typeof(Queryable) || mce.Method.DeclaringType == typeof(Enumerable)) &&
                mce.Method.Name == nameof(Queryable.Any))
            {
                string field = mce.Arguments[0].GetSolrMemberProduct(type);

                if (mce.Arguments.Count == 1)
                {
                    return new SolrHasValueQuery(field);
                }

                if (mce.Arguments.Count == 2)
                {
                    Type[] interfaces = mce.Arguments[0].Type.GetInterfaces();
                    Type ienumerable = interfaces.First(intf =>
                        intf.IsGenericType && intf.Name.StartsWith(nameof(IEnumerable)));

                    Type elemenType = ienumerable.GetGenericArguments().Single();
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
                    if (obj.HasMemberAccess(type))
                    {
                        return new SolrQueryByField(obj.GetSolrMemberProduct(type, true),
                            arg2.GetSolrMemberProduct(type, true));
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

                    return new SolrQueryInList(arg2.GetSolrMemberProduct(type, true),
                        list.OfType<object>().Select(o => o.SerializeToSolr()));
                }
            }

            throw new NotImplementedException();
        }
    }
}