using SolrNet.Attributes;
using SolrNet.Impl.FieldSerializers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace SolrNet.Linq.Expressions
{
    public static class MemberExpressionExtensions
    {
        private static readonly Dictionary<ExpressionType, Func<BinaryExpression, string>> BinaryHelper = new Dictionary<ExpressionType, Func<BinaryExpression, string>> {
            { ExpressionType.Divide ,(b) => $"div({b.Left.GetSolrMemberProduct()},{b.Right.GetSolrMemberProduct()})" },
            { ExpressionType.Subtract ,(b) => $"sub({b.Left.GetSolrMemberProduct()},{b.Right.GetSolrMemberProduct()})" },
            { ExpressionType.Multiply ,(b) => $"mul({b.Left.GetSolrMemberProduct()},{b.Right.GetSolrMemberProduct()})" },
            { ExpressionType.Add ,(b) => $"sum({b.Left.GetSolrMemberProduct()},{b.Right.GetSolrMemberProduct()})" },
        };

        private static readonly Dictionary<string, Func<MethodCallExpression, string>> CallHelper = new Dictionary<string, Func<MethodCallExpression, string>> {
            { typeof(Math).FullName + nameof(Math.Abs) ,(c) => $"abs({c.Arguments[0].GetSolrMemberProduct()})" },
            { typeof(Math).FullName + nameof(Math.Log10) ,(c) => $"log({c.Arguments[0].GetSolrMemberProduct()})" },
            { typeof(Math).FullName + nameof(Math.Max) ,(c) => $"max({c.Arguments[0].GetSolrMemberProduct()},{c.Arguments[1].GetSolrMemberProduct()})" },
            { typeof(Math).FullName + nameof(Math.Min) ,(c) => $"min({c.Arguments[0].GetSolrMemberProduct()},{c.Arguments[1].GetSolrMemberProduct()})" },
            { typeof(Math).FullName + nameof(Math.Pow) ,(c) => $"pow({c.Arguments[0].GetSolrMemberProduct()},{c.Arguments[1].GetSolrMemberProduct()})" },
            { typeof(Math).FullName + nameof(Math.Sqrt) ,(c) => $"sqrt({c.Arguments[0].GetSolrMemberProduct()})" },
        };

        private static readonly DefaultFieldSerializer DefaultFieldSerializer = new DefaultFieldSerializer();
        private static readonly ConcurrentDictionary<MemberInfo, string> MemberNames =
            new ConcurrentDictionary<MemberInfo, string>();

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

        public static string GetSolrMemberProduct(this Expression exp)
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
                    if (BinaryHelper.ContainsKey(bin.NodeType))
                    {
                        return BinaryHelper[bin.NodeType].Invoke(bin);
                    }
                }

                if (exp is MethodCallExpression call)
                {
                    string key = call.Method.DeclaringType.FullName + call.Method.Name;
                    if (CallHelper.ContainsKey(key))
                    {
                        return CallHelper[key].Invoke(call);
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
