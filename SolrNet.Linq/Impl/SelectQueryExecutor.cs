using System;
using System.Threading.Tasks;
using SolrNet.Commands.Parameters;
using SolrNet.Impl;

namespace SolrNet.Linq.Impl
{
    public class SelectQueryExecutor<T> : IExecuter<T>
    {
        public SelectQueryExecutor(
            SolrQueryExecuter<T> executer)
        {
            Executer = executer ?? throw new ArgumentNullException(nameof(executer));
        }
        public SolrQueryResults<T> Execute(ISolrQuery q, QueryOptions options)
        {
            return this.Executer.Execute(q, options);
        }

        public Task<SolrQueryResults<T>> ExecuteAsync(ISolrQuery q, QueryOptions options)
        {
            return this.Executer.ExecuteAsync(q, options);
        }

        public SolrQueryExecuter<T> Executer { get; }
    }    
}