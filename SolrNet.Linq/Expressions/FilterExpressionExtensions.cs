using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

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

                    case ExpressionType.NotEqual:
                    {
                        Tuple<Expression, Expression, bool> memberToLeft = binaryExpression.MemberToLeft(type);
                        KeyValuePair<string, string> kvp = memberToLeft.MemberValue(type);

                        return kvp.Value == null
                            ? new SolrHasValueQuery(kvp.Key)
                            : CreateNotSolrQuery(new SolrQueryByField(kvp.Key, kvp.Value));

                    }

                    case ExpressionType.GreaterThan:
                    case ExpressionType.GreaterThanOrEqual:
                    case ExpressionType.LessThan:
                    case ExpressionType.LessThanOrEqual:
                    {
                        var memberToLeft = binaryExpression.MemberToLeft(type);

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

                            return Conditional(ce, CompareBuilder, CompareBuilder, type);
                        }
                        
                        KeyValuePair<string, string> kvp = memberToLeft.MemberValue(type);
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

            if (expression is UnaryExpression unaryExpression)
            {
                switch (nodeType)
                {
                    case ExpressionType.Not:
                    {
                        ISolrQuery operand = GetSolrFilterQuery(unaryExpression.Operand, type);
                        ISolrQuery result = CreateNotSolrQuery(operand);
                        return result;
                    }                    
                }
            }

            if (expression is MemberExpression memberExpression)
            {
                if (memberExpression.Type != typeof(bool))
                {
                    throw new InvalidOperationException(
                        $"Member '{memberExpression.Member.Name}' must be boolean to be a part of filter");
                }

                if (memberExpression.Member.DeclaringType == type)
                {
                    return new SolrQueryByField(expression.GetSolrMemberProduct(type),
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
                return ConstantToConstant(expression, Expression.Constant(true), (a, b) => (bool)a == (bool)b);
            }            

            if (expression is ConditionalExpression conditionalExpression)
            {
                return Conditional(conditionalExpression, t => t, f => f, type);
            }            

            throw new InvalidOperationException(
                $"Node type {nodeType} not supported in filter query");
        }

        private static ISolrQuery ConstantToConstant(Expression a, Expression b, Func<object, object, bool> valueCheck)
        {
            try
            {
                object v1 = Expression.Lambda(a).Compile().DynamicInvoke();
                object v2 = Expression.Lambda(b).Compile().DynamicInvoke();

                if (valueCheck(v1, v2))
                {
                    return SolrQuery.All;
                }

                return CreateNotSolrQuery(SolrQuery.All);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(
                    $"Unable to process check for values with expressions '{a}' and '{b}'", e);
            }
            
        }

        private static ISolrQuery Conditional(ConditionalExpression expression,Func<Expression,Expression> ifTrueBuilder, Func<Expression, Expression> ifFalseBuilder, Type type)
        {
            ISolrQuery testPositive = expression.Test.GetSolrFilterQuery(type);
            ISolrQuery trueCase = ifTrueBuilder(expression.IfTrue).GetSolrFilterQuery(type);

            ISolrQuery testNegative = CreateNotSolrQuery(testPositive);
            ISolrQuery falseCase = ifFalseBuilder(expression.IfFalse).GetSolrFilterQuery(type);

            return GetMultipleCriteriaQuery(
                GetMultipleCriteriaQuery(testPositive, trueCase, SolrMultipleCriteriaQuery.Operator.AND),
                GetMultipleCriteriaQuery(testNegative, falseCase, SolrMultipleCriteriaQuery.Operator.AND),
                SolrMultipleCriteriaQuery.Operator.OR);
        }

        private static ISolrQuery GetMultipleCriteriaQuery(ISolrQuery left, ISolrQuery right, string criteriaOperator)
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

        private static ISolrQuery CreateNotSolrQuery(ISolrQuery operand)
        {
            if (operand is SolrMultipleCriteriaQuery notQuery)
            {
                if (notQuery.Oper == "NOT")
                {
                    return notQuery.Queries.ElementAt(1);
                }
            }

            return new SolrMultipleCriteriaQuery(new[] {SolrQuery.All, operand}, "NOT");
        }

        public static Tuple<Expression, Expression, bool> MemberToLeft(this BinaryExpression expression, Type type)
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