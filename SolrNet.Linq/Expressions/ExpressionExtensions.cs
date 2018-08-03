﻿using System;
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

        public static bool HasMemberAccess(this Expression expression, Type type)
        {
            expression = expression.HandleConversion();
            
            if (expression is MemberExpression me)
            {
                if (me.Member.DeclaringType == type)
                {
                    return true;
                }

                if (me.Member.DeclaringType != null && 
                    me.Member.DeclaringType.IsGenericType &&
                    me.Member.DeclaringType.Name.StartsWith(nameof(Nullable)))
                {
                    return me.Expression.HasMemberAccess(type);
                }
            }

            if (expression is BinaryExpression be)
            {
                return be.Left.HasMemberAccess(type) || be.Right.HasMemberAccess(type);
            }

            if (expression is UnaryExpression ue)
            {
                return ue.Operand.HasMemberAccess(type);
            }            

            if (expression is ConditionalExpression ce)
            {
                return ce.Test.HasMemberAccess(type) || 
                       ce.IfTrue.HasMemberAccess(type) ||
                       ce.IfFalse.HasMemberAccess(type);
            }            

            return false;
        }
    }
}