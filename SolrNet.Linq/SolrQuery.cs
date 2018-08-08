using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SolrNet.Linq
{
    public class SolrQuery<TEntity> : IOrderedQueryable<TEntity>
    {
        public SolrQuery(SolrQueryProvider<TEntity> provider)
        {
            this.Provider = provider ?? throw new ArgumentNullException(nameof(provider));
            this.Expression = Expression.Constant(this);
        }

        public SolrQuery(SolrQueryProvider<TEntity> provider, Expression expression)
        {
            this.Expression = expression ?? throw new ArgumentNullException(nameof(expression));            

            this.Provider = provider ?? throw new ArgumentNullException(nameof(provider));

            if (!typeof(IQueryable<TEntity>).IsAssignableFrom(expression.Type))
            {
                throw new ArgumentOutOfRangeException(nameof(expression));
            }
        }

        public Type ElementType => typeof(TEntity);

        public Expression Expression { get; }

        public IQueryProvider Provider { get; }

        public IEnumerator<TEntity> GetEnumerator()
        {
            return ((IEnumerable<TEntity>)this.Provider.Execute(this.Expression)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this.Provider.Execute(this.Expression)).GetEnumerator();
        }
    }
}