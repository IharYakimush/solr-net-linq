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

        //[Fact]
        //public void CompareMethod()
        //{
        //    Expression<Func<Product, bool>> exp = (Product p) => p.Id.CompareTo("qwe") > 0;
        //    ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(typeof(Product));

        //    Assert.Equal("inStock_b:(true)", _serializer.Serialize(query));
        //}

        [Fact]
        public void NotEqualValue()
        {
            Expression<Func<Product, bool>> exp = (Product p) => p.Id != "qwe";
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(typeof(Product));

            Assert.Equal("(*:* NOT id:(qwe))", _serializer.Serialize(query));
        }

        [Fact]
        public void NotEqualNull()
        {
            Expression<Func<Product, bool>> exp = (Product p) => p.Id != null;
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(typeof(Product));

            Assert.Equal("id:[* TO *]", _serializer.Serialize(query));
        }

        [Fact]
        public void ConstantTrue()
        {
            Expression<Func<Product, bool>> exp = (Product p) => true;
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(typeof(Product));

            Assert.Equal("*:*", _serializer.Serialize(query));
        }

        [Fact]
        public void VarTrue()
        {
            bool b = true;
            Expression<Func<Product, bool>> exp = (Product p) => b;
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(typeof(Product));

            Assert.Equal("*:*", _serializer.Serialize(query));
        }

        [Fact]
        public void ConstantFalse()
        {
            Expression<Func<Product, bool>> exp = (Product p) => false;
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(typeof(Product));

            Assert.Equal("(*:* NOT *:*)", _serializer.Serialize(query));
        }

        [Fact]
        public void NotConstantFalse()
        {
            Expression<Func<Product, bool>> exp = (Product p) => !false;
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(typeof(Product));

            Assert.Equal("*:*", _serializer.Serialize(query));
        }

        [Fact]
        public void ConditionalTestMemberTrueMemberFalseMember()
        {
            Expression<Func<Product, bool>> exp = (Product p) => p.Popularity != null ? p.InStock : p.InStock;
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(typeof(Product));

            Assert.Equal("((popularity:[* TO *] AND inStock_b:(true)) OR ((*:* NOT popularity:[* TO *]) AND inStock_b:(true)))", _serializer.Serialize(query));
        }

        [Fact]
        public void ConditionalTestMemberTrueMemberFalseConstFalse()
        {
            Expression<Func<Product, bool>> exp = (Product p) => p.Popularity != null ? p.InStock : false;
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(typeof(Product));

            Assert.Equal("(popularity:[* TO *] AND inStock_b:(true))", _serializer.Serialize(query));
        }

        [Fact]
        public void ConditionalTestMemberTrueMemberFalseConstTrue()
        {
            Expression<Func<Product, bool>> exp = (Product p) => p.Popularity != null ? p.InStock : true;
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(typeof(Product));

            Assert.Equal("((popularity:[* TO *] AND inStock_b:(true)) OR (*:* NOT popularity:[* TO *]))", _serializer.Serialize(query));
        }

        [Fact]
        public void ConditionalTestMemberTrueConstFalseFalseMember()
        {
            Expression<Func<Product, bool>> exp = (Product p) => p.Popularity != null ? false : p.InStock;
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(typeof(Product));

            Assert.Equal("((*:* NOT popularity:[* TO *]) AND inStock_b:(true))", _serializer.Serialize(query));
        }

        [Fact]
        public void ConditionalTestMemberTrueConstTrueFalseMember()
        {
            Expression<Func<Product, bool>> exp = (Product p) => p.Popularity != null ? true : p.InStock;
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(typeof(Product));

            Assert.Equal("(popularity:[* TO *] OR ((*:* NOT popularity:[* TO *]) AND inStock_b:(true)))", _serializer.Serialize(query));
        }

        [Fact]
        public void ConditionalLessThanTestMemberTrueMemberFalseMember()
        {
            Expression<Func<Product, bool>> exp = (Product p) => (p.Popularity != null ? p.Popularity.Value : p.Price) > 7;
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(typeof(Product));

            Assert.Equal("((popularity:[* TO *] AND popularity:{7 TO *}) OR ((*:* NOT popularity:[* TO *]) AND price:{7 TO *}))", _serializer.Serialize(query));
        }
    }
}