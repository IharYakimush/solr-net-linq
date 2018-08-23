using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Xml.Linq;
using SolrNet.Impl;
using SolrNet.Linq.Expressions;

namespace SolrNet.Linq.Impl
{
    public class SelectResponseParser<TNew,TOld> : TransformationResponseParser<TNew, TOld>
    {
        private readonly MethodCallExpression _selectCall;
        private readonly SelectExpressionsCollection _selectState;

        public SelectResponseParser(
            ISolrDocumentResponseParser<TOld> inner, 
            ISolrDocumentResponseParser<Dictionary<string, object>> dictionaryParser, 
            MethodCallExpression selectCall, 
            SelectExpressionsCollection selectState):base(inner, dictionaryParser)
        {
            _selectCall = selectCall ?? throw new ArgumentNullException(nameof(selectCall));
            _selectState = selectState ?? throw new ArgumentNullException(nameof(selectState));
        }        

        protected override TNew GetResult(TOld old, Dictionary<string, object> dictionary)
        {
            ReplaceCalculatedVisitor visitor = new ReplaceCalculatedVisitor(this._selectState, dictionary);

            LambdaExpression lambdaExpression = (LambdaExpression)this._selectCall.Arguments[1].StripQuotes();

            LambdaExpression expression = (LambdaExpression)visitor.Visit(lambdaExpression);

            object result = expression.Compile().DynamicInvoke(old);

            return (TNew) result;
        }
    }
}