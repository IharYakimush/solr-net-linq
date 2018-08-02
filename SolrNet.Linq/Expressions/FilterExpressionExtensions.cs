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
                        Tuple<MemberExpression, Expression, bool> memberToLeft = binaryExpression.MemberToLeft(type);
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
                        Tuple<MemberExpression, Expression, bool> memberToLeft;
                        try
                        {
                            memberToLeft = binaryExpression.MemberToLeft(type);
                        }
                        // No member access, try to calculate constant expression
                        catch (InvalidOperationException)
                        {
                            throw;
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
                else
                {
                    // try to calculate
                    return ConstantToConstant(expression, Expression.Constant(true), (a, b) => (bool)a == (bool)b);
                }
            }

            if (expression is ConstantExpression constantExpression)
            {
                return ConstantToConstant(expression, Expression.Constant(true), (a, b) => (bool)a == (bool)b);
            }

            if (expression is ConditionalExpression conditionalExpression)
            {
                throw new NotImplementedException();
                //return Conditional(conditionalExpression, true, type);
            }

            throw new InvalidOperationException(
                $"Node type {nodeType} not supported in filter query");
        }

        private static ISolrQuery ConstantToConstant(Expression a, Expression b, Func<object, object, bool> valueCheck)
        {
            object v1 = Expression.Lambda(a).Compile().DynamicInvoke();
            object v2 = Expression.Lambda(b).Compile().DynamicInvoke();

            if (valueCheck(v1,v2))
            {
                return SolrQuery.All;
            }

            return CreateNotSolrQuery(SolrQuery.All);
        }

        private static ISolrQuery Conditional(ConditionalExpression expression, Func<object,bool> valueCheck, Type type)
        {
            throw new NotImplementedException();
            Tuple<MemberExpression, Expression, bool> memberToLeft =
                MemberToLeft(expression.IfTrue, expression.IfFalse, type);
        }

        private static ISolrQuery GetMultipleCriteriaQuery(ISolrQuery left, ISolrQuery right, string criteriaOperator)
        {
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

            return new SolrMultipleCriteriaQuery(queries, criteriaOperator);
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

        public static Tuple<MemberExpression, Expression, bool> MemberToLeft(this BinaryExpression expression, Type type)
        {
            return MemberToLeft(expression.Left, expression.Right, type);
        }

        public static Tuple<MemberExpression, Expression, bool> MemberToLeft(Expression l, Expression r, Type type)
        {
            Expression a = l.HandleConversion();
            Expression b = r.HandleConversion();

            if (a is MemberExpression am && am.Member.DeclaringType == type)
            {
                return new Tuple<MemberExpression, Expression, bool>(am, b, false);
            }

            if (b is MemberExpression bm && bm.Member.DeclaringType == type)
            {
                return new Tuple<MemberExpression, Expression, bool>(bm, a, true);
            }

            throw new InvalidOperationException(
                $"Access to member of type '{type}' not found in both '{a}' and '{b}'.");
        }

        public static KeyValuePair<string, string> MemberValue(this Tuple<MemberExpression, Expression, bool> member, Type type)
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