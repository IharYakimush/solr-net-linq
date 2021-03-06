﻿using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SolrNet.Linq.Impl
{
    public interface IAsyncProvider<TEntity>
    {
        Task<TResult> ExecuteAsync<TResult>(Expression expression);
    }
}