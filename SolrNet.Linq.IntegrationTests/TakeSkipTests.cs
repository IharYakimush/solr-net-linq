using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SolrNet.Linq.IntegrationOData
{
    public class TakeSkipTests
    {
        [Fact]
        public void TakeSkip()
        {
            Product t1 = Product.SolrOperations.Value.AsQueryable().Take(1).AsEnumerable().Single();
            Product t2 = Product.SolrOperations.Value.AsQueryable().Skip(1).Take(1).AsEnumerable().Single();
            Product t3 = Product.SolrOperations.Value.AsQueryable().Skip(1).Take(1).AsEnumerable().Single();

            Assert.NotEqual(t1.Id, t2.Id);
            Assert.Equal(t3.Id, t2.Id);
        }
    }
}