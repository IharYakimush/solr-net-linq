using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SolrNet.Linq.IntegrationTests
{
    public class InitTests
    {        
        [Fact]
        public void Test1()
        {
            SolrQueryResults<Product> result = Product.SolrOperations.Value.Query("*:*");

            Assert.True(result.Count > 0);
        }        
    }
}
