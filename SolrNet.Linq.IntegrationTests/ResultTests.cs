using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SolrNet.Linq.IntegrationOData
{
    public class ResultTests
    {
        [Fact]
        public void AsEnumerable()
        {
            IEnumerable<Product> result = Product.SolrOperations.Value.AsQueryable().AsEnumerable();

            Assert.True(result.Any());
        }

        [Fact]
        public void ToSolrQueryResults()
        {
            SolrQueryResults<Product> result = Product.SolrOperations.Value.AsQueryable().ToSolrQueryResults();

            Assert.True(result.NumFound > 0);
        }

        [Fact]
        public async Task ToSolrQueryResultsAsync()
        {
            SolrQueryResults<Product> result = await Product.SolrOperations.Value.AsQueryable().ToSolrQueryResultsAsync();

            Assert.True(result.NumFound > 0);
        }
    }
}