using System;
using System.Linq.Expressions;
using Xunit;
using SolrNet.Linq.Expressions;
using SolrNet.Linq.Expressions.Context;

namespace SolrNet.Linq.Tests
{
    public class MemberExpressionExtensionsTest
    {
        [Fact]
        public void Member()
        {
            Expression<Func<Product,object>> exp = (Product p) => p.Popularity;
            
            Assert.Equal("popularity", MemberContext.ForType<Product>().GetSolrMemberProduct(exp.Body));            
        }

        [Fact]
        public void Div()
        {
            Expression<Func<Product, object>> exp = (Product p) => p.Popularity / 10;

            Assert.Equal("div(popularity,10)", MemberContext.ForType<Product>().GetSolrMemberProduct(exp.Body));
        }

        [Fact]
        public void DivVariable()
        {
            double qwe = 12;
            Expression<Func<Product, object>> exp = (Product p) => p.Sequence / qwe;

            Assert.Equal("div(sequence_i,12)", MemberContext.ForType<Product>().GetSolrMemberProduct(exp.Body));
        }

        [Fact]
        public void Sub()
        {
            Expression<Func<Product, object>> exp = (Product p) => p.Popularity - 10;

            Assert.Equal("sub(popularity,10)", MemberContext.ForType<Product>().GetSolrMemberProduct(exp.Body));
        }

        [Fact]
        public void Sum()
        {
            Expression<Func<Product, object>> exp = (Product p) => p.Popularity + 10;

            Assert.Equal("sum(popularity,10)", MemberContext.ForType<Product>().GetSolrMemberProduct(exp.Body));
        }

        [Fact]
        public void Mul()
        {
            Expression<Func<Product, object>> exp = (Product p) => p.Popularity * 10;

            Assert.Equal("mul(popularity,10)", MemberContext.ForType<Product>().GetSolrMemberProduct(exp.Body));
        }

        [Fact]
        public void Abs()
        {
            Expression<Func<Product, object>> exp = (Product p) => Math.Abs(p.Sequence);

            Assert.Equal("abs(sequence_i)", MemberContext.ForType<Product>().GetSolrMemberProduct(exp.Body));
        }

        [Fact]
        public void Log()
        {
            Expression<Func<Product, object>> exp = (Product p) => Math.Log10(p.Sequence);

            Assert.Equal("log(sequence_i)", MemberContext.ForType<Product>().GetSolrMemberProduct(exp.Body));
        }

        [Fact]
        public void Max()
        {
            Expression<Func<Product, object>> exp = (Product p) => Math.Max(p.Sequence, 11);

            Assert.Equal("max(sequence_i,11)", MemberContext.ForType<Product>().GetSolrMemberProduct(exp.Body));
        }

        [Fact]
        public void Min()
        {
            Expression<Func<Product, object>> exp = (Product p) => Math.Min(p.Sequence, 11);

            Assert.Equal("min(sequence_i,11)", MemberContext.ForType<Product>().GetSolrMemberProduct(exp.Body));
        }

        [Fact]
        public void Pow()
        {
            Expression<Func<Product, object>> exp = (Product p) => Math.Pow(p.Sequence, 11);

            Assert.Equal("pow(sequence_i,11)", MemberContext.ForType<Product>().GetSolrMemberProduct(exp.Body));
        }

        [Fact]
        public void Sqrt()
        {
            Expression<Func<Product, object>> exp = (Product p) => Math.Sqrt(p.Sequence);

            Assert.Equal("sqrt(sequence_i)", MemberContext.ForType<Product>().GetSolrMemberProduct(exp.Body));
        }
    }
}
