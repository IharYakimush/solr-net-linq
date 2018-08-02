using System;
using System.Linq.Expressions;
using SolrNet.Impl.FieldSerializers;
using SolrNet.Impl.QuerySerializers;
using SolrNet.Linq.Expressions;
using Xunit;

namespace SolrNet.Linq.Tests
{
    public class FilterExpressionExtensionsTests
    {
        private readonly DefaultQuerySerializer _serializer = new DefaultQuerySerializer(new DefaultFieldSerializer());
        [Fact]
        public void Member()
        {
            Expression<Func<Product, bool>> exp = (Product p) => p.InStock;
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery();
            
            Assert.Equal("inStock_b:(true)", _serializer.Serialize(query));
        }
    }
}