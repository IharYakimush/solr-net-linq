# solr-net-linq
SolrNet IQueryable provider. Extend SolrNet functionality by adding limited LINQ to SOLR support.

## Code Sample
### Prerequisites
To use LINQ with SolrNet you need to have `ISolrOperations<TEntity>` or even `ISolrBasicReadOnlyOperations<TEntity>`. By default `TEntity` should be a type with properties marked with `SolrField` attribute, however it is not mandatory if you going to set another mapping manager when initializing a query. For instance 
```
public class Product
{
    [SolrUniqueKey("id")]
    public string Id { get; set; }

    [SolrField("cat")]
    public ICollection<string> Categories { get; set; }

    [SolrField("popularity")]
    public decimal? Popularity { get; set; }
}
```
### IQueryable initialization
Once you have solr operations interface, call `AsQueryable()` extension method to create `IQueryable<T>`. For instance 
```
var solr = ServiceLocator.Current.GetInstance<ISolrOperations<Product>>();
IQueryable<Product> solrLinq = solr.AsQueryable()
```
Now you can use supported linq methods.
### Getting results
To get result you can
 -  Call `ToSolrQueryResultsAsync()` or `ToSolrQueryResults()` extensions methods to get normal SolrNet query result
 ```
 SolrQueryResults<Product> result = solrLinq.Where(p => p.Popularity.HasValue).Take(10).ToSolrQueryResults();
 ```
 - Trigger result by starting enumeration like any other IQueryable result triggering
 ```
 Product[] result = solrLinq.Where(p => p.Popularity.HasValue).Take(10).ToArray();
 ```
## Customization and workarounds for not suppported capabilities
It is possible to combine linq and regular SolrNet QueryOptions and main query. For instance if you need to append facets:
```
IQueryable<Product> solrLinq = solr.AsQueryable(setup =>
{
    // Set q parameter. By default *:* will be used.
    // LINQ Where method append fq (Filter Query) to query options and not affect main query
    options.MainQuery = new SolrQuery("some query");

    // Configure SolrNet QueryOptions.
    // This function will be called after applying query options from LINQ
    // You can setup options not covered by LINQ, for instance facets
    options.SetupQueryOptions = queryOptions =>
    {
        queryOptions.AddFacets();
    };

    // override default serializer if needed
    options.SolrFieldSerializer = new DefaultFieldSerializer();

    // override default mapping manager if needed
    options.MappingManager = new AttributesMappingManager();
});
```

## Supported methods

### OrderBy, OrderByDescending, ThenBy, ThenByDescending
  - Order by field
 ```
 Product[] result = solrLinq.OrderBy(p => p.Price).ThenBy(p => p.Id).ToArray();
 ```
  - Order by functions (not all of SOLR functions currently supported)
 ```
 Product[] result = solrLinq.OrderBy(p => Math.Pow(p.Price,3) + 1).ToArray();
 ```
### Where
  - Simple comparison
  ```
  // result fq  
  // price:{12 TO *}
  Product[] result = solrLinq.Where(p => p.Price > 12).ToArray();
  ```
  - Not equal
  ```
  // result fq  
  // (*:* NOT id:(qwe))
  Product[] result = solrLinq.Where(p => p.Id != "qwe").ToArray();
  ```
  - Conditional expressions
  ```
  // result fq  
  // (popularity:{7 TO *} OR ((*:* NOT popularity:[* TO *]) AND price:{7 TO *}))
  Product[] result = solrLinq.Where(p => (p.Popularity != null ? p.Popularity.Value : p.Price) > 7).ToArray();
  ```
  - Contains() method
  ```
  // result fq  
  // (price:((1) OR (2) OR (3)))
  List<decimal> list = new List<decimal> {1, 2, 3};
  Product[] result = solrLinq.Where(p => list.Contains(p.Price)).ToArray();
  ```
  - Any() method
  ```
  // result fq  
  // cat:(qwe)
  Product[] result = solrLinq.Where(p => p.Categories.Any(s => s == "qwe")).ToArray();
  ```
  ### Select
  - Anonymous and regular types with fields, pseudo field, functions and transformers
  ```
  var result = solrLinq.Select(p => 
      new {
              p.Id, 
	          p.Categories, 
	          Qwe = Math.Pow(2,2), // evaluated locally
              Next = new Product2 {Id = p.Id} // evaluated locally after selecting required info from solr
	          ValStr = SolrExpr.Transformers.Value("qwe"), // value transformer evaluated in solr
	          Score= SolrExpr.Fields.Score() // score pseudo field evaluated in solr
	      }).OrderBy(arg => arg.Score) // allowed to use expressions evaluated in solr for ordering
		  .ToSolrQueryResults();
  ```
  - Combine Select with other methods
  ```
  var result = solrLinq
	.Where(p => p.Id != null)
    .Select(p => new {p.Id, p.Price, p.Categories, Score= SolrExpr.Fields.Score()})
    .Where(arg => arg.Categories.Any(s => s == "electronics"))
    .OrderBy(arg => arg.Id).ThenBy(arg=>arg.Score) 
    .Select(arg => new {arg.Id})
    .FirstOrDefault(arg2 => arg2.Id != null);
  ```
  ### Paging
  - Top
  - Skip
  ### Any
  - Any
  - AnyAsync
  ### Count
  - Count
  - CountAsync
  - LongCount
  - LongCountAsync
  ### First
  - First
  - FirstAsync
  - FirstOrDefault 
  - FirstOrDefaultAsync
  ### Single
  - Single 
  - SingleAsync
  - SingleOrDefault
  - SingleOrDefaultAsync
  ### Cast
  Cast to interface and get SolrQueryResults object of interface type
  ```
  SolrQueryResults<IProduct> result = solrLinq
	.Where(p => p.Id != null)
    .Cast<IProduct>
    .ToSolrQueryResults();
  ```
## Nuget
https://www.nuget.org/packages/SolrNet.Linq
