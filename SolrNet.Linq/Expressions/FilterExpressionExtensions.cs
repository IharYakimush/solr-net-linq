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
    }
}