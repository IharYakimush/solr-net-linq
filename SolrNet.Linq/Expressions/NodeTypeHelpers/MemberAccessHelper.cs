using System;
using System.Linq.Expressions;

namespace SolrNet.Linq.Expressions.NodeTypeHelpers
{
    public static class MemberAccessHelper
    {
        public static ISolrQuery HandleMemberAccess(this MemberExpression memberExpression, Type type)
        {
            if (memberExpression.Type != typeof(bool))
            {
                throw new InvalidOperationException(
                    $"Member '{memberExpression.Member.Name}' must be boolean to be a part of filter");
            }

            if (memberExpression.Member.DeclaringType == type)
            {
                return new SolrQueryByField(memberExpression.GetSolrMemberProduct(type),
                    Expression.Constant(true).GetSolrMemberProduct(type));
            }

            if (memberExpression.IsNullableMember())
            {
                if (memberExpression.Expression.HandleConversion() is MemberExpression inner)
                {
                    if (inner.Member.DeclaringType == type)
                    {
                        if (memberExpression.Member.Name == nameof(Nullable<int>.HasValue))
                        {
                            return new SolrHasValueQuery(inner.GetSolrMemberProduct(type));
                        }

                        if (memberExpression.Member.Name == nameof(Nullable<int>.Value))
                        {
                            return new SolrQueryByField(inner.GetSolrMemberProduct(type),
                                Expression.Constant(true).GetSolrMemberProduct(type));
                        }
                    }
                }
            }

            // try to calculate values
            return FilterExpressionExtensions.ConstantToConstant(memberExpression, Expression.Constant(true), (a, b) => (bool)a == (bool)b);
        }
    }
}