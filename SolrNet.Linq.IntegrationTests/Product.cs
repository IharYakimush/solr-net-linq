using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using SolrNet.Attributes;

namespace SolrNet.Linq.IntegrationTests
{
    public class Product
    {
        [SolrUniqueKey("id")]
        public string Id { get; set; }

        [SolrField("manu_exact")]
        public string Manufacturer { get; set; }

        [SolrField("cat")]
        public ICollection<string> Categories { get; set; }

        [SolrField("price")]
        public decimal Price { get; set; }

        [SolrField("inStock_b")]
        public bool InStock { get; set; }

        public static Lazy<ISolrOperations<Product>> SolrOperations = new Lazy<ISolrOperations<Product>>(() =>
        {
            IServiceCollection services = new ServiceCollection();
            services.AddSolrNet("http://localhost:8983/solr/demo");

            return services.BuildServiceProvider().GetRequiredService<ISolrOperations<Product>>();
        }, LazyThreadSafetyMode.ExecutionAndPublication);
    }
}