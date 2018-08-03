using System.Collections.Generic;
using System.Linq;
using SolrNet.Impl;

namespace SolrNet.Linq.Expressions
{
    public static class SolrQueryExtensions
    {
        public static ISolrQuery TrySimplify(this ISolrQuery query)
        {
            if (query is SolrMultipleCriteriaQuery q)
            {
                LinkedList<ISolrQuery> toTrim = new LinkedList<ISolrQuery>();

                if (q.Queries.Any(sq => sq == SolrQuery.All))
                {
                    if (q.Oper == SolrMultipleCriteriaQuery.Operator.AND)
                    {
                        toTrim.AddLast(SolrQuery.All);
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
                        foreach (ISolrQuery solrQuery in q.Queries.Where(sq => sq.IsNothing()))
                        {
                            toTrim.AddLast(solrQuery);
                        }
                    }
                }

                IEnumerable<IGrouping<string, ISolrQuery>> ranges = q.Queries
                    .Where(sq => sq is SolrHasValueQuery || sq is ISolrQueryByRange).GroupBy(
                        r =>
                        {
                            if (r is SolrHasValueQuery hq)
                            {
                                return hq.Field;
                            }

                            return (r as ISolrQueryByRange).FieldName;
                        }
                    );

                foreach (var gr in ranges.Where(g => g.Count() > 1))
                {
                    if (q.Oper == SolrMultipleCriteriaQuery.Operator.AND)
                    {
                        foreach (SolrHasValueQuery hv in gr.OfType<SolrHasValueQuery>())
                        {
                            toTrim.AddLast(hv);
                        }
                    }

                    if (q.Oper == SolrMultipleCriteriaQuery.Operator.OR)
                    {
                        foreach (ISolrQuery br in gr.OfType<ISolrQueryByRange>().OfType<ISolrQuery>())
                        {
                            toTrim.AddLast(br);
                        }
                    }
                }                

                if (toTrim.Any())
                {
                    ISolrQuery[] result = q.Queries.Except(toTrim).ToArray();

                    if (result.Length == 1)
                    {
                        return result.Single();
                    }

                    if (result.Length > 1)
                    {
                        return new SolrMultipleCriteriaQuery(result, q.Oper);
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

        public static ISolrQuery CreateNotSolrQuery(this ISolrQuery operand)
        {
            if (operand is SolrMultipleCriteriaQuery notQuery)
            {
                if (notQuery.Oper == "NOT")
                {
                    return notQuery.Queries.ElementAt(1);
                }
            }

            return new SolrMultipleCriteriaQuery(new[] { SolrQuery.All, operand }, "NOT");
        }
    }
}