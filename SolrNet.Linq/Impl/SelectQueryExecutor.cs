using System;
using System.Threading.Tasks;
using SolrNet.Commands.Parameters;
using SolrNet.Impl;

namespace SolrNet.Linq.Impl
{
    public class SelectQueryExecutor<TNew> : IExecuter<TNew>
    {
        public SelectQueryExecutor(
            SolrQueryExecuter<TNew> executer)
        {
            Executer = executer ?? throw new ArgumentNullException(nameof(executer));
        }
        public SolrQueryResults<TNew> Execute(ISolrQuery q, QueryOptions options)
        {
            return this.Executer.Execute(q, options);
        }

        public Task<SolrQueryResults<TNew>> ExecuteAsync(ISolrQuery q, QueryOptions options)
        {
            return this.Executer.ExecuteAsync(q, options);
        }

        public SolrQueryExecuter<TNew> Executer { get; }
    }    
}