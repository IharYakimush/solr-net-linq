using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Xml.Linq;
using SolrNet.Impl;
using SolrNet.Linq.Expressions;

namespace SolrNet.Linq.Impl
{
    public class SelectResponseParser2<TNew,TOld> : ISolrDocumentResponseParser<TNew>
    {
        private readonly ISolrDocumentResponseParser<TOld> _inner;
        private readonly ISolrDocumentResponseParser<Dictionary<string, object>> _dictionaryParser;
        private readonly MethodCallExpression _selectCall;
        private readonly SelectExpressionsCollection _selectState;

        public SelectResponseParser2(ISolrDocumentResponseParser<TOld> inner, ISolrDocumentResponseParser<Dictionary<string, object>> dictionaryParser, MethodCallExpression selectCall, SelectExpressionsCollection selectState)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _dictionaryParser = dictionaryParser ?? throw new ArgumentNullException(nameof(dictionaryParser));
            _selectCall = selectCall;
            _selectState = selectState;
        }
        public IList<TNew> ParseResults(XElement parentNode)
        {
            if (parentNode == null)
                return null;

            List<TNew> result = new List<TNew>();
            var docs = this._dictionaryParser.ParseResults(parentNode);
            IList<TOld> olds = this._inner.ParseResults(parentNode);

            for (int i = 0; i < olds.Count; i++)
            {
                result.Add(this.GetResult(olds[i], docs[i]));
            }

            return result;
        }

        private TNew GetResult(TOld old, Dictionary<string, object> dictionary)
        {
            ReplaceCalculatedVisitor visitor = new ReplaceCalculatedVisitor(this._selectState, dictionary);

            LambdaExpression lambdaExpression = (LambdaExpression)this._selectCall.Arguments[1].StripQuotes();

            LambdaExpression expression = (LambdaExpression)visitor.Visit(lambdaExpression);

            object result = expression.Compile().DynamicInvoke(old);

            return (TNew) result;
        }
    }
}