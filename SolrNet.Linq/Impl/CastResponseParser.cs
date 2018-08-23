using System.Collections.Generic;
using System.Linq;
using SolrNet.Impl;

namespace SolrNet.Linq.Impl
{
    public class CastResponseParser<TNew, TOld> : TransformationResponseParser<TNew, TOld>
    {
        public CastResponseParser(ISolrDocumentResponseParser<TOld> inner,
            ISolrDocumentResponseParser<Dictionary<string, object>> dictionaryParser) : base(inner, dictionaryParser)
        {
        }

        protected override TNew GetResult(TOld old, Dictionary<string, object> dictionary)
        {
            return Enumerable.Repeat(old, 1).Cast<TNew>().Single();
        }
    }
}