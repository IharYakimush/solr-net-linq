using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using SolrNet.Commands.Parameters;
using SolrNet.Linq.Expressions;
using SolrNet.Linq.Expressions.Context;

namespace SolrNet.Linq
{
    public class SolrQueryProvider<TEntity> : IQueryProvider
    {
        public ISolrBasicReadOnlyOperations<TEntity> Operations { get; }
        public SolrNetLinqOptions Options { get; }

        public SolrQueryProvider(ISolrBasicReadOnlyOperations<TEntity> operations, SolrNetLinqOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            Operations = operations ?? throw new ArgumentNullException(nameof(operations));
            Options = options;
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
            SolrQueryTranslator translator = new SolrQueryTranslator(this.Options, this.MemberContext);
            Tuple<ISolrQuery, QueryOptions, EnumeratedResult> result = translator.Translate(this, expression);
            this.Options.SetupQueryOptions?.Invoke(result.Item2);
            SolrQueryResults<TEntity> solrQueryResults =
                Operations.Query(this.Options.MainQuery ?? result.Item1, result.Item2);

            switch (result.Item3)
            {
                case EnumeratedResult.First: return solrQueryResults.First();
                case EnumeratedResult.FirstOrDefault: return solrQueryResults.FirstOrDefault();
                case EnumeratedResult.Single: return solrQueryResults.Single();
                case EnumeratedResult.SingleOrDefault: return solrQueryResults.SingleOrDefault();

                default: return solrQueryResults;
            }
        }

        public MemberContext MemberContext
        {
            get
            {
                MemberContext memberContext = MemberContext.ForType<TEntity>();
                memberContext.FieldSerializer = this.Options.SolrFieldSerializer;
                memberContext.MappingManager = this.Options.MappingManager;
                return memberContext;
            }
        }

        public Task<SolrQueryResults<TEntity>> ExecuteAsync(Expression expression)
        {
            SolrQueryTranslator
                translator = new SolrQueryTranslator(this.Options, this.MemberContext);
            var result = translator.Translate(this, expression);
            this.Options.SetupQueryOptions?.Invoke(result.Item2);
            
            return Operations.QueryAsync(this.Options.MainQuery ?? result.Item1, result.Item2);
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