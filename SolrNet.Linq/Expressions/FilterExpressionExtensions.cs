using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using SolrNet.Linq.Expressions.NodeTypeHelpers;

namespace SolrNet.Linq.Expressions
{
    public static class FilterExpressionExtensions
    {
        public static ISolrQuery GetSolrFilterQuery(this Expression expression, Type type)
        {
            expression = expression.HandleConversion();
            ExpressionType nodeType = expression.NodeType;

            // No member access, try to calculate constant expression
            if (!expression.HasMemberAccess(type))
            {
                return ConstantToConstant(expression, Expression.Constant(true), (a, b) => (bool)a == (bool)b);
            }

            if (expression is BinaryExpression binaryExpression)
            {
                switch (nodeType)
                {
                    case ExpressionType.AndAlso:
                    {
                        ISolrQuery left = GetSolrFilterQuery(binaryExpression.Left, type);
                        ISolrQuery right = GetSolrFilterQuery(binaryExpression.Right, type);

                        string op = SolrMultipleCriteriaQuery.Operator.AND;
                        return GetMultipleCriteriaQuery(left, right, op);                        
                    }

                    case ExpressionType.OrElse:
                    {
                        ISolrQuery left = GetSolrFilterQuery(binaryExpression.Left, type);
                        ISolrQuery right = GetSolrFilterQuery(binaryExpression.Right, type);

                        string op = SolrMultipleCriteriaQuery.Operator.OR;
                        return GetMultipleCriteriaQuery(left, right, op);
                    }

                    case ExpressionType.Equal:
                    {
                        return binaryExpression.HandleEqual(type);
                    }

                    case ExpressionType.NotEqual:
                    {
                        Tuple<Expression, Expression, bool> memberToLeft = binaryExpression.MemberToLeft(type);
                        KeyValuePair<string, string> kvp = memberToLeft.MemberValue(type);

                        return kvp.Value == null
                            ? new SolrHasValueQuery(kvp.Key)
                            : new SolrQueryByField(kvp.Key, kvp.Value).CreateNotSolrQuery();

                    }

                    case ExpressionType.GreaterThan:
                    case ExpressionType.GreaterThanOrEqual:
                    case ExpressionType.LessThan:
                    case ExpressionType.LessThanOrEqual:
                    {
                        return binaryExpression.HandleComparison(type);
                    }
                }
            }

            if (expression is UnaryExpression unaryExpression)
            {
                switch (nodeType)
                {
                    case ExpressionType.Not:
                    {
                        ISolrQuery operand = GetSolrFilterQuery(unaryExpression.Operand, type);
                        ISolrQuery result = operand.CreateNotSolrQuery();
                        return result;
                    }                    
                }
            }

            if (expression is MemberExpression memberExpression)
            {
                return memberExpression.HandleMemberAccess(type);
            }            

            if (expression is ConditionalExpression conditionalExpression)
            {
                return ConditionalQuery(conditionalExpression, t => t, f => f, type);
            }            

            throw new InvalidOperationException(
                $"Node type {nodeType} not supported in filter query");
        }

        internal static ISolrQuery ConstantToConstant(Expression a, Expression b, Func<object, object, bool> valueCheck)
        {
            try
            {
                object v1 = Expression.Lambda(a).Compile().DynamicInvoke();
                object v2 = Expression.Lambda(b).Compile().DynamicInvoke();

                if (valueCheck(v1, v2))
                {
                    return SolrQuery.All;
                }

                return SolrQuery.All.CreateNotSolrQuery();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(
                    $"Unable to process check for values with expressions '{a}' and '{b}'", e);
            }
            
        }
        public static ISolrQuery ConditionalQuery(this ConditionalExpression expression, Func<Expression,Expression> ifTrueBuilder, Func<Expression, Expression> ifFalseBuilder, Type type)
        {
            ISolrQuery testPositive = expression.Test.GetSolrFilterQuery(type);
            ISolrQuery trueCase = ifTrueBuilder(expression.IfTrue).GetSolrFilterQuery(type);

            ISolrQuery testNegative = testPositive.CreateNotSolrQuery();
            ISolrQuery falseCase = ifFalseBuilder(expression.IfFalse).GetSolrFilterQuery(type);

            return GetMultipleCriteriaQuery(
                GetMultipleCriteriaQuery(testPositive, trueCase, SolrMultipleCriteriaQuery.Operator.AND),
                GetMultipleCriteriaQuery(testNegative, falseCase, SolrMultipleCriteriaQuery.Operator.AND),
                SolrMultipleCriteriaQuery.Operator.OR);
        }

        internal static ISolrQuery GetMultipleCriteriaQuery(ISolrQuery left, ISolrQuery right, string criteriaOperator)
        {
            left = left.TrySimplify();
            right = right.TrySimplify();
            if (left == right)
            {
                return left;
            }

            if (left == SolrQuery.All)
            {
                return right;
            }

            if (right == SolrQuery.All)
            {
                return left;
            }            

            SolrMultipleCriteriaQuery leftAnd = left as SolrMultipleCriteriaQuery;
            SolrMultipleCriteriaQuery rightAnd = right as SolrMultipleCriteriaQuery;

            List<ISolrQuery> queries = new List<ISolrQuery>();

            if (leftAnd != null && leftAnd.Oper == criteriaOperator)
            {
                queries.AddRange(leftAnd.Queries);
            }
            else
            {
                queries.Add(left);
            }

            if (rightAnd != null && rightAnd.Oper == criteriaOperator)
            {
                queries.AddRange(rightAnd.Queries);
            }
            else
            {
                queries.Add(right);
            }

            return new SolrMultipleCriteriaQuery(queries, criteriaOperator).TrySimplify();
        }

        internal static Tuple<Expression, Expression, bool> MemberToLeft(this BinaryExpression expression, Type type)
        {
            return MemberToLeft(expression.Left, expression.Right, type);
        }

        public static Tuple<Expression, Expression, bool> MemberToLeft(Expression l, Expression r, Type type)
        {
            Expression a = l.HandleConversion();
            Expression b = r.HandleConversion();

            if (a.HasMemberAccess(type))
            {
                return new Tuple<Expression, Expression, bool>(a, b, false);
            }

            if (b.HasMemberAccess(type))
            {
                return new Tuple<Expression, Expression, bool>(b, a, true);
            }

            throw new InvalidOperationException(
                $"Access to member of type '{type}' not found in both '{a}' and '{b}'.");
        }

        public static KeyValuePair<string, string> MemberValue(this Tuple<Expression, Expression, bool> member, Type type)
        {
            string key = member.Item1.GetSolrMemberProduct(type, true);
            string dynamicInvoke;

            try
            {
                dynamicInvoke = member.Item2.GetSolrMemberProduct(type, true);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Unable to resolve value for {member.Item1}", e);
            }
            
            return new KeyValuePair<string, string>(key, dynamicInvoke);
        }
    }
}