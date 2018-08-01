using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SolrNet.Linq
{
    public class SolrQuery<T> : IOrderedQueryable<T>
    {
        public SolrQuery(SolrQueryProvider<T> provider)
        {
            this.Provider = provider ?? throw new ArgumentNullException(nameof(provider));
            this.Expression = Expression.Constant(this);
        }

        public SolrQuery(SolrQueryProvider<T> provider, Expression expression)
        {
            this.Expression = expression ?? throw new ArgumentNullException(nameof(expression));            

            this.Provider = provider ?? throw new ArgumentNullException(nameof(provider));

            if (!typeof(IQueryable<T>).IsAssignableFrom(expression.Type))
            {
                throw new ArgumentOutOfRangeException(nameof(expression));
            }
        }

        public Type ElementType => typeof(T);

        public Expression Expression { get; }

        public IQueryProvider Provider { get; }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)this.Provider.Execute(this.Expression)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this.Provider.Execute(this.Expression)).GetEnumerator();
        }
    }
}