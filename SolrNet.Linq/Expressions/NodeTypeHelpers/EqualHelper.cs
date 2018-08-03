using System;
using System.Linq.Expressions;

namespace SolrNet.Linq.Expressions.NodeTypeHelpers
{
    public static class EqualHelper
    {
        public static ISolrQuery HandleEqual(this BinaryExpression binaryExpression, Type type)
        {
            var nodeType = binaryExpression.NodeType;
            Tuple<Expression, Expression, bool> m = binaryExpression.MemberToLeft(type);
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
                return new SolrHasValueQuery(m.Item1.GetSolrMemberProduct(type)).CreateNotSolrQuery();
            }

            if (value is bool bv)
            {
                if (bv)
                {
                    return m.Item1.GetSolrFilterQuery(type);
                }

                if (!(m.Item1.Type == typeof(bool)))
                {
                    return m.Item1.GetSolrFilterQuery(type).CreateNotSolrQuery();
                }                
            }

            return new SolrQueryByField(m.Item1.GetSolrMemberProduct(type),
                Expression.Constant(value).GetSolrMemberProduct(type, true));
        }
    }
}