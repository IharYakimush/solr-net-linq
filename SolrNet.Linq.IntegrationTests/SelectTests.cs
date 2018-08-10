using System;
using System.Collections.Generic;
using System.Linq;
using SolrNet.Exceptions;
using Xunit;

namespace SolrNet.Linq.IntegrationTests
{
    public class Product2
    {
        public string Id { get; set; }

        public ICollection<string> Categories { get; set; }

        public decimal Price { get; set; }

        public double Score { get; set; }
    }
    public class SelectTests
    {
        [Fact]
        public void AnonymousClass()
        {
            var t1 = Product.SolrOperations.Value.AsQueryable(lo => lo.SetupQueryOptions = qo =>
                    {
                        Assert.Equal("id", qo.Fields.ElementAt(0));
                        Assert.Equal("price", qo.Fields.ElementAt(1));
                        Assert.Equal("cat", qo.Fields.ElementAt(2));
                    }).Where(p => p.Id != null)
                .Select(p => new {p.Id, p.Price, p.Categories, Qwe = Math.Pow(2,2) })
                .Where(arg => arg.Categories.Any(s => s == "electronics"))
                .OrderBy(arg => arg.Id)
                .FirstOrDefault();

            Assert.NotNull(t1);
            Assert.NotNull(t1.Id);
            Assert.Equal(4, t1.Qwe);
            Assert.True(t1.Categories.Count > 0);
            Assert.True(t1.Price > 0);
        }

        [Fact]
        public void AnonymousMemberWithConversion()
        {
            var t1 = Product.SolrOperations.Value.AsQueryable(lo => lo.SetupQueryOptions = qo =>
                {
                    Assert.Equal("id", qo.Fields.ElementAt(0));
                    Assert.Equal("price", qo.Fields.ElementAt(1));
                    Assert.Equal("cat", qo.Fields.ElementAt(2));
                }).Where(p => p.Id != null)
                .Select(p => new { p.Id, Price = (int)p.Price, p.Categories, Qwe = Math.Pow(2, 2) })
                .Where(arg => arg.Categories.Any(s => s == "electronics"))
                .OrderBy(arg => arg.Id)
                .FirstOrDefault();

            Assert.NotNull(t1);
            Assert.NotNull(t1.Id);
            Assert.Equal(4, t1.Qwe);
            Assert.True(t1.Categories.Count > 0);
            Assert.True(t1.Price > 0);
        }

        [Fact]
        public void AnonymousWithConstAndNext()
        {
            var t1 = Product.SolrOperations.Value.AsQueryable(lo => lo.SetupQueryOptions = qo =>
                {
                    Assert.Equal("id", qo.Fields.ElementAt(0));
                    Assert.Equal("price", qo.Fields.ElementAt(1));
                    Assert.Equal("cat", qo.Fields.ElementAt(2));
                }).Where(p => p.Id != null)
                .Select(p => new {p.Id, p.Price, p.Categories, Qwe = "qwe", Next = new {p.Id}})
                .Where(arg => arg.Categories.Any(s => s == "electronics"))
                .OrderBy(arg => arg.Id)
                .FirstOrDefault();

            Assert.NotNull(t1);
            Assert.NotNull(t1.Id);
            Assert.NotNull(t1.Next);
            Assert.Equal(t1.Next.Id, t1.Id);
            Assert.Equal("qwe", t1.Qwe);
            Assert.True(t1.Categories.Count > 0);
            Assert.True(t1.Price > 0);
        }

        [Fact]
        public void AnonymousIdAndScore()
        {
            var t1 = Product.SolrOperations.Value.AsQueryable(lo => lo.SetupQueryOptions = qo =>
                {
                    Assert.Equal("id", qo.Fields.ElementAt(0));
                    Assert.Equal("v1731e0:score", qo.Fields.ElementAt(1));
                })
                .Select(p => new { p.Id, Score= SolrExpr.Fields.Score()})                
                .OrderBy(arg => arg.Score)
                .FirstOrDefault();

            Assert.NotNull(t1);
            Assert.NotNull(t1.Id);
            Assert.Equal(1, t1.Score);
        }

        [Fact]
        public void AnonymousOrderByScore()
        {
            var t1 = Product.SolrOperations.Value.AsQueryable(lo => lo.SetupQueryOptions = qo =>
                {
                    Assert.Equal("id", qo.Fields.ElementAt(0));
                    Assert.Equal("price", qo.Fields.ElementAt(1));
                    Assert.Equal("cat", qo.Fields.ElementAt(2));
                    Assert.Equal("v1731e0:score", qo.Fields.ElementAt(3));
                }).Where(p => p.Id != null)
                .Select(p => new { p.Id, p.Price, p.Categories, Score = SolrExpr.Fields.Score() })
                .Where(arg => arg.Categories.Any(s => s == "electronics"))
                .OrderBy(arg => arg.Id).ThenBy(arg=>arg.Score)
                .FirstOrDefault();

            Assert.NotNull(t1);
            Assert.NotNull(t1.Id);
            Assert.Equal(1, t1.Score);
            Assert.True(t1.Categories.Count > 0);
            Assert.True(t1.Price > 0);
        }

        [Fact]
        public void AnonymousClassSolrResult()
        {
            var t1 = Product.SolrOperations.Value.AsQueryable().Where(p => p.Id != null)
                .Select(p => new {p.Id, p.Price, p.Categories, Qwe = Math.Pow(2, 2)})
                .Where(arg => arg.Categories.Any(s => s == "electronics"))
                .OrderBy(arg => arg.Id)
                .ToSolrQueryResults();

            Assert.NotNull(t1);
            Assert.NotNull(t1[0].Id);
            Assert.Equal(4, t1[0].Qwe);
            Assert.True(t1.Count > 0);
            Assert.True(t1.NumFound > 0);
        }

