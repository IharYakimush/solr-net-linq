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
        private static readonly Dictionary<ExpressionType, Func<BinaryExpression, Type, string>> BinaryHelper = new Dictionary<ExpressionType, Func<BinaryExpression, Type, string>> {
            { ExpressionType.Divide ,(b,t) => $"div({b.Left.GetSolrMemberProduct(t)},{b.Right.GetSolrMemberProduct(t)})" },
            { ExpressionType.Subtract ,(b,t) => $"sub({b.Left.GetSolrMemberProduct(t)},{b.Right.GetSolrMemberProduct(t)})" },
            { ExpressionType.Multiply ,(b,t) => $"mul({b.Left.GetSolrMemberProduct(t)},{b.Right.GetSolrMemberProduct(t)})" },
            { ExpressionType.Add ,(b,t) => $"sum({b.Left.GetSolrMemberProduct(t)},{b.Right.GetSolrMemberProduct(t)})" },
        };

        private static readonly Dictionary<string, Func<MethodCallExpression, Type, string>> CallHelper = new Dictionary<string, Func<MethodCallExpression, Type, string>> {
            { typeof(Math).FullName + nameof(Math.Abs) ,(c,t) => $"abs({c.Arguments[0].GetSolrMemberProduct(t)})" },
            { typeof(Math).FullName + nameof(Math.Log10) ,(c,t) => $"log({c.Arguments[0].GetSolrMemberProduct(t)})" },
            { typeof(Math).FullName + nameof(Math.Max) ,(c,t) => $"max({c.Arguments[0].GetSolrMemberProduct(t)},{c.Arguments[1].GetSolrMemberProduct(t)})" },
            { typeof(Math).FullName + nameof(Math.Min) ,(c,t) => $"min({c.Arguments[0].GetSolrMemberProduct(t)},{c.Arguments[1].GetSolrMemberProduct(t)})" },
            { typeof(Math).FullName + nameof(Math.Pow) ,(c,t) => $"pow({c.Arguments[0].GetSolrMemberProduct(t)},{c.Arguments[1].GetSolrMemberProduct(t)})" },
            { typeof(Math).FullName + nameof(Math.Sqrt) ,(c,t) => $"sqrt({c.Arguments[0].GetSolrMemberProduct(t)})" },
        };

        private static readonly DefaultFieldSerializer DefaultFieldSerializer = new DefaultFieldSerializer();

        private static readonly ConcurrentDictionary<MemberInfo, string> MemberNames = new ConcurrentDictionary<MemberInfo, string>();

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

        internal static string SerializeToSolrDefault(this object value)
        {
            return DefaultFieldSerializer.Serialize(value).First().FieldValue;
        }

        public static string GetSolrMemberProduct(this Expression exp, Type type, bool disableFunctions = false)
        {
            try
            {
                exp = exp.HandleConversion();

                if (exp is MemberExpression lambdaExp)
                {
                    MemberInfo memberInfo = lambdaExp.Member;

                    if (memberInfo.DeclaringType == type)
                    {
                        return memberInfo.GetMemberSolrName();
                    }                    
                }

                if (!disableFunctions && exp is BinaryExpression bin)
                {
                    if (BinaryHelper.ContainsKey(bin.NodeType))
                    {
                        return BinaryHelper[bin.NodeType].Invoke(bin, type);
                    }
                }

                if (!disableFunctions && exp is MethodCallExpression call)
                {
                    string key = call.Method.DeclaringType.FullName + call.Method.Name;
                    if (CallHelper.ContainsKey(key))
                    {
                        return CallHelper[key].Invoke(call, type);
                    }
                }

                // Access to member of other type can't be translated, so assume it should be used as a value
                object value = Expression.Lambda(exp).Compile().DynamicInvoke();
                if (value == null)
                {
                    return null;
                }

                if (DefaultFieldSerializer.CanHandleType(value.GetType()))
                {
                    return DefaultFieldSerializer.Serialize(value).First().FieldValue;
                }

                throw new InvalidOperationException($"Unable to serialize '{value}'.");
            }
            catch (InvalidOperationException exception)
            {
                throw new InvalidOperationException($"Unable to translate SOLR expression {exp}", exception);
            }            

            throw new InvalidOperationException($"Unable to translate SOLR expression {exp}");
        }
    }
}
