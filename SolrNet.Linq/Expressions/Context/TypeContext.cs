using System;
using System.Linq.Expressions;

namespace SolrNet.Linq.Expressions.Context
{
    public class TypeContext : MemberContext
    {
        private readonly Type _type;

        public TypeContext(Type type)
        {
            _type = type ?? throw new ArgumentNullException(nameof(type));
        }
        public override bool HasMemberAccess(Expression expression)
        {
            return expression.HasMemberAccess(_type);
        }

        public override string GetSolrMemberProduct(Expression expression, bool disableFunctions = false)
        {
            return expression.GetSolrMemberProduct(_type, disableFunctions);
        }

        public override bool IsAccessToMember(MemberExpression expression)
        {
            return expression.Member.DeclaringType == _type;
        }

        public override string ToString()
        {
            return $"EntityType: {_type}";
        }
    }
}