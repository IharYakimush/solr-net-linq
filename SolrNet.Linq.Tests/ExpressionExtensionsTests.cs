using System;
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
    }
}