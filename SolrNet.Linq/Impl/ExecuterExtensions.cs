using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using SolrNet.Impl;
using SolrNet.Impl.DocumentPropertyVisitors;
using SolrNet.Impl.FieldParsers;
using SolrNet.Impl.ResponseParsers;
using SolrNet.Mapping;

namespace SolrNet.Linq.Impl
{
    internal static class ExecuterExtensions
    {
        public static IExecuter<TNew> ChangeType<TNew, TOld>(this IExecuter<TOld> executer, MethodCallExpression selectExpression, SelectExpressionsCollection selectExpressionsCollection)
        {
            if (executer == null) throw new ArgumentNullException(nameof(executer));
            if (selectExpression == null) throw new ArgumentNullException(nameof(selectExpression));
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

                ISolrDocumentResponseParser<TNew> docParser = new SelectResponseParser2<TNew, TOld>(oldParser,
                    new SolrDictionaryDocumentResponseParser(fieldParser), selectExpression,
                    selectExpressionsCollection);
                                
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

        //internal static T GetSingleField<T>(this object instance)
        //{
        //    if (instance == null) throw new ArgumentNullException(nameof(instance));

        //    BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
        //                             | BindingFlags.Static;

        //    Type type = instance.GetType();

        //    FieldInfo field = type.GetFields(bindFlags).Single(info => info.FieldType == typeof(T));
            
        //    return (T)field.GetValue(instance);
        //}

        internal static T GetFieldRecursive<T>(this object instance, int limit = 5) where T:class 
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));

            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            Type type = instance.GetType();
            FieldInfo[] fields = type.GetFields(bindFlags);
            

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
                        if (fieldValue is IEnumerable enumerable)
                        {
                            foreach (object obj in enumerable)
                            {
                                T inner = obj.GetFieldRecursive<T>(limit - 1);

                                if (inner != null)
                                {
                                    return inner;
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