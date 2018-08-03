using System;
using System.Linq.Expressions;
using SolrNet.Linq.Expressions.Context;

namespace SolrNet.Linq.Expressions.NodeTypeHelpers
{
    public static class EqualHelper
    {
        public static ISolrQuery HandleEqual(this BinaryExpression binaryExpression, MemberContext context)
        {
            var nodeType = binaryExpression.NodeType;
            Tuple<Expression, Expression, bool> m = binaryExpression.MemberToLeft(context);
            object value;
            try
            {
                value = Expression.Lambda(m.Item2).Compile().DynamicInvoke();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Unable to calculate value for '{m.Item2}' expression.", e);
            }

            if (value == null)
            {
                // Only member eq null supported with assumption that it not has value query
                return new SolrHasValueQuery(context.GetSolrMemberProduct(m.Item1)).CreateNotSolrQuery();
            }

            if (value is bool bv)
            {
                if (bv)
                {
                    return m.Item1.GetSolrFilterQuery(context);
                }

                if (!(m.Item1.Type == typeof(bool)))
                {
                    return m.Item1.GetSolrFilterQuery(context).CreateNotSolrQuery();
                }                
            }

            return new SolrQueryByField(context.GetSolrMemberProduct(m.Item1),
                context.GetSolrMemberProduct(Expression.Constant(value), true));
        }
    }
}