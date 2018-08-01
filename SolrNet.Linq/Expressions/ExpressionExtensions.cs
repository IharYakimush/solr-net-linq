using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using SolrNet.Attributes;
using SolrNet.Schema;

namespace SolrNet.Linq.Expressions
{
    public static class ExpressionExtensions
    {
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
    }
}