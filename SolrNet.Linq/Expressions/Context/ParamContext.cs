using System;
using System.Linq;
using System.Linq.Expressions;

namespace SolrNet.Linq.Expressions.Context
{
    public class ParamContext : MemberContext
    {
        private readonly string _fieldName;
        private ParameterExpression _expression;

        public ParamContext(LambdaExpression lambdaExpression, string fieldName)
        {
            if (lambdaExpression == null) throw new ArgumentNullException(nameof(lambdaExpression));
            _fieldName = fieldName ?? throw new ArgumentNullException(nameof(fieldName));
            this._expression = lambdaExpression.Parameters.Single();
        }
        public override bool HasMemberAccess(Expression expression)
        {
            expression = expression.HandleConversion();
            if (expression == _expression)
            {
                return true;
            }

            if (expression is BinaryExpression be)
            {
                return this.HasMemberAccess(be.Left) || this.HasMemberAccess(be.Right);
            }

            if (expression is UnaryExpression ue)
            {
                return this.HasMemberAccess(ue.Operand);
            }

            if (expression is MethodCallExpression mc)
            {
                return (mc.Object != null && this.HasMemberAccess(mc.Object)) || mc.Arguments.Any(this.HasMemberAccess);
            }

            if (expression is ConditionalExpression ce)
            {
                return this.HasMemberAccess(ce.Test) ||
                       this.HasMemberAccess(ce.IfTrue) ||
                       this.HasMemberAccess(ce.IfFalse);
            }

            return false;
        }

        public override string GetSolrMemberProduct(Expression expression, bool disableFunctions = false)
        {
            if (expression ==_expression )
            {
                return _fieldName;
            }

            return expression.GetSolrMemberProduct(typeof(ParamContext), disableFunctions);
        }

        public override bool IsAccessToMember(MemberExpression expression)
        {
            throw new NotSupportedException();
        }

        public override string ToString()
        {
            return $"NestedField: {_fieldName} as {_expression}";
        }
    }
}