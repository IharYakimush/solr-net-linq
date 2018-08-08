using System;
using System.Threading.Tasks;
using SolrNet.Commands.Parameters;
using SolrNet.Impl;

namespace SolrNet.Linq.Impl
{
    public class SelectQueryExecutor<T> : IExecuter<T>
    {
        private readonly SolrQueryExecuter<T> _executer;

        public SelectQueryExecutor(SolrQueryExecuter<T> executer)
        {
            _executer = executer ?? throw new ArgumentNullException(nameof(executer));
        }
        public SolrQueryResults<T> Execute(ISolrQuery q, QueryOptions options)
        {
            return this._executer.Execute(q, options);
        }

        public Task<SolrQueryResults<T>> ExecuteAsync(ISolrQuery q, QueryOptions options)
        {
            return this._executer.ExecuteAsync(q, options);
        }

        public SolrQueryExecuter<T> Executer => this._executer;
    }
}