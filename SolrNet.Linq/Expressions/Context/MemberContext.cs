using System;
using System.Linq.Expressions;

namespace SolrNet.Linq.Expressions.Context
{
    public abstract class MemberContext
    {
        public abstract bool HasMemberAccess(Expression expression);

        public abstract string GetSolrMemberProduct(Expression expression, bool disableFunctions = false);

        public abstract bool IsAccessToMember(MemberExpression expression);

        public static string TrueStringSerialized { get; } =
            Expression.Constant(true).GetSolrMemberProduct(typeof(MemberContext), true);

        public static MemberContext ForType<T>()
        {
            return new TypeContext(typeof(T));
        }

        public static MemberContext ForType(Type type)
        {
            return new TypeContext(type);
        }

        public static MemberContext ForLambda(LambdaExpression lambdaExpression, string fieldName)
        {
            return new ParamContext(lambdaExpression, fieldName);
        }
    }
}