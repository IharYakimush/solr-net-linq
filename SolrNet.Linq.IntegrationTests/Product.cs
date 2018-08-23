using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using SolrNet.Attributes;

namespace SolrNet.Linq.IntegrationTests
{
    public interface IProduct
    {
        string Id { get; set; }
        ICollection<string> Categories { get; set; }
    }
    public class Product : IProduct
    {
        [DataMember]
        [SolrUniqueKey("id")]
        public string Id { get; set; }

        [DataMember]
        [SolrField("manu_exact")]
        public string Manufacturer { get; set; }

        [SolrField("cat")]
        public ICollection<string> Categories { get; set; }

        [DataMember]
        [SolrField("price")]
        public decimal Price { get; set; }

        [SolrField("sequence_i")]
        public int Sequence { get; set; }

        [SolrField("popularity")]
        public decimal? Popularity { get; set; }

        public decimal NotMapped { get; set; }

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