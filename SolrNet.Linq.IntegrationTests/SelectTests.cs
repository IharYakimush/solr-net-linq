using System;
using System.Linq;
using Xunit;

namespace SolrNet.Linq.IntegrationTests
{
    public class SelectTests
    {
        [Fact]
        public void AnonymousClass()
        {
            var t1 = Product.SolrOperations.Value.AsQueryable().Where(p => p.Id != null)
                .Select(p => new {p.Id, p.Price, p.Categories, Qwe = Math.Pow(2,2) })
                .Where(arg => arg.Categories.Any(s => s == "electronics"))
                .OrderBy(arg => arg.Id)
                .FirstOrDefault();

            Assert.NotNull(t1);
        }
    }
}