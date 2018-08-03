# solr-net-linq
SolrNet IQueryable provider

## Code Sample
### Prerequisites
To use LINQ with SolrNet you need to have `ISolrOperations<TEntity>` or even 'ISolrBasicReadOnlyOperations<TEntity>' where TEntity is a type with properties marked with `SolrField` attribute. For instance 
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


## Nuget
https://www.nuget.org/packages/SolrNet.Linq
