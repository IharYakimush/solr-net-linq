using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using SolrNet.Mapping.Validation;

namespace SolrNet.Linq
{
    public static class SolrLinqExtensions
    {
        public static async Task<TSource> FirstAsync<TSource>(this IQueryable<TSource> query)
        {
            if (query.Provider is SolrQueryProvider<TSource> provider)
            {
                MethodCallExpression mce = Expression.Call(
                    null,
                    GetMethod<TSource>(nameof(Queryable.First), 1),
                    query.Expression);

                TSource result = await provider.ExecuteAsync<TSource>(mce);

                return result;
            }

            return query.First();
        }

        public static async Task<TSource> FirstAsync<TSource>(this IQueryable<TSource> query, Expression<Func<TSource, bool>> predicate)
        {
            if (query.Provider is SolrQueryProvider<TSource> provider)
            {
                TSource result = await provider.ExecuteAsync<TSource>(Expression.Call(
                    null,
                    GetMethod<TSource>(nameof(Queryable.First), 2), query.Expression, predicate));

                return result;
            }

            return query.First(predicate);
        }

        public static async Task<TSource> FirstOrDefaultAsync<TSource>(this IQueryable<TSource> query)
        {
            if (query.Provider is SolrQueryProvider<TSource> provider)
            {
                TSource result = await provider.ExecuteAsync<TSource>(Expression.Call(
                    null,
                    GetMethod<TSource>(nameof(Queryable.FirstOrDefault), 1), query.Expression));

                return result;
            }

            return query.FirstOrDefault();
        }

        public static async Task<TSource> FirstOrDefaultAsync<TSource>(this IQueryable<TSource> query, Expression<Func<TSource, bool>> predicate)
        {
            if (query.Provider is SolrQueryProvider<TSource> provider)
            {
                TSource result = await provider.ExecuteAsync<TSource>(Expression.Call(
                    null,
                    GetMethod<TSource>(nameof(Queryable.FirstOrDefault), 2), query.Expression, predicate));

                return result;
            }

            return query.FirstOrDefault(predicate);
        }

        public static async Task<TSource> SingleAsync<TSource>(this IQueryable<TSource> query)
        {
            if (query.Provider is SolrQueryProvider<TSource> provider)
            {
                TSource result = await provider.ExecuteAsync<TSource>(Expression.Call(
                    null,
                    GetMethod<TSource>(nameof(Queryable.Single), 1), query.Expression));

                return result;
            }

            return query.Single();
        }

        public static async Task<TSource> SingleAsync<TSource>(this IQueryable<TSource> query, Expression<Func<TSource, bool>> predicate)
        {
            if (query.Provider is SolrQueryProvider<TSource> provider)
            {
                TSource result = await provider.ExecuteAsync<TSource>(Expression.Call(
                    null,
                    GetMethod<TSource>(nameof(Queryable.Single), 2), query.Expression, predicate));

                return result;
            }

            return query.Single(predicate);
        }

        public static async Task<TSource> SingleOrDefaultAsync<TSource>(this IQueryable<TSource> query)
        {
            if (query.Provider is SolrQueryProvider<TSource> provider)
            {
                TSource result = await provider.ExecuteAsync<TSource>(Expression.Call(
                    null,
                    GetMethod<TSource>(nameof(Queryable.SingleOrDefault), 1), query.Expression));

                return result;
            }

            return query.SingleOrDefault();
        }

        public static async Task<TSource> SingleOrDefaultAsync<TSource>(this IQueryable<TSource> query, Expression<Func<TSource, bool>> predicate)
        {
            if (query.Provider is SolrQueryProvider<TSource> provider)
            {
                TSource result = await provider.ExecuteAsync<TSource>(Expression.Call(
                    null,
                    GetMethod<TSource>(nameof(Queryable.SingleOrDefault), 2), query.Expression, predicate));

                return result;
            }

            return query.SingleOrDefault(predicate);
        }

        private static MethodInfo GetMethod<T>(string methodName, int paramsCount)
        {
            var candidates = typeof(Queryable).GetMember(methodName, MemberTypes.Method, BindingFlags.Public | BindingFlags.Static);

            foreach (MethodInfo candidate in candidates)
            {
                var genericArguments = candidate.GetGenericArguments();
                if (genericArguments.Length == 1 && candidate.GetParameters().Length == paramsCount)
                {
                    return candidate.MakeGenericMethod(typeof(T));
                }
            }

            throw new InvalidOperationException($"Unable to get method {methodName} to build enumerable expression");
        }
    }
}