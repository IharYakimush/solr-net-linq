using System;
using System.Linq.Expressions;
using SolrNet.Linq.Expressions.Context;

namespace SolrNet.Linq.Expressions.NodeTypeHelpers
{
    public static class MemberAccessHelper
    {
        public static ISolrQuery HandleMemberAccess(this MemberExpression memberExpression, MemberContext context)
        {
            if (memberExpression.Type != typeof(bool))
            {
                throw new InvalidOperationException(
                    $"Member '{memberExpression.Member.Name}' must be boolean to be a part of filter");
            }

            if (context.IsAccessToMember(memberExpression))
            {
                return new SolrQueryByField(context.GetSolrMemberProduct(memberExpression, true),
                    Expression.Constant(true).GetSolrMemberProduct(typeof(MemberAccessHelper)));
            }

            if (memberExpression.IsNullableMember())
            {
                if (memberExpression.Expression.HandleConversion() is MemberExpression inner)
                {
                    if (context.IsAccessToMember(inner))
                    {
                        if (memberExpression.Member.Name == nameof(Nullable<int>.HasValue))
                        {
                            return new SolrHasValueQuery(context.GetSolrMemberProduct(inner));
                        }

                        if (memberExpression.Member.Name == nameof(Nullable<int>.Value))
                        {
                            return new SolrQueryByField(context.GetSolrMemberProduct(inner),
                                Expression.Constant(true).GetSolrMemberProduct(typeof(MemberAccessHelper)));
                        }
                    }
                }
            }

            // try to calculate values
            return FilterExpressionExtensions.ConstantToConstant(memberExpression, Expression.Constant(true), (a, b) => (bool)a == (bool)b);
        }
    }
}