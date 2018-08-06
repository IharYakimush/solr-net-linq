using System;
using SolrNet.Commands.Parameters;
using SolrNet.Impl;

namespace SolrNet.Linq
{
    /// <summary>
    /// Options to configure SolrNet.Linq behavior and combine it with main SolrNet functionality 
    /// </summary>
    public class SolrNetLinqOptions
    {
        public ISolrQuery MainQuery { get; set; } = null;

        public Action<QueryOptions> SetupQueryOptions { get; set; } = null;

        public ISolrFieldSerializer SolrFieldSerializer { get; set; } = null;
    }
}