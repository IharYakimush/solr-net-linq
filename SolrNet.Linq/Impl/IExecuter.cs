using System.Threading.Tasks;
using SolrNet.Commands.Parameters;
using SolrNet.Impl;

namespace SolrNet.Linq.Impl
{
    public interface IExecuter<T>
    {
        SolrQueryResults<T> Execute(ISolrQuery q, QueryOptions options);

        Task<SolrQueryResults<T>> ExecuteAsync(ISolrQuery q, QueryOptions options);

        SolrQueryExecuter<T> Executer { get; }
    }
}