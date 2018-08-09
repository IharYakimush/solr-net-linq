using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using SolrNet.Impl;
using SolrNet.Impl.DocumentPropertyVisitors;
using SolrNet.Impl.FieldParsers;
using SolrNet.Impl.ResponseParsers;
using SolrNet.Mapping;

namespace SolrNet.Linq.Impl
{
    public static class ExecuterExtensions
    {
        public static IExecuter<TNew> ChangeType<TNew, TOld>(this IExecuter<TOld> executer, ISolrFieldParser sfp = null)
        {
            try
            {
                SolrQueryExecuter<TOld> oldExecuter = executer.Executer;

                ISolrConnection connection = oldExecuter.GetSingleField<ISolrConnection>();
                ISolrQuerySerializer serializer = oldExecuter.GetSingleField<ISolrQuerySerializer>();
                ISolrFacetQuerySerializer facetQuerySerializer = oldExecuter.GetSingleField<ISolrFacetQuerySerializer>();

                sfp = sfp ?? new DefaultFieldParser();
                ISolrDocumentResponseParser<TNew> docParser;

                // Anonymous types can't be created by default SolrNet parsers, because the don't have property setters.
                if (CheckIfAnonymousType(typeof(TNew)))
                {
                    docParser =
                        new SelectResponseParser<TNew>(sfp);
                }
                else
                {
                    IReadOnlyMappingManager mapper = new AllPropertiesMappingManager();
                    docParser = new SolrDocumentResponseParser<TNew>(mapper, new DefaultDocumentVisitor(mapper, sfp),
                        new SolrDocumentActivator<TNew>());
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

        internal static T GetSingleField<T>(this object instance)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));

            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                                     | BindingFlags.Static;

            Type type = instance.GetType();

            FieldInfo field = type.GetFields(bindFlags).Single(info => info.FieldType == typeof(T));
            
            return (T)field.GetValue(instance);
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