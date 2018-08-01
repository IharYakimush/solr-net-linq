using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using SolrNet.Attributes;
using SolrNet.Impl.FieldSerializers;
using SolrNet.Schema;

namespace SolrNet.Linq.Expressions
{
    public static class ExpressionExtensions
    {
        private static readonly DefaultFieldSerializer DefaultFieldSerializer = new DefaultFieldSerializer();
        private static readonly ConcurrentDictionary<MemberInfo, string> MemberNames =
            new ConcurrentDictionary<MemberInfo, string>();
        public static Expression StripQuotes(this Expression expression)
        {
            while (expression.NodeType == ExpressionType.Quote)
            {
                expression = ((UnaryExpression)expression).Operand;
            }

            return expression;
        }

        public static string GetMemberSolrName(this MemberInfo info)
        {
            return MemberNames.GetOrAdd(info, m =>
            {
                SolrFieldAttribute att = m.GetCustomAttributes().OfType<SolrFieldAttribute>().FirstOrDefault();
                
                if (att == null)
                {
                    throw new InvalidOperationException($"Unable to get solr name for {m.DeclaringType}.{m.Name}");
                }

                return att.FieldName;
            });
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

        public static string GetSolrExpression(this Expression exp)
        {
            try
            {
                exp = exp.HandleConversion();

                if (exp is MemberExpression lambdaExp)
                {
                    MemberInfo memberInfo = lambdaExp.Member;

                    return memberInfo.GetMemberSolrName();
                }

                if (exp is BinaryExpression bin)
                {
                    if (bin.NodeType == ExpressionType.Divide)
                    {
                        return $"div({bin.Left.GetSolrExpression()},{bin.Right.GetSolrExpression()})";
                    }
                }

                if (exp.NodeType == ExpressionType.Constant)
                {
                    ConstantExpression constantExpression = (ConstantExpression)exp;

                    if (DefaultFieldSerializer.CanHandleType(constantExpression.Type))
                    {
                        return DefaultFieldSerializer.Serialize(constantExpression.Value).First().FieldValue;
                    }
                    
                    throw new InvalidOperationException($"Unable to serialize {constantExpression.Value} value");
                }
            }
            catch (InvalidOperationException exception)
            {
                throw new InvalidOperationException($"Unable to translate SOLR expression {exp}", exception);
            }

            throw new InvalidOperationException($"Unable to translate SOLR expression {exp}");
        }
    }
}