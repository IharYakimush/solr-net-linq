using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using SolrNet.Linq.Expressions.Context;

namespace SolrNet.Linq.Expressions.NodeTypeHelpers
{
    public static class ComparisonHelper
    {
        public static ISolrQuery HandleComparison(this BinaryExpression binaryExpression, MemberContext context)
        {
            var nodeType = binaryExpression.NodeType;
            var memberToLeft = binaryExpression.MemberToLeft(context);

            if (memberToLeft.Item1 is ConditionalExpression ce)
            {
                Expression CompareBuilder(Expression exp)
                {
                    switch (binaryExpression.NodeType)
                    {
                        case ExpressionType.GreaterThan: return Expression.GreaterThan(exp, memberToLeft.Item2);
                        case ExpressionType.GreaterThanOrEqual: return Expression.GreaterThanOrEqual(exp, memberToLeft.Item2);
                        case ExpressionType.LessThan: return Expression.LessThan(exp, memberToLeft.Item2);
                        case ExpressionType.LessThanOrEqual: return Expression.LessThanOrEqual(exp, memberToLeft.Item2);
                    }

                    throw new NotSupportedException();
                }

                return ce.ConditionalQuery(CompareBuilder, CompareBuilder, context);
            }

            KeyValuePair<string, string> kvp = memberToLeft.MemberValue(context);
            string from = null;
            string to = null;
            bool directGreater = (nodeType == ExpressionType.GreaterThan ||
                                  nodeType == ExpressionType.GreaterThanOrEqual) &&
                                 !memberToLeft.Item3;

            bool reverseGreater = (nodeType == ExpressionType.LessThan ||
                                   nodeType == ExpressionType.LessThanOrEqual) &&
                                  memberToLeft.Item3;

            if (directGreater || reverseGreater)
            {
                from = kvp.Value;
            }
            else
            {
                to = kvp.Value;
            }

            bool inc = nodeType == ExpressionType.GreaterThanOrEqual ||
                       nodeType == ExpressionType.LessThanOrEqual;

            return new SolrQueryByRange<string>(kvp.Key, from, to, inc);
        }
    }
}