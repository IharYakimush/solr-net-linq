using System.Collections.Generic;
using System.Linq;

namespace SolrNet.Linq.Expressions
{
    public static class SolrQueryExtensions
    {
        public static ISolrQuery TrySimplify(this ISolrQuery query)
        {
            if (query is SolrMultipleCriteriaQuery q)
            {
                if (q.Queries.Any(sq => sq == SolrQuery.All))
                {
                    if (q.Oper == SolrMultipleCriteriaQuery.Operator.AND)
                    {
                        ISolrQuery[] r = q.Queries.Where(sq => sq != SolrQuery.All).ToArray();

                        if (r.Length == 1)
                        {
                            return r.Single();
                        }

                        if (r.Length > 1)
                        {
                            return new SolrMultipleCriteriaQuery(r, q.Oper);
                        }
                    }

                    if (q.Oper == SolrMultipleCriteriaQuery.Operator.OR)
                    {
                        return SolrQuery.All;
                    }
                }

                if (q.Queries.Any(sq => sq.IsNothing()))
                {
                    if (q.Oper == SolrMultipleCriteriaQuery.Operator.AND)
                    {
                        return q.Queries.First(sq => sq.IsNothing());
                    }

                    if (q.Oper == SolrMultipleCriteriaQuery.Operator.OR)
                    {
                        ISolrQuery[] r = q.Queries.Where(sq => !sq.IsNothing()).ToArray();

                        if (r.Length == 1)
                        {
                            return r.Single();
                        }

                        if (r.Length > 1)
                        {
                            return new SolrMultipleCriteriaQuery(r, q.Oper);
                        }
                    }
                }
            }

            return query;
        }

        public static bool IsNothing(this ISolrQuery query)
        {
            if (query is SolrMultipleCriteriaQuery q)
            {
                if (q.Oper == "NOT")
                {
                    if (q.Queries.Count() == 2)
                    {
                        return q.Queries.ElementAt(0) == SolrQuery.All && q.Queries.ElementAt(1) == SolrQuery.All;
                    }
                }
            }

            return false;
        }
    }
}