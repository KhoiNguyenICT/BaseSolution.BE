using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using BaseSolution.Core.Commons.Enums;
using Microsoft.EntityFrameworkCore;

namespace BaseSolution.Core.Commons.Extensions
{
    public class QueryResult<T>
    {
        public long Count { get; set; }
        public IEnumerable<T> Items { get; set; }
    }

    public static class QueryResultExtension
    {
        #region ToQueryResult<T>

        public static async Task<QueryResult<T>> ToQueryResult<T>(
            this IQueryable<T> queryable,
            int skip,
            int take)
        {
            return new QueryResult<T>
            {
                Count = await queryable.CountAsync(),
                Items = await queryable.Skip(skip).Take(take).ToListAsync()
            };
        }

        public static async Task<QueryResult<T>> ToQueryResult<T>(
            this IQueryable<T> queryable,
            int skip,
            int take,
            SortDirection sortDirection,
            params Expression<Func<T, object>>[] sortExpressions)
        {
            return new QueryResult<T>
            {
                Count = await queryable.CountAsync(),
                Items = await queryable.SortBy(sortDirection, sortExpressions).Skip(skip).Take(take).ToListAsync()
            };
        }

        public static async Task<QueryResult<T>> ToQueryResult<T>(
            this IQueryable<T> queryable,
            int skip,
            int take,
            SortDirection sortDirection,
            IEnumerable<Expression<Func<T, object>>> sortExpressions)
        {
            return await queryable.ToQueryResult(skip, take, sortDirection, sortExpressions?.ToArray());
        }

        public static async Task<QueryResult<T>> ToQueryResult<T>(
            this IQueryable<T> queryable,
            int skip,
            int take,
            Expression<Func<T, bool>> predicate)
        {
            return await queryable.Where(predicate).ToQueryResult(skip, take);
        }

        public static async Task<QueryResult<T>> ToQueryResult<T>(
            this IQueryable<T> queryable,
            int skip,
            int take,
            params Expression<Func<T, object>>[] includes) where T : class
        {
            return await queryable.IncludeAll(includes).ToQueryResult(skip, take);
        }

        public static async Task<QueryResult<T>> ToQueryResult<T>(
            this IQueryable<T> queryable,
            int skip,
            int take,
            Expression<Func<T, bool>> predicate,
            params Expression<Func<T, object>>[] includes) where T : class
        {
            return await queryable.Where(predicate).IncludeAll(includes).ToQueryResult(skip, take);
        }

        public static async Task<QueryResult<T>> ToQueryResult<T>(
            this IQueryable<T> queryable,
            int skip,
            int take,
            Expression<Func<T, bool>> predicate,
            IEnumerable<Expression<Func<T, object>>> includes,
            SortDirection sortDirection = SortDirection.Ascending,
            params Expression<Func<T, object>>[] sortExpressions) where T : class
        {
            return await queryable.Where(predicate).IncludeAll(includes?.ToArray()).ToQueryResult(skip, take, sortDirection, sortExpressions);
        }

        public static async Task<QueryResult<T>> ToQueryResult<T>(
            this IQueryable<T> queryable,
            int skip,
            int take,
            Expression<Func<T, bool>> predicate,
            IEnumerable<Expression<Func<T, object>>> includes,
            SortDirection sortDirection,
            IEnumerable<Expression<Func<T, object>>> sortExpressions) where T : class
        {
            return await queryable.Where(predicate).IncludeAll(includes?.ToArray()).ToQueryResult(skip, take, sortDirection, sortExpressions);
        }

        #endregion

        #region ToQueryResult<T, TOut>

        public static async Task<QueryResult<TOut>> ToQueryResult<T, TOut>(
            this IQueryable<T> queryable,
            int skip,
            int take,
            Expression<Func<T, TOut>> selector)
        {
            return new QueryResult<TOut>
            {
                Count = await queryable.CountAsync(),
                Items = await queryable.Select(selector).Skip(skip).Take(take).ToListAsync()
            };
        }

        public static async Task<QueryResult<TOut>> ToQueryResult<T, TOut>(
            this IQueryable<T> queryable,
            int skip,
            int take,
            Expression<Func<T, TOut>> selector,
            SortDirection sortDirection,
            params Expression<Func<T, object>>[] sortExpressions)
        {
            return new QueryResult<TOut>
            {
                Count = await queryable.CountAsync(),
                Items = await queryable.SortBy(sortDirection, sortExpressions).Select(selector).Skip(skip).Take(take).ToListAsync()
            };
        }

        public static async Task<QueryResult<TOut>> ToQueryResult<T, TOut>(
            this IQueryable<T> queryable,
            int skip,
            int take,
            Expression<Func<T, TOut>> selector,
            SortDirection sortDirection,
            IEnumerable<Expression<Func<T, object>>> sortExpressions)
        {
            return await queryable.ToQueryResult(skip, take, selector, sortDirection, sortExpressions?.ToArray());
        }

        public static async Task<QueryResult<TOut>> ToQueryResult<T, TOut>(
            this IQueryable<T> queryable,
            int skip,
            int take,
            Expression<Func<T, TOut>> selector,
            Expression<Func<T, bool>> predicate)
        {
            return await queryable.Where(predicate).ToQueryResult(skip, take, selector);
        }

        public static async Task<QueryResult<TOut>> ToQueryResult<T, TOut>(
            this IQueryable<T> queryable,
            int skip,
            int take,
            Expression<Func<T, TOut>> selector,
            params Expression<Func<T, object>>[] includes) where T : class
        {
            return await queryable.IncludeAll(includes).ToQueryResult(skip, take, selector);
        }

        public static async Task<QueryResult<TOut>> ToQueryResult<T, TOut>(
            this IQueryable<T> queryable,
            int skip,
            int take,
            Expression<Func<T, TOut>> selector,
            Expression<Func<T, bool>> predicate,
            params Expression<Func<T, object>>[] includes) where T : class
        {
            return await queryable.Where(predicate).IncludeAll(includes).ToQueryResult(skip, take, selector);
        }

        public static async Task<QueryResult<TOut>> ToQueryResult<T, TOut>(
            this IQueryable<T> queryable,
            int skip,
            int take,
            Expression<Func<T, TOut>> selector,
            Expression<Func<T, bool>> predicate,
            IEnumerable<Expression<Func<T, object>>> includes,
            SortDirection sortDirection = SortDirection.Ascending,
            params Expression<Func<T, object>>[] sortExpressions) where T : class
        {
            return await queryable.Where(predicate).IncludeAll(includes?.ToArray()).ToQueryResult(skip, take, selector, sortDirection, sortExpressions);
        }

        public static async Task<QueryResult<TOut>> ToQueryResult<T, TOut>(
            this IQueryable<T> queryable,
            int skip,
            int take,
            Expression<Func<T, TOut>> selector,
            Expression<Func<T, bool>> predicate,
            IEnumerable<Expression<Func<T, object>>> includes,
            SortDirection sortDirection,
            IEnumerable<Expression<Func<T, object>>> sortExpressions) where T : class
        {
            return await queryable.Where(predicate).IncludeAll(includes?.ToArray()).ToQueryResult(skip, take, selector, sortDirection, sortExpressions?.ToArray());
        }

        #endregion
    }
}
