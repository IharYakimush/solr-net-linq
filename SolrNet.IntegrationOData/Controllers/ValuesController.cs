using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Community.OData.Linq;
using Community.OData.Linq.AspNetCore;
using Community.OData.Linq.Json;
using Microsoft.AspNetCore.Mvc;
using SolrNet.Impl.FieldSerializers;
using SolrNet.Linq;
using SolrNet.Mapping;

namespace SolrNet.IntegrationOData.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        public ISolrOperations<Product> Solr { get; }

        public ValuesController(ISolrOperations<Product> solr)
        {
            Solr = solr ?? throw new ArgumentNullException(nameof(solr));
        }
        // GET api/values
        [HttpGet]
        public IActionResult Get()
        {
            return this.Ok(new []
            {
                "/api/values/1?$filter=price gt 100&$orderby=Popularity desc&$top=3&$skip=1",
                "/api/values/1?$filter=Popularity ne null",
                "/api/values/1?$filter=Popularity eq null",
                "/api/values/1?$filter=Categories/any(c: c eq 'electronics')",
                "/api/values/2?$select=Id,Price,Categories",
            });
        }

        // GET api/values/5
        [HttpGet("1")]
        public ActionResult<string> Get1(ODataQueryOptions odata)
        {
            IQueryable<Product> query = this.Solr.AsQueryable(options =>
            {
                // Set q parameter. By default *:* will be used
                options.MainQuery = new SolrQuery("*:*");

                // Configure SolrNet QueryOptions.
                // This function will be called after applying query options from LINQ
                // You can setup options not covered by LINQ. For instance facets
                options.SetupQueryOptions = queryOptions =>
                {
                    queryOptions.AddFacets();
                };

                // override default serializer if needed
                options.SolrFieldSerializer = new DefaultFieldSerializer();

                // override default mapping manager if needed.
                options.MappingManager = new AttributesMappingManager();
            });

            return this.Ok(query.OData().ApplyQueryOptionsWithoutSelectExpand(odata).ToSolrQueryResults());
        }

        // GET api/values/5
        [HttpGet("2")]
        public ActionResult<string> Get2(ODataQueryOptions odata)
        {
            IQueryable<Product> query = this.Solr.AsQueryable();

            return this.Ok(query.OData().ApplyQueryOptions(odata).ToJson());
        }
    }
}
