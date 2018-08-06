using System.Linq;
using Xunit;

namespace SolrNet.Linq.IntegrationTests
{
    public class WhereTests
    {
        [Fact]
        public void ByMember()
        {
            Product t1 = Product.SolrOperations.Value.AsQueryable().Where(p => p.InStock).AsEnumerable()
                .FirstOrDefault();

            Product t2 = Product.SolrOperations.Value.AsQueryable().Where(p => !p.InStock).AsEnumerable()
                .FirstOrDefault();

            Assert.Null(t1);
            Assert.NotNull(t2);
        }
    }
}