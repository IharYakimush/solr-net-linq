# solr-net-linq
SolrNet IQueryable provider

## Code Sample
### Prerequisites
To use LINQ with SolrNet you need to have `ISolrOperations<TEntity>` or even `ISolrBasicReadOnlyOperations<TEntity>` where `TEntity` is a type with properties marked with `SolrField` attribute. For instance 
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
Once you have solr operations interface, call `AsQuerable()` extension method to create `IQueryable<T>`. For instance 
```
var solr = ServiceLocator.Current.GetInstance<ISolrOperations<Product>>();
IQueryable<Product> solrLinq = solr.AsQuerable()
```
It is possible to combine linq and regular SolrNet QueryOptions and main query. For instance if you need to append facets:
```
IQueryable<Product> solrLinq = solr.AsQuerable(options =>
{
    // Configure SolrNet QueryOptions by setting params not available in linq
}, new SolrQuery("some custom query"))
```
Now you can use supported linq methods.
### Getting results
To get result you can
 -  Call `ToSolrQueryResultsAsync()` or `ToSolrQueryResults()` extensions methods to get normal SolrNet query result
 ```
 SolrQueryResults<Product> result = solrLinq.Where(p => p.Popularity.HasValue).Take(10).ToSolrQueryResults();
 ```
 - Trigger result by starting enumeration like any other IQuerable result triggering
 ```
 Product[] result = solrLinq.Where(p => p.Popularity.HasValue).Take(10).ToArray();
 ```
## Supported methods

### Top, Skip
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
## Nuget
https://www.nuget.org/packages/SolrNet.Linq
