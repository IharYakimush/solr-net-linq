using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SolrNet.Linq.IntegrationTests
{
    public class ResultTests
    {
        [Fact]
        public void AsEnumerable()
        {
            IEnumerable<Product> result = Product.SolrOperations.Value.AsQuerable().AsEnumerable();

            Assert.True(result.Any());
        }

        [Fact]
        public void ToSolrQueryResults()
        {
            SolrQueryResults<Product> result = Product.SolrOperations.Value.AsQuerable().ToSolrQueryResults();

            Assert.True(result.NumFound > 0);
        }

        [Fact]
        public async Task ToSolrQueryResultsAsync()
        {
            SolrQueryResults<Product> result = await Product.SolrOperations.Value.AsQuerable().ToSolrQueryResultsAsync();

            Assert.True(result.NumFound > 0);
        }
    }
}