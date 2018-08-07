using System;
using System.Linq.Expressions;
using SolrNet.Linq.Expressions.Context;

namespace SolrNet.Linq.Expressions
{
    public class SelectQueryProvider<TEntity,TDocument> : SolrQueryProvider<TEntity, TDocument>
    {
        public SelectQueryProvider(ISolrBasicReadOnlyOperations<TDocument> operations, SolrNetLinqOptions options,
            MethodCallExpression selectExpression, MemberContext context) : base(operations, options, context)
        {
        }

        protected override TEntity GetEntity(TDocument document)
        {
            throw new NotImplementedException();
        }
    }
}