        [Fact]
        public void MultipleSelects()
        {
            var t1 = Product.SolrOperations.Value.AsQueryable(lo => lo.SetupQueryOptions = qo =>
                {
                    Assert.Equal(1, qo.Fields.Count);
                    Assert.Equal(3, qo.FilterQueries.Count);
                    Assert.Equal("id", qo.Fields.ElementAt(0));
                }).Where(p => p.Id != null)
                .Select(p => new {p.Id, p.Price, p.Categories})
                .Where(arg => arg.Categories.Any(s => s == "electronics"))
                .OrderBy(arg => arg.Id)
                .Select(arg => new {arg.Id})
                .FirstOrDefault(arg2 => arg2.Id != null);

            Assert.NotNull(t1);
            Assert.NotNull(t1.Id);
        }

        [Fact]
        public void Transformers()
        {
            var dateTime = new DateTime(2011, 1, 2, 3, 4, 5, DateTimeKind.Utc);
            var t1 = Product.SolrOperations.Value.AsQueryable()
                .Select(arg => new
                {
                    ValStr = SolrExpr.Transformers.Value("qwe"),
                    ValInt = SolrExpr.Transformers.Value((int) 1),
                    ValFloat = SolrExpr.Transformers.Value((float) 2),
                    ValDouble = SolrExpr.Transformers.Value((double) 3),
                    ValDate = SolrExpr.Transformers.Value(dateTime),
                    ExplText = SolrExpr.Transformers.ExplainText(),
                    ExplHtml = SolrExpr.Transformers.ExplainHtml(),
                    DocId = SolrExpr.Transformers.DocId()
                }).Skip(1)
                .First();

            Assert.Equal("qwe", t1.ValStr);
            Assert.Equal(1, t1.ValInt);
            Assert.Equal(2f, t1.ValFloat);
            Assert.Equal(3d, t1.ValDouble);
            Assert.Equal(dateTime, t1.ValDate);

            Assert.NotNull(t1.ExplText);
            
            Assert.NotNull(t1.ExplHtml);

            Assert.Equal(1, t1.DocId);
        }

        [Fact]
        public void Product2()
        {
            var t1 = Product.SolrOperations.Value.AsQueryable(lo => lo.SetupQueryOptions = qo =>
                {
                    Assert.Equal(4, qo.Fields.Count);
                    Assert.Equal("id", qo.Fields.ElementAt(0));
                    Assert.Equal("price", qo.Fields.ElementAt(1));
                    Assert.Equal("cat", qo.Fields.ElementAt(2));
                    Assert.Equal("v1731e0:score", qo.Fields.ElementAt(3));

                    Assert.Equal(2, qo.OrderBy.Count);
                    Assert.Equal("id", qo.OrderBy.ElementAt(0).FieldName);
                    Assert.Equal("score", qo.OrderBy.ElementAt(1).FieldName);

                    Assert.Equal(2, qo.FilterQueries.Count);

                }).Where(p => p.Id != null)
                .Select(p => new Product2 {Id = p.Id, Price = p.Price, Categories = p.Categories, Score = SolrExpr.Fields.Score()})
                .Where(arg => arg.Categories.Any(s => s == "electronics"))
                .OrderBy(arg => arg.Id).ThenBy(arg => arg.Score)
                .FirstOrDefault();

            Assert.NotNull(t1);
            Assert.NotNull(t1.Id);

            var t2 = Product.SolrOperations.Value.AsQueryable().Where(p => p.Id != null)                
                .Where(arg => arg.Categories.Any(s => s == "electronics"))
                .OrderBy(arg => arg.Id)
                .FirstOrDefault();

            Assert.NotNull(t2); 
            Assert.Equal(t2.Id, t1.Id);

            SolrQueryResults<Product2> t3 = Product.SolrOperations.Value.AsQueryable().Where(p => p.Id != null)
                .Select(p => new Product2 {Id = p.Id, Price = p.Price, Categories = p.Categories})
                .Where(arg => arg.Categories.Any(s => s == "electronics"))
                .OrderBy(arg => arg.Id).Take(1).ToSolrQueryResults();

            Assert.Single(t3);
            Assert.Equal(t1.Id, t3.Single().Id);
        }

        [Fact]
        public void Product2WithMemberProduct()
        {
            Assert.Throws<SolrConnectionException>(() => Product.SolrOperations.Value.AsQueryable(lo => lo.SetupQueryOptions = qo =>
                {
                    Assert.Equal(3, qo.Fields.Count);
                    Assert.Equal("id", qo.Fields.ElementAt(0));
                    Assert.Equal("price", qo.Fields.ElementAt(1));
                    Assert.Equal("v1731e0:score", qo.Fields.ElementAt(2));

                    Assert.Equal(3, qo.OrderBy.Count);
                    Assert.Equal("id", qo.OrderBy.ElementAt(0).FieldName);
                    Assert.Equal("sum(price,1)", qo.OrderBy.ElementAt(1).FieldName);
                    Assert.Equal("sum(score,1)", qo.OrderBy.ElementAt(2).FieldName);
                }).Where(p => p.Id != null)
                .Select(p =>
                    new Product2 {Id = p.Id, Price = p.Price + 1, Score = SolrExpr.Fields.Score() + 1})
                .OrderBy(arg => arg.Id).ThenBy(arg => arg.Price).ThenBy(arg => arg.Score)
                .FirstOrDefault());
        }
    }
}