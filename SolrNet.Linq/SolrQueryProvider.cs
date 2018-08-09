using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using SolrNet.Commands.Parameters;
using SolrNet.Linq.Expressions;
using SolrNet.Linq.Expressions.Context;
using SolrNet.Linq.Impl;

namespace SolrNet.Linq
{
    public class SolrQueryProvider<TEntity> : IQueryProvider, IAsyncProvider<TEntity>
    {  
        public IExecuter<TEntity> Operations { get; }
        public SolrNetLinqOptions Options { get; }

        public SolrQueryProvider(IExecuter<TEntity> operations, SolrNetLinqOptions options, MemberContext context)
        {
            Operations = operations ?? throw new ArgumentNullException(nameof(operations));
            Options = options ?? throw new ArgumentNullException(nameof(options));

            if (context == null)
            {
                this.MemberContext = MemberContext.ForType<TEntity>();
                this.MemberContext.FieldSerializer = this.Options.SolrFieldSerializer;
                this.MemberContext.MappingManager = this.Options.MappingManager;
            }
            else
            {
                this.MemberContext = context;
            }            
        }

        public IQueryable CreateQuery(Expression expression)
        {
            Type elementType = TypeSystem.GetElementType(expression.Type);

            if (elementType == typeof(TEntity))
            {
                return new SolrQuery<TEntity>(this, expression);
            }

            if (expression is MethodCallExpression se)
            {
                if (se.Arguments.Count > 1 && se.Method.DeclaringType == typeof(Queryable) &&
                    se.Method.Name == nameof(Queryable.Select))
                {
                    return (IQueryable) Activator.CreateInstance(
                        typeof(SolrQuery<>).MakeGenericType(elementType),
                        Activator.CreateInstance(
                            typeof(SolrQueryProvider<>).MakeGenericType(elementType),
                            typeof(ExecuterExtensions)
                                .GetMethod(nameof(ExecuterExtensions.ChangeType), BindingFlags.Public | BindingFlags.Static)
                                .MakeGenericMethod(elementType, typeof(TEntity))
                                .Invoke(null, new object[] {this.Operations, this.Options.SolrFieldParser }),
                            this.Options,
                            this.MemberContext),
                        expression);
                }
            }

            throw new InvalidOperationException();
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return (IQueryable<TElement>) this.CreateQuery(expression);
        }

        public object Execute(Expression expression)
        {
            Tuple<ISolrQuery, QueryOptions, EnumeratedResult> result = Translate(expression);

            SolrQueryResults<TEntity> solrQueryResults =
                Operations.Execute(this.Options.MainQuery ?? result.Item1, result.Item2);

            return HandleResults(result, solrQueryResults);
        }

        private object HandleResults(Tuple<ISolrQuery, QueryOptions, EnumeratedResult> result, SolrQueryResults<TEntity> solrQueryResults)
        {
            switch (result.Item3)
            {
                case EnumeratedResult.First: return solrQueryResults.First();
                case EnumeratedResult.FirstOrDefault: return solrQueryResults.FirstOrDefault();
                case EnumeratedResult.Single: return solrQueryResults.Single();
                case EnumeratedResult.SingleOrDefault: return solrQueryResults.SingleOrDefault();
                case EnumeratedResult.Any: return solrQueryResults.NumFound > 0;
                case EnumeratedResult.Count: return solrQueryResults.NumFound;
                case EnumeratedResult.LongCount: return (long)solrQueryResults.NumFound;

                default: return solrQueryResults;
            }
        }

        private Tuple<ISolrQuery, QueryOptions, EnumeratedResult> Translate(Expression expression)
        {
            SolrQueryTranslator translator = new SolrQueryTranslator(this.Options, this.MemberContext);
            Tuple<ISolrQuery, QueryOptions, EnumeratedResult> result = translator.Translate(this, expression);
            this.Options.SetupQueryOptions?.Invoke(result.Item2);
            return result;
        }
        
        public MemberContext MemberContext { get; set; }

        public async Task<object> ExecuteAsync(Expression expression)
        {
            Tuple<ISolrQuery, QueryOptions, EnumeratedResult> result = Translate(expression);

            SolrQueryResults<TEntity> solrQueryResults =
                await Operations.ExecuteAsync(this.Options.MainQuery ?? result.Item1, result.Item2);

            return HandleResults(result, solrQueryResults);            
        }

        public TResult Execute<TResult>(Expression expression)
        {
            object providerResult = this.Execute(expression);

            return CastResult<TResult>(providerResult);
        }

        public async Task<TResult> ExecuteAsync<TResult>(Expression expression)
        {
            object providerResult = await this.ExecuteAsync(expression);

            return CastResult<TResult>(providerResult);
        }

        private static TResult CastResult<TResult>(object providerResult)
        {
            TResult result = default(TResult);

            try
            {
                result = (TResult) providerResult;
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