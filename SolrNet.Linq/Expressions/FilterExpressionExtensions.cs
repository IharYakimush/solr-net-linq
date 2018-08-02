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
                        List<ISolrQuery> queries = GetMultipleCriteriaQuery(left, right, op);
                        return new SolrMultipleCriteriaQuery(queries, op);
                    }

                    case ExpressionType.OrElse:
                    {
                        ISolrQuery left = GetSolrFilterQuery(binaryExpression.Left, type);
                        ISolrQuery right = GetSolrFilterQuery(binaryExpression.Right, type);

                        string op = SolrMultipleCriteriaQuery.Operator.OR;
                        List<ISolrQuery> queries = GetMultipleCriteriaQuery(left, right, op);
                        return new SolrMultipleCriteriaQuery(queries, op);
                    }

                    case ExpressionType.GreaterThan:
                    case ExpressionType.GreaterThanOrEqual:
                    case ExpressionType.LessThan:
                    case ExpressionType.LessThanOrEqual:
                    {
                        Tuple<MemberExpression, Expression, bool> memberToLeft = binaryExpression.MemberToLeft(type);
                        KeyValuePair<string, object> kvp = memberToLeft.MemberValue(type);
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
                            from = kvp.Value.SerializeToSolrDefault();
                        }
                        else
                        {
                            to = kvp.Value.SerializeToSolrDefault();
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

                return new SolrQueryByField(expression.GetSolrMemberProduct(type),
                    Expression.Constant(true).GetSolrMemberProduct(type));
            }

            throw new InvalidOperationException(
                $"Node type {nodeType} not supported in filter query");
        }

        private static List<ISolrQuery> GetMultipleCriteriaQuery(ISolrQuery left, ISolrQuery right, string criteriaOperator)
        {
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

            return queries;
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

        public static KeyValuePair<string, object> MemberValue(this Tuple<MemberExpression, Expression, bool> member, Type type)
        {
            string key = member.Item1.GetSolrMemberProduct(type);
            object dynamicInvoke;

            try
            {
                dynamicInvoke = Expression.Lambda(member.Item2).Compile().DynamicInvoke();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Unable to resolve value for {member.Item1}", e);
            }
            
            return new KeyValuePair<string, object>(key, dynamicInvoke);
        }
    }
}