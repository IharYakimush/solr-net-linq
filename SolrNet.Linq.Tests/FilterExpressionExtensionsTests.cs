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
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(typeof(Product));
            
            Assert.Equal("inStock_b:(true)", _serializer.Serialize(query));
        }

        [Fact]
        public void CompareLess()
        {
            Expression<Func<Product, bool>> exp = (Product p) => p.Price < 12;
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(typeof(Product));

            Assert.Equal("price:{* TO 12}", _serializer.Serialize(query));
        }

        [Fact]
        public void CompareLessOrEqual()
        {
            Expression<Func<Product, bool>> exp = (Product p) => p.Price <= 12;
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(typeof(Product));

            Assert.Equal("price:[* TO 12]", _serializer.Serialize(query));
        }

        [Fact]
        public void CompareGreater()
        {
            Expression<Func<Product, bool>> exp = (Product p) => p.Price > 12;
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(typeof(Product));

            Assert.Equal("price:{12 TO *}", _serializer.Serialize(query));
        }

        [Fact]
        public void CompareGreaterConversion()
        {
            Expression<Func<Product, bool>> exp = (Product p) => (int)p.Price > 12;
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(typeof(Product));

            Assert.Equal("price:{12 TO *}", _serializer.Serialize(query));
        }

        [Fact]
        public void CompareGreaterVar()
        {
            int i = 12;
            Expression<Func<Product, bool>> exp = (Product p) => p.Price > i;
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(typeof(Product));

            Assert.Equal("price:{12 TO *}", _serializer.Serialize(query));
        }

        [Fact]
        public void CompareGreaterExpression()
        {
            int i = 12;
            Expression<Func<Product, bool>> exp = (Product p) => p.Price > i + 1;
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(typeof(Product));

            Assert.Equal("price:{13 TO *}", _serializer.Serialize(query));
        }

        [Fact]
        public void CompareGreaterReverse()
        {
            Expression<Func<Product, bool>> exp = (Product p) => 12 > p.Price;
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(typeof(Product));

            Assert.Equal("price:{* TO 12}", _serializer.Serialize(query));            
        }

        [Fact]
        public void CompareGreaterOrEqual()
        {
            Expression<Func<Product, bool>> exp = (Product p) => p.Price >= 12;
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(typeof(Product));

            Assert.Equal("price:[12 TO *]", _serializer.Serialize(query));
        }

        [Fact]
        public void CompareMethod()
        {
            Expression<Func<Product, bool>> exp = (Product p) => p.Id.CompareTo("qwe") > 0;
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(typeof(Product));

            Assert.Equal("inStock_b:(true)", _serializer.Serialize(query));
        }
    }
}