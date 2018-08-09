using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SolrNet.Linq.Impl
{
    public class ReplaceCalculatedVisitor : ExpressionVisitor
    {
        private readonly SelectExpressionsCollection _collection;
        private readonly Dictionary<string, object> _dictionary;

        public ReplaceCalculatedVisitor(SelectExpressionsCollection collection, Dictionary<string, object> dictionary)
        {
            _collection = collection ?? throw new ArgumentNullException(nameof(collection));
            _dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (this._collection.Computed.ContainsKey(node))
            {
                return Expression.Convert(Expression.Constant(_dictionary[this._collection.Computed[node]]), node.Type);
            }

            return base.VisitMethodCall(node);
        }
    }
}