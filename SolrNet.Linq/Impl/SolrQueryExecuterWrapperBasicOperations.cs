using System;
using System.Threading.Tasks;
using SolrNet.Commands.Parameters;
using SolrNet.Impl;

namespace SolrNet.Linq.Impl
{
    public class SolrQueryExecuterWrapperBasicOperations<T> : IExecuter<T>
    {
        private readonly ISolrBasicReadOnlyOperations<T> _basicOperations;

        public SolrQueryExecuterWrapperBasicOperations(ISolrBasicReadOnlyOperations<T> basicOperations)
        {
            _basicOperations = basicOperations ?? throw new ArgumentNullException(nameof(basicOperations));
        }
        public SolrQueryResults<T> Execute(ISolrQuery q, QueryOptions options)
        {
            return this._basicOperations.Query(q, options);
        }       

        public Task<SolrQueryResults<T>> ExecuteAsync(ISolrQuery q, QueryOptions options)
        {
            return this._basicOperations.QueryAsync(q, options);
        }

        public SolrQueryExecuter<T> Executer
        {
            get
            {
                if (this._basicOperations is SolrBasicServer<T> sbs)
                {
                    return sbs.GetSingleField<ISolrQueryExecuter<T>>() as SolrQueryExecuter<T>;
                }

                if (this._basicOperations is SolrServer<T> srv)
                {
                    if (srv.GetSingleField<ISolrBasicOperations<T>>() is SolrBasicServer<T> sbs2)
                    {
                        return sbs2.GetSingleField<ISolrQueryExecuter<T>>() as SolrQueryExecuter<T>;
                    }
                }

                throw new InvalidOperationException(
                    $"Unable to get executer from current instance of type {_basicOperations.GetType()}");
            }
        }
    }
}