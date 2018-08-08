using System;
using System.Linq;
using System.Reflection;
using SolrNet.Impl;
using SolrNet.Impl.DocumentPropertyVisitors;
using SolrNet.Impl.FieldParsers;
using SolrNet.Impl.ResponseParsers;
using SolrNet.Mapping;

namespace SolrNet.Linq.Impl
{
    public static class ExecuterExtensions
    {
        public static IExecuter<TNew> ChangeType<TNew, TOld>(this IExecuter<TOld> executer)
        {
            try
            {
                SolrQueryExecuter<TOld> oldExecuter = executer.Executer;

                ISolrConnection connection = oldExecuter.GetSingleField<ISolrConnection>();
                ISolrQuerySerializer serializer = oldExecuter.GetSingleField<ISolrQuerySerializer>();
                ISolrFacetQuerySerializer facetQuerySerializer = oldExecuter.GetSingleField<ISolrFacetQuerySerializer>();

                //TODO: 
                IReadOnlyMappingManager mapper = new AllPropertiesMappingManager();

                ISolrFieldParser sfp = new DefaultFieldParser();
                ISolrDocumentPropertyVisitor sdpv = new DefaultDocumentVisitor(mapper, sfp);

                ISolrAbstractResponseParser<TNew> parser =
                    new DefaultResponseParser<TNew>(
                        new SolrDocumentResponseParser<TNew>(mapper, sdpv, new SolrDocumentActivator<TNew>()));

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
    }
}