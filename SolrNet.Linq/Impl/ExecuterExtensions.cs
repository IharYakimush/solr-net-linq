using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Xsl;
using SolrNet.Impl;
using SolrNet.Impl.DocumentPropertyVisitors;
using SolrNet.Impl.FieldParsers;
using SolrNet.Impl.ResponseParsers;
using SolrNet.Mapping;

namespace SolrNet.Linq.Impl
{
    internal static class ExecuterExtensions
    {
        public static IExecuter<TNew> ChangeType<TNew, TOld>(
            this IExecuter<TOld> executer, 
            MethodCallExpression expression, 
            SelectExpressionsCollection selectExpressionsCollection)
        {
            if (executer == null) throw new ArgumentNullException(nameof(executer));
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            if (selectExpressionsCollection == null) throw new ArgumentNullException(nameof(selectExpressionsCollection));

            try
            {
                SolrQueryExecuter<TOld> oldExecuter = executer.Executer;

                ISolrConnection connection = oldExecuter.GetFieldRecursive<ISolrConnection>();
                ISolrQuerySerializer serializer = oldExecuter.GetFieldRecursive<ISolrQuerySerializer>();
                ISolrFacetQuerySerializer facetQuerySerializer = oldExecuter.GetFieldRecursive<ISolrFacetQuerySerializer>();                

                ISolrDocumentResponseParser<TOld> oldParser =
                    oldExecuter.GetFieldRecursive<ISolrDocumentResponseParser<TOld>>();

                ISolrFieldParser fieldParser = oldParser.GetFieldRecursive<ISolrFieldParser>();
                SolrDictionaryDocumentResponseParser dictionaryParser = new SolrDictionaryDocumentResponseParser(fieldParser);

                ISolrDocumentResponseParser<TNew> docParser;

                if (expression.Method.DeclaringType == typeof(Queryable) && expression.Method.Name == nameof(Queryable.Cast))
                {
                    docParser = new CastResponseParser<TNew, TOld>(oldParser, dictionaryParser);
                }
                else if (expression.Method.DeclaringType == typeof(Queryable) && expression.Method.Name == nameof(Queryable.Select))
                {
                    docParser = new SelectResponseParser<TNew, TOld>(oldParser,
                        dictionaryParser, expression,
                        selectExpressionsCollection);
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Unable to change query type from {typeof(TOld).Name} to {typeof(TNew).Name}. Method {expression.Method.Name} not supported");
                }
                                
                ISolrAbstractResponseParser<TNew> parser = new DefaultResponseParser<TNew>(docParser);

                SolrQueryExecuter<TNew> newExecuter = new SolrQueryExecuter<TNew>(parser, connection, serializer,
                    facetQuerySerializer,
                    new SolrMoreLikeThisHandlerQueryResultsParser<TNew>(Enumerable.Repeat(parser, 1)));

                newExecuter.DefaultHandler = oldExecuter.DefaultHandler;
                newExecuter.DefaultRows = oldExecuter.DefaultRows;
                newExecuter.MoreLikeThisHandler = oldExecuter.MoreLikeThisHandler;

                return new SelectQueryExecutor<TNew>(newExecuter);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(
                    $"Unable to change solr query executer from {typeof(TOld)} to {typeof(TNew)}.", e);
            }
        }       

        internal static T GetFieldRecursive<T>(this object instance, int limit = 5) where T:class 
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));

            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            Type type = instance.GetType();
            List<FieldInfo> fields = new List<FieldInfo>();
            fields.AddRange(type.GetFields(bindFlags));
            while (type.BaseType != null && type.BaseType != typeof(object))
            {
                type = type.BaseType;
                fields.AddRange(type.GetFields(bindFlags));
            }

            foreach (FieldInfo field in fields
                .OrderBy(info => typeof(T).IsAssignableFrom(info.FieldType) ? 0 : 1)
                .ThenBy(info => (info.GetValue(instance) as IEnumerable) == null ? 0: 1))
            {
                if (typeof(T).IsAssignableFrom(field.FieldType))
                {
                    return (T)field.GetValue(instance);
                }

                object fieldValue = field.GetValue(instance);

                if (fieldValue != null)
                {
                    if (limit > 0)
                    {
                        if (fieldValue is IEnumerable enumerable && !(fieldValue is IQueryable))
                        {
                            int loopLimit = 1000;
                            foreach (object obj in enumerable)
                            {
                                if (obj == null)
                                {
                                    continue;
                                }

                                T inner = obj.GetFieldRecursive<T>(limit - 1);

                                if (inner != null)
                                {
                                    return inner;
                                }

                                loopLimit--;

                                if (loopLimit < 0)
                                {
                                    break;
                                }
                            }
                        }
                        else
                        {
                            T inner = fieldValue.GetFieldRecursive<T>(limit - 1);

                            if (inner != null)
                            {
                                return inner;
                            }
                        }
                        
                    }                    
                }
            }

            return null;
        }

        private static bool CheckIfAnonymousType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            // HACK: The only way to detect anonymous types right now.
            return Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false)
                   && type.IsGenericType && type.Name.Contains("AnonymousType")
                   && (type.Name.StartsWith("<>") || type.Name.StartsWith("VB$"))
                   && (type.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic;
        }
    }
}