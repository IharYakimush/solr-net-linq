using Moq;

using SolrNet.Commands.Parameters;
using SolrNet.Linq.Impl;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Xunit;

namespace SolrNet.Linq.Tests
{
    public class EnumeratedTests
    {
        //[Fact]
        //public async Task OrDefaultAsync()
        //{
            

        //    Product t1 = await q.FirstOrDefaultAsync(p => p.Id == "qwe");
        //    Product t2 = await q.Where(p => p.Id == "qwe").FirstOrDefaultAsync();
        //    Product t3 = await q.SingleOrDefaultAsync(p => p.Id == "qwe");
        //    Product t4 = await q.Where(p => p.Id == "qwe").SingleOrDefaultAsync();

        //    Assert.Null(t1);
        //    Assert.Null(t2);
        //    Assert.Null(t3);
        //    Assert.Null(t4);
        //}

        private static IQueryable<Product> GetQueryWithResult(int count)
        {
            SolrNetLinqOptions options = new SolrNetLinqOptions();

            Mock<ISolrBasicReadOnlyOperations<Product>> mock = new Mock<ISolrBasicReadOnlyOperations<Product>>();

            SolrQueryResults<Product> result = new SolrQueryResults<Product>();

            result.NumFound = count;
            for (int i = 0; i < count; i++)
            {
                result.Add(new Product() { Id = $"id{i}" });
            }

            mock.Setup(x => x.QueryAsync(It.IsAny<ISolrQuery>(), It.IsAny<QueryOptions>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(result));

            mock.Setup(x=>x.Query(It.IsAny<ISolrQuery>(), It.IsAny<QueryOptions>()))
                .Returns(result);

            return new SolrQuery<Product>(new SolrQueryProvider<Product>(
                 new SolrQueryExecuterWrapperBasicOperations<Product>(mock.Object),
                 options, null, null));
        }

        [Fact]
        public void HasResults()
        {
            Product t1 = GetQueryWithResult(1).OrderBy(p => p.Id).First();
            string id = t1.Id;
            Product t2 = GetQueryWithResult(1).OrderBy(p => p.Id).First(p => p.Id != "qwe");
            Product t3 = GetQueryWithResult(1).OrderBy(p => p.Id).FirstOrDefault();
            Product t4 = GetQueryWithResult(1).OrderBy(p => p.Id).FirstOrDefault(p => p.Id != "qwe");
            Product t5 = GetQueryWithResult(1).OrderBy(p => p.Id).Single(p => p.Id == id);
            Product t6 = GetQueryWithResult(1).OrderBy(p => p.Id).SingleOrDefault(p => p.Id == id);

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
            Product t1 = await GetQueryWithResult(1).OrderBy(p => p.Id).FirstAsync();
            string id = t1.Id;
            Product t2 = await GetQueryWithResult(1).OrderBy(p => p.Id).FirstAsync(p => p.Id != "qwe");
            Product t3 = await GetQueryWithResult(1).OrderBy(p => p.Id).FirstOrDefaultAsync();
            Product t4 = await GetQueryWithResult(1).OrderBy(p => p.Id).FirstOrDefaultAsync(p => p.Id != "qwe");
            Product t5 = await GetQueryWithResult(1).OrderBy(p => p.Id).SingleAsync(p => p.Id == id);
            Product t6 = await GetQueryWithResult(1).OrderBy(p => p.Id).SingleOrDefaultAsync(p => p.Id == id);

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
            Product t1 = GetQueryWithResult(0).FirstOrDefault(p => p.Id == "qwe");
            Product t2 = GetQueryWithResult(0).SingleOrDefault(p => p.Id == "qwe");

            Assert.Null(t1);
            Assert.Null(t2);
        }

        [Fact]
        public void Throw()
        {
            Assert.Throws<InvalidOperationException>(() =>
                GetQueryWithResult(0).First(p => p.Id == "qwe"));
            Assert.Throws<InvalidOperationException>(() =>
                GetQueryWithResult(0).Single(p => p.Id == "qwe"));

            Assert.Throws<InvalidOperationException>(() => GetQueryWithResult(0).Single());
        }

        [Fact]
        public async Task ThrowAsync()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                GetQueryWithResult(0).FirstAsync(p => p.Id == "qwe"));

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                GetQueryWithResult(0).SingleAsync(p => p.Id == "qwe"));

            await Assert.ThrowsAsync<InvalidOperationException>(() => GetQueryWithResult(0).SingleAsync());
        }

        [Fact]
        public async Task OrDefaultAsync()
        {
            Product t1 = await GetQueryWithResult(0).FirstOrDefaultAsync(p => p.Id == "qwe");
            Product t2 = await GetQueryWithResult(0).Where(p => p.Id == "qwe").FirstOrDefaultAsync();
            Product t3 = await GetQueryWithResult(0).SingleOrDefaultAsync(p => p.Id == "qwe");
            Product t4 = await GetQueryWithResult(0).Where(p => p.Id == "qwe").SingleOrDefaultAsync();

            Assert.Null(t1);
            Assert.Null(t2);
            Assert.Null(t3);
            Assert.Null(t4);
        }

        [Fact]
        public async Task Any()
        {
            Assert.True(await GetQueryWithResult(1).AnyAsync(p => p.Id != "qwe"));
            Assert.True(await GetQueryWithResult(1).AnyAsync());
            Assert.True(GetQueryWithResult(1).Any());
            Assert.True(GetQueryWithResult(1).Any(p => p.Id != "qwe"));

            Assert.False(await GetQueryWithResult(0).AnyAsync(p => p.Id == "qwe"));
            Assert.False(await GetQueryWithResult(0).Where(p => p.Id == "qwe").AnyAsync());
            Assert.False(GetQueryWithResult(0).Where(p => p.Id == "qwe").Any());
            Assert.False(GetQueryWithResult(0).Any(p => p.Id == "qwe"));
        }

        [Fact]
        public async Task CountLongCount()
        {
            int c1 = await GetQueryWithResult(1).CountAsync(p => p.Id != "qwe");
            long c2 = await GetQueryWithResult(2).LongCountAsync(p => p.Id != "qwe");
            long c3 = GetQueryWithResult(3).LongCount(p => p.Id != "qwe");
            int c4 = GetQueryWithResult(4).Count(p => p.Id != "qwe");

            Assert.Equal(1, c1);
            Assert.Equal(2, c2);
            Assert.Equal(3, c3);
            Assert.Equal(4, c4);

            int c5 = await GetQueryWithResult(5).CountAsync(p => p.Id == "qwe");
            long c6 = await GetQueryWithResult(6).LongCountAsync(p => p.Id == "qwe");
            long c7 = GetQueryWithResult(7).LongCount(p => p.Id == "qwe");
            int c8 = GetQueryWithResult(8).Count(p => p.Id == "qwe");

            Assert.Equal(5, c5);
            Assert.Equal(6, c6);
            Assert.Equal(7, c7);
            Assert.Equal(8, c8);
        }
    }
}
