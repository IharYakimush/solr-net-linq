using System;
using SolrNet.Commands.Parameters;
using SolrNet.Impl;
using SolrNet.Impl.FieldParsers;
using SolrNet.Impl.FieldSerializers;
using SolrNet.Mapping;

namespace SolrNet.Linq
{
    /// <summary>
    /// Options to configure SolrNet.Linq behavior and combine it with main SolrNet functionality 
    /// </summary>
    public class SolrNetLinqOptions
    {
        /// <summary>
        /// Set main query (q SOLR parameter). By default *:* will be used. LINQ Where method append fq (Filter Query) to query options and not affect main query.
        /// </summary>
        public ISolrQuery MainQuery { get; set; } = null;

        /// <summary>
        /// Action to perform additional setup of <see cref="QueryOptions"/> SolrNet QueryOptions. Useful to set options not supported by LINQ. Will be applied after translating LINQ expression. 
        /// </summary>
        public Action<QueryOptions> SetupQueryOptions { get; set; } = null;

        /// <summary>
        /// Set <see cref="ISolrFieldSerializer"/> field serializer. If not set <see cref="DefaultFieldSerializer"/> default SolrNet field serializer will be used.
        /// </summary>
        public ISolrFieldSerializer SolrFieldSerializer { get; set; } = null;

        /// <summary>
        /// Set <see cref="IReadOnlyMappingManager"/> field mapping manager. If not set <see cref="AttributesMappingManager"/> attributes mapping manager will be used. More about SolrNet mapping: https://github.com/SolrNet/SolrNet/blob/master/Documentation/Mapping.md
        /// </summary>
        public IReadOnlyMappingManager MappingManager { get; set; } = null;

        ///// <summary>
        ///// Set solr field parser which will be used in case of Select() method applying. If not set <see cref="DefaultFieldParser"/> will be used.
        ///// </summary>
        //public ISolrFieldParser SolrFieldParser { get; set; } = null;
    }
}