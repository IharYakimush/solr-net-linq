using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SolrNet.Linq.IntegrationTests
{
    public class EnumeratedTests
    {
        [Fact]
        public void HasResults()
        {
            Product t1 = Product.SolrOperations.Value.AsQueryable().OrderBy(p => p.Id).First();
            string id = t1.Id;
            Product t2 = Product.SolrOperations.Value.AsQueryable().OrderBy(p => p.Id).First(p => p.Id != "qwe");
            Product t3 = Product.SolrOperations.Value.AsQueryable().OrderBy(p => p.Id).FirstOrDefault();
            Product t4 = Product.SolrOperations.Value.AsQueryable().OrderBy(p => p.Id).FirstOrDefault(p => p.Id != "qwe");
            Product t5 = Product.SolrOperations.Value.AsQueryable().OrderBy(p => p.Id).Single(p => p.Id == id);
            Product t6 = Product.SolrOperations.Value.AsQueryable().OrderBy(p => p.Id).SingleOrDefault(p => p.Id == id);

            Assert.NotNull(t1);
            Assert.NotNull(t2);
            Assert.NotNull(t3);
            Assert.NotNull(t4);
            Assert.NotNull(t5);
            Assert.NotNull(t6);

            Assert.Equal(id, t2.Id);
            Assert.Equal(id, t3.Id);
            Assert.Equal(id, t4.Id);
            Assert.Equal(id, t5.Id);
            Assert.Equal(id, t6.Id);
        }

        [Fact]
        public async Task HasResultsAsync()
        {
            Product t1 = await Product.SolrOperations.Value.AsQueryable().OrderBy(p => p.Id).FirstAsync();
            string id = t1.Id;
            Product t2 = await Product.SolrOperations.Value.AsQueryable().OrderBy(p => p.Id).FirstAsync(p => p.Id != "qwe");
            Product t3 = await Product.SolrOperations.Value.AsQueryable().OrderBy(p => p.Id).FirstOrDefaultAsync();
            Product t4 = await Product.SolrOperations.Value.AsQueryable().OrderBy(p => p.Id).FirstOrDefaultAsync(p => p.Id != "qwe");
            Product t5 = await Product.SolrOperations.Value.AsQueryable().OrderBy(p => p.Id).SingleAsync(p => p.Id == id);
            Product t6 = await Product.SolrOperations.Value.AsQueryable().OrderBy(p => p.Id).SingleOrDefaultAsync(p => p.Id == id);

            Assert.NotNull(t1);
            Assert.NotNull(t2);
            Assert.NotNull(t3);
            Assert.NotNull(t4);
            Assert.NotNull(t5);
            Assert.NotNull(t6);

            Assert.Equal(id, t2.Id);
            Assert.Equal(id, t3.Id);
            Assert.Equal(id, t4.Id);
            Assert.Equal(id, t5.Id);
            Assert.Equal(id, t6.Id);
        }

        [Fact]
        public void OrDefault()
        {
            Product t1 = Product.SolrOperations.Value.AsQueryable().FirstOrDefault(p => p.Id == "qwe");
            Product t2 = Product.SolrOperations.Value.AsQueryable().SingleOrDefault(p => p.Id == "qwe");

            Assert.Null(t1);
            Assert.Null(t2);
        }

        [Fact]
        public void Throw()
        {
            Assert.Throws<InvalidOperationException>(() =>
                Product.SolrOperations.Value.AsQueryable().First(p => p.Id == "qwe"));
            Assert.Throws<InvalidOperationException>(() =>
                Product.SolrOperations.Value.AsQueryable().Single(p => p.Id == "qwe"));
            Assert.Throws<InvalidOperationException>(() => Product.SolrOperations.Value.AsQueryable().Single());           
        }

        [Fact]
        public async Task ThrowAsync()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                Product.SolrOperations.Value.AsQueryable().FirstAsync(p => p.Id == "qwe"));

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                Product.SolrOperations.Value.AsQueryable().SingleAsync(p => p.Id == "qwe"));
            await Assert.ThrowsAsync<InvalidOperationException>(() => Product.SolrOperations.Value.AsQueryable().SingleAsync());
        }

        [Fact]
        public async Task OrDefaultAsync()
        {
            Product t1 = await Product.SolrOperations.Value.AsQueryable().FirstOrDefaultAsync(p => p.Id == "qwe");
            Product t2 = await Product.SolrOperations.Value.AsQueryable().Where(p => p.Id == "qwe").FirstOrDefaultAsync();
            Product t3 = await Product.SolrOperations.Value.AsQueryable().SingleOrDefaultAsync(p => p.Id == "qwe");
            Product t4 = await Product.SolrOperations.Value.AsQueryable().Where(p => p.Id == "qwe").SingleOrDefaultAsync();

            Assert.Null(t1);
            Assert.Null(t2);
            Assert.Null(t3);
            Assert.Null(t4);
        }
    }
}