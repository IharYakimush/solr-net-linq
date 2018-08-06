using System;
using System.Linq;
using Xunit;

namespace SolrNet.Linq.IntegrationOData
{
    public class SortingTests
    {
        [Fact]
        public void ByProperty()
        {
            var asc = Product.SolrOperations.Value.AsQueryable().OrderBy(p => p.Price).ThenBy(p => p.Id).ToList();
            var desc = Product.SolrOperations.Value.AsQueryable().OrderByDescending(p => p.Price).ThenByDescending(p => p.Id).ToList();

            Assert.Equal(asc.First().Id, desc.Last().Id);
            Assert.Equal(desc.First().Id, asc.Last().Id);
        }

        [Fact]
        public void DoubleOrders()
        {
            Assert.Throws<InvalidOperationException>(() =>
                Product.SolrOperations.Value.AsQueryable().OrderBy(p => p.Price).OrderBy(p => p.Id).ToList());
        }

        [Fact]
        public void ByNotMapped()
        {
            Assert.Throws<InvalidOperationException>(() =>
                Product.SolrOperations.Value.AsQueryable().OrderBy(p => p.NotMapped).ToList());
        }

        [Fact]
        public void ByConversion()
        {
            var result = Product.SolrOperations.Value.AsQueryable().OrderBy(p => (int)p.Price).ToList();
            
            Assert.True(result.Any());
        }

        [Fact]
        public void ByNullable()
        {
            var result = Product.SolrOperations.Value.AsQueryable().OrderBy(p => p.Popularity).ToList();

            Assert.True(result.Any());
        }

        [Fact]
        public void ByDiv()
        {
            var result = Product.SolrOperations.Value.AsQueryable().OrderBy(p => p.Sequence / 10).ToList();

            Assert.True(result.Any());
        }

        [Fact]
        public void ByAbs()
        {
            var result = Product.SolrOperations.Value.AsQueryable().OrderBy(p => Math.Abs(p.Sequence)).ToList();

            Assert.True(result.Any());
        }
    }
}