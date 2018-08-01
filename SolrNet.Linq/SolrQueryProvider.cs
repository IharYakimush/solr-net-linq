using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using SolrNet.Commands.Parameters;

namespace SolrNet.Linq
{
    public class SolrQueryProvider<TEntity> : IQueryProvider
    {
        public ISolrBasicReadOnlyOperations<TEntity> Operations { get; }

        public QueryOptions QueryOptions { get; }

        public ISolrQuery SolrQuery { get; set; }

        public SolrQueryProvider(ISolrBasicReadOnlyOperations<TEntity> operations, QueryOptions queryOptions = null, ISolrQuery solrQuery = null)
        {
            Operations = operations ?? throw new ArgumentNullException(nameof(operations));
            QueryOptions = queryOptions ?? new QueryOptions();
            SolrQuery = solrQuery ?? global::SolrNet.SolrQuery.All;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            Type elementType = TypeSystem.GetElementType(expression.Type);
            try
            {
                return
                    (IQueryable)
                    Activator.CreateInstance(
                        typeof(SolrQuery<>).MakeGenericType(elementType), new object[] { this, expression });
            }
            catch (TargetInvocationException tie)
            {
                throw tie.InnerException ?? tie;
            }
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            if (typeof(TEntity) == typeof(TElement))
            {
                return new SolrQuery<TElement>(this as SolrQueryProvider<TElement>, expression);
            }
            
            throw new InvalidOperationException();
        }

        public object Execute(Expression expression)
        {
            SolrQueryTranslator translator = new SolrQueryTranslator(this.SolrQuery, this.QueryOptions);
            var result = translator.Translate(this, expression);
            return Operations.Query(result.Item1, result.Item2);
        }

        public Task<SolrQueryResults<TEntity>> ExecuteAsync(Expression expression)
        {
            SolrQueryTranslator translator = new SolrQueryTranslator(this.SolrQuery, this.QueryOptions);
            var result = translator.Translate(this, expression);
            return Operations.QueryAsync(result.Item1, result.Item2);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            object providerResult = this.Execute(expression);

            TResult result = default(TResult);

            try
            {
                result = (TResult)providerResult;
            }
            catch (InvalidCastException exception)
            {
                string message =
                    string.Format(
                        "Query should return object of type '{0}'. Requested return type {1}.",
                        typeof(SolrQueryResults<>),
                        typeof(TResult));
                throw new InvalidOperationException(message, exception);
            }

            return result;
        }
    }
}