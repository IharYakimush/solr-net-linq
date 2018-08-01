using System.Linq.Expressions;

namespace SolrNet.Linq.Expressions
{
    public static class ExpressionExtensions
    {        
        public static Expression StripQuotes(this Expression expression)
        {
            while (expression.NodeType == ExpressionType.Quote)
            {
                expression = ((UnaryExpression)expression).Operand;
            }

            return expression;
        }        

        public static Expression HandleConversion(this Expression expression)
        {
            if (expression.NodeType == ExpressionType.Convert)
            {
                if (expression is UnaryExpression orderingMemberConvert)
                {
                    expression = HandleConversion(orderingMemberConvert.Operand);
                }
            }

            return expression;
        }        
    }
}