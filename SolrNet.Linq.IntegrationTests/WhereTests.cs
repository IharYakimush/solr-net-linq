using System.Linq;
using Xunit;

namespace SolrNet.Linq.IntegrationOData
{
    public class WhereTests
    {
        [Fact]
        public void ByMember()
        {
            Product t1 = Product.SolrOperations.Value.AsQuerable().Where(p => p.InStock).AsEnumerable()
                .FirstOrDefault();

            Product t2 = Product.SolrOperations.Value.AsQuerable().Where(p => !p.InStock).AsEnumerable()
                .FirstOrDefault();

            Assert.Null(t1);
            Assert.NotNull(t2);
        }
    }
}