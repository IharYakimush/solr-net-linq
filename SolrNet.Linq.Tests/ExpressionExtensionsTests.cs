using System;
using System.Linq;
using System.Linq.Expressions;
using SolrNet.Linq.Expressions;
using Xunit;

namespace SolrNet.Linq.Tests
{
    public class ExpressionExtensionsTests
    {
        [Fact]
        public void HasMemberNullableValue()
        {
            Expression<Func<Product, bool>> exp = (Product p) => p.Popularity.Value > 7;
            bool result = exp.Body.HasMemberAccess(typeof(Product));

            Assert.True(result);
        }

        [Fact]
        public void HasMemberNullableHasValue()
        {
            Expression<Func<Product, bool>> exp = (Product p) => p.Popularity.HasValue;
            bool result = exp.Body.HasMemberAccess(typeof(Product));

            Assert.True(result);
        }

        [Fact]
        public void HasMemberAny()
        {
            Expression<Func<Product, bool>> exp = (Product p) => p.Categories.Any();
            bool result = exp.Body.HasMemberAccess(typeof(Product));

            Assert.True(result);
        }

        [Fact]
        public void HasMemberContains()
        {
            Expression<Func<Product, bool>> exp = (Product p) => p.Categories.Contains("qwe");
            bool result = exp.Body.HasMemberAccess(typeof(Product));

            Assert.True(result);
        }
    }
}