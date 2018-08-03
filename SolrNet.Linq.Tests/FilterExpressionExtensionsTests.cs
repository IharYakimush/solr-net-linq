using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using SolrNet.Impl.FieldSerializers;
using SolrNet.Impl.QuerySerializers;
using SolrNet.Linq.Expressions;
using SolrNet.Linq.Expressions.Context;
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
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(MemberContext.ForType<Product>());
            
            Assert.Equal("inStock_b:(true)", _serializer.Serialize(query));
        }

        [Fact]
        public void CompareLess()
        {
            Expression<Func<Product, bool>> exp = (Product p) => p.Price < 12;
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(MemberContext.ForType<Product>());

            Assert.Equal("price:{* TO 12}", _serializer.Serialize(query));
        }

        [Fact]
        public void CompareLessOrEqual()
        {
            Expression<Func<Product, bool>> exp = (Product p) => p.Price <= 12;
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(MemberContext.ForType<Product>());

            Assert.Equal("price:[* TO 12]", _serializer.Serialize(query));
        }

        [Fact]
        public void CompareGreater()
        {
            Expression<Func<Product, bool>> exp = (Product p) => p.Price > 12;
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(MemberContext.ForType<Product>());

            Assert.Equal("price:{12 TO *}", _serializer.Serialize(query));
        }

        [Fact]
        public void CompareGreaterConversion()
        {
            Expression<Func<Product, bool>> exp = (Product p) => (int)p.Price > 12;
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(MemberContext.ForType<Product>());

            Assert.Equal("price:{12 TO *}", _serializer.Serialize(query));
        }

        [Fact]
        public void CompareGreaterVar()
        {
            int i = 12;
            Expression<Func<Product, bool>> exp = (Product p) => p.Price > i;
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(MemberContext.ForType<Product>());

            Assert.Equal("price:{12 TO *}", _serializer.Serialize(query));
        }

        [Fact]
        public void CompareGreaterExpression()
        {
            int i = 12;
            Expression<Func<Product, bool>> exp = (Product p) => p.Price > i + 1;
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(MemberContext.ForType<Product>());

            Assert.Equal("price:{13 TO *}", _serializer.Serialize(query));
        }

        [Fact]
        public void CompareGreaterReverse()
        {
            Expression<Func<Product, bool>> exp = (Product p) => 12 > p.Price;
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(MemberContext.ForType<Product>());

            Assert.Equal("price:{* TO 12}", _serializer.Serialize(query));            
        }

        [Fact]
        public void CompareGreaterOrEqual()
        {
            Expression<Func<Product, bool>> exp = (Product p) => p.Price >= 12;
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(MemberContext.ForType<Product>());

            Assert.Equal("price:[12 TO *]", _serializer.Serialize(query));
        }

        //[Fact]
        //public void CompareMethod()
        //{
        //    Expression<Func<Product, bool>> exp = (Product p) => p.Id.CompareTo("qwe") > 0;
        //    ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(MemberContext.ForType<Product>());

        //    Assert.Equal("inStock_b:(true)", _serializer.Serialize(query));
        //}

        [Fact]
        public void NotEqualValue()
        {
            Expression<Func<Product, bool>> exp = (Product p) => p.Id != "qwe";
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(MemberContext.ForType<Product>());

            Assert.Equal("(*:* NOT id:(qwe))", _serializer.Serialize(query));
        }

        [Fact]
        public void NotEqualValueReverse()
        {
            Expression<Func<Product, bool>> exp = (Product p) => "qwe" != p.Id;
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(MemberContext.ForType<Product>());

            Assert.Equal("(*:* NOT id:(qwe))", _serializer.Serialize(query));
        }

        [Fact]
        public void NotEqualNull()
        {
            Expression<Func<Product, bool>> exp = (Product p) => p.Id != null;
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(MemberContext.ForType<Product>());

            Assert.Equal("id:[* TO *]", _serializer.Serialize(query));
        }

        [Fact]
        public void ConstantTrue()
        {
            Expression<Func<Product, bool>> exp = (Product p) => true;
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(MemberContext.ForType<Product>());

            Assert.Equal("*:*", _serializer.Serialize(query));
        }

        [Fact]
        public void VarTrue()
        {
            bool b = true;
            Expression<Func<Product, bool>> exp = (Product p) => b;
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(MemberContext.ForType<Product>());

            Assert.Equal("*:*", _serializer.Serialize(query));
        }

        [Fact]
        public void ConstantFalse()
        {
            Expression<Func<Product, bool>> exp = (Product p) => false;
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(MemberContext.ForType<Product>());

            Assert.Equal("(*:* NOT *:*)", _serializer.Serialize(query));
        }

        [Fact]
        public void NotConstantFalse()
        {
            Expression<Func<Product, bool>> exp = (Product p) => !false;
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(MemberContext.ForType<Product>());

            Assert.Equal("*:*", _serializer.Serialize(query));
        }

        [Fact]
        public void ConditionalTestMemberTrueMemberFalseMember()
        {
            Expression<Func<Product, bool>> exp = (Product p) => p.Popularity != null ? p.InStock : p.InStock;
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(MemberContext.ForType<Product>());

            Assert.Equal("((popularity:[* TO *] AND inStock_b:(true)) OR ((*:* NOT popularity:[* TO *]) AND inStock_b:(true)))", _serializer.Serialize(query));
        }

        [Fact]
        public void ConditionalTestMemberTrueMemberFalseConstFalse()
        {
            Expression<Func<Product, bool>> exp = (Product p) => p.Popularity != null ? p.InStock : false;
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(MemberContext.ForType<Product>());

            Assert.Equal("(popularity:[* TO *] AND inStock_b:(true))", _serializer.Serialize(query));
        }

        [Fact]
        public void ConditionalTestMemberTrueMemberFalseConstTrue()
        {
            Expression<Func<Product, bool>> exp = (Product p) => p.Popularity != null ? p.InStock : true;
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(MemberContext.ForType<Product>());

            Assert.Equal("((popularity:[* TO *] AND inStock_b:(true)) OR (*:* NOT popularity:[* TO *]))", _serializer.Serialize(query));
        }

        [Fact]
        public void ConditionalTestMemberTrueConstFalseFalseMember()
        {
            Expression<Func<Product, bool>> exp = (Product p) => p.Popularity != null ? false : p.InStock;
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(MemberContext.ForType<Product>());

            Assert.Equal("((*:* NOT popularity:[* TO *]) AND inStock_b:(true))", _serializer.Serialize(query));
        }

        [Fact]
        public void ConditionalTestMemberTrueConstTrueFalseMember()
        {
            Expression<Func<Product, bool>> exp = (Product p) => p.Popularity != null ? true : p.InStock;
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(MemberContext.ForType<Product>());

            Assert.Equal("(popularity:[* TO *] OR ((*:* NOT popularity:[* TO *]) AND inStock_b:(true)))", _serializer.Serialize(query));
        }

        [Fact]
        public void ConditionalLessThanTestMemberTrueMemberFalseMember()
        {
            Expression<Func<Product, bool>> exp = (Product p) => (p.Popularity != null ? p.Popularity.Value : p.Price) > 7;
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(MemberContext.ForType<Product>());

            Assert.Equal("(popularity:{7 TO *} OR ((*:* NOT popularity:[* TO *]) AND price:{7 TO *}))", _serializer.Serialize(query));
        }

        [Fact]
        public void NullableHasValue()
        {
            Expression<Func<Product, bool>> exp = (Product p) => p.Popularity.HasValue;
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(MemberContext.ForType<Product>());

            Assert.Equal("popularity:[* TO *]", _serializer.Serialize(query));
        }

        [Fact]
        public void NullableNotHasValue()
        {
            Expression<Func<Product, bool>> exp = (Product p) => !p.Popularity.HasValue;
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(MemberContext.ForType<Product>());

            Assert.Equal("(*:* NOT popularity:[* TO *])", _serializer.Serialize(query));
        }

        [Fact]
        public void MemberEqualConstTrue()
        {
            Expression<Func<Product, bool>> exp = (Product p) => p.InStock == true;
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(MemberContext.ForType<Product>());

            Assert.Equal("inStock_b:(true)", _serializer.Serialize(query));
        }

        [Fact]
        public void MemberEqualVarTrue()
        {
            bool b = true;
            Expression<Func<Product, bool>> exp = (Product p) => p.InStock == b;
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(MemberContext.ForType<Product>());

            Assert.Equal("inStock_b:(true)", _serializer.Serialize(query));
        }

        [Fact]
        public void MemberEqualNull()
        {
            bool b = true;
            Expression<Func<Product, bool>> exp = (Product p) => p.Popularity == null;
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(MemberContext.ForType<Product>());

            Assert.Equal("(*:* NOT popularity:[* TO *])", _serializer.Serialize(query));
        }

        [Fact]
        public void MemberEqualVarNullableTrue()
        {
            bool? b = true;
            Expression<Func<Product, bool>> exp = (Product p) => p.InStock == b;
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(MemberContext.ForType<Product>());

            Assert.Equal("inStock_b:(true)", _serializer.Serialize(query));
        }

        [Fact]
        public void MemberEqualConstFalse()
        {
            Expression<Func<Product, bool>> exp = (Product p) => p.InStock == false;
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(MemberContext.ForType<Product>());

            Assert.Equal("inStock_b:(false)", _serializer.Serialize(query));
        }

        [Fact]
        public void MemberEqualConstValue()
        {
            Expression<Func<Product, bool>> exp = (Product p) => p.Price == 2;
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(MemberContext.ForType<Product>());

            Assert.Equal("price:(2)", _serializer.Serialize(query));
        }

        [Fact]
        public void MemberEqualConstValueReverse()
        {
            Expression<Func<Product, bool>> exp = (Product p) => 2 == p.Price;
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(MemberContext.ForType<Product>());

            Assert.Equal("price:(2)", _serializer.Serialize(query));
        }

        [Fact]
        public void MemberEqualVarValue()
        {
            int i = 2;
            Expression<Func<Product, bool>> exp = (Product p) => p.Price == i;
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(MemberContext.ForType<Product>());

            Assert.Equal("price:(2)", _serializer.Serialize(query));
        }

        [Fact]
        public void ConstEqualVarTrue()
        {
            int i = 2;
            Expression<Func<Product, bool>> exp = (Product p) => 2 == i;
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(MemberContext.ForType<Product>());

            Assert.Equal("*:*", _serializer.Serialize(query));
        }

        [Fact]
        public void ConstNotEqualVarTrue()
        {
            int i = 2;
            Expression<Func<Product, bool>> exp = (Product p) => 3 != i;
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(MemberContext.ForType<Product>());

            Assert.Equal("*:*", _serializer.Serialize(query));
        }

        [Fact]
        public void ConstEqualVarFalse()
        {
            int i = 2;
            Expression<Func<Product, bool>> exp = (Product p) => 3 == i;
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(MemberContext.ForType<Product>());

            Assert.Equal("(*:* NOT *:*)", _serializer.Serialize(query));
        }

        [Fact]
        public void ContainsArray()
        {
            decimal[] a = {1, 2, 3};
            
            Expression<Func<Product, bool>> exp = (Product p) =>a.Contains(p.Price) ;
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(MemberContext.ForType<Product>());

            Assert.Equal("(price:((1) OR (2) OR (3)))", _serializer.Serialize(query));
        }

        [Fact]
        public void ContainsList()
        {
            List<decimal> a = new List<decimal> {1, 2, 3};

            Expression<Func<Product, bool>> exp = (Product p) => a.Contains(p.Price);
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(MemberContext.ForType<Product>());

            Assert.Equal("(price:((1) OR (2) OR (3)))", _serializer.Serialize(query));
        }

        [Fact]
        public void ContainsIEnumerable()
        {
            IEnumerable<decimal> a = new List<decimal> { 1, 2, 3 };

            Expression<Func<Product, bool>> exp = (Product p) => a.Contains(p.Price);
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(MemberContext.ForType<Product>());

            Assert.Equal("(price:((1) OR (2) OR (3)))", _serializer.Serialize(query));
        }
        
        [Fact]
        public void CollectionAnyEqual()
        {
            int i = 2;
            Expression<Func<Product, bool>> exp = (Product p) => p.Categories.Any(s => s == "qwe");
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(MemberContext.ForType<Product>());

            Assert.Equal("cat:(qwe)", _serializer.Serialize(query));
        }

        [Fact]
        public void CollectionAnyEqualReverse()
        {
            int i = 2;
            Expression<Func<Product, bool>> exp = (Product p) => p.Categories.Any(s => "qwe" == s);
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(MemberContext.ForType<Product>());

            Assert.Equal("cat:(qwe)", _serializer.Serialize(query));
        }

        [Fact]
        public void CollectionAnyContains()
        {
            string[] arr = {"q1", "q2"};
            Expression<Func<Product, bool>> exp = (Product p) => p.Categories.Any(s => arr.Contains(s));
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(MemberContext.ForType<Product>());

            Assert.Equal("(cat:((q1) OR (q2)))", _serializer.Serialize(query));
        }

        [Fact]
        public void CollectionAnyNotEqual()
        {
            int i = 2;
            Expression<Func<Product, bool>> exp = (Product p) => p.Categories.Any(s => s != "qwe");
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(MemberContext.ForType<Product>());

            Assert.Equal("(*:* NOT cat:(qwe))", _serializer.Serialize(query));
        }

        [Fact]
        public void CollectionAnyWithoutPredicate()
        {
            int i = 2;
            Expression<Func<Product, bool>> exp = (Product p) => p.Categories.Any();
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(MemberContext.ForType<Product>());

            Assert.Equal("cat:[* TO *]", _serializer.Serialize(query));
        }

        [Fact]
        public void CollectionContains()
        {
            int i = 2;
            Expression<Func<Product, bool>> exp = (Product p) => p.Categories.Contains("qwe");
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(MemberContext.ForType<Product>());

            Assert.Equal("cat:(qwe)", _serializer.Serialize(query));
        }

        [Fact]
        public void CollectionContainsVar()
        {
            string i = "qwe";
            Expression<Func<Product, bool>> exp = (Product p) => p.Categories.Contains(i);
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(MemberContext.ForType<Product>());

            Assert.Equal("cat:(qwe)", _serializer.Serialize(query));
        }

        [Theory]
        [InlineData("qwe", "cat:(qwe)")]
        public void CollectionContainsParam(string item, string expected)
        {
            Expression<Func<Product, bool>> exp = (Product p) => p.Categories.Contains(item);
            ISolrQuery query = ((LambdaExpression)exp).Body.GetSolrFilterQuery(MemberContext.ForType<Product>());

            Assert.Equal(expected, _serializer.Serialize(query));
        }
    }
}