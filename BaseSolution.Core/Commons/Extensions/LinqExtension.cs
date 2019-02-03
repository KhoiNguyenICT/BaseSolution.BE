using BaseSolution.Core.Commons.Enums;
using BaseSolution.Core.Commons.Utils;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace BaseSolution.Core.Commons.Extensions
{
    public static class LinqExtension
    {
        public static Func<T, object> GetLambda<T>(string property)
        {
            var param = Expression.Parameter(typeof(T), "p");

            Expression parent = Expression.Property(param, property);
            var isValueType = parent.Type.GetTypeInfo().IsValueType;
            if (isValueType)
            {
                return Expression.Lambda<Func<T, object>>(parent, param).Compile();
            }
            var convert = Expression.Convert(parent, typeof(object));
            return Expression.Lambda<Func<T, object>>(convert, param).Compile();
        }

        public static Expression<T> ComposeExpression<T>(this Expression<T> first, Expression<T> second, Func<Expression, Expression, Expression> merge)
        {
            if (first.IsEquals(second)) return first;
            var map = first.Parameters.Select((f, i) => new { f, s = second.Parameters[i] }).ToDictionary(p => p.s, p => p.f);
            var secondBody = ParameterRebinder.ReplaceParameters(map, second.Body);
            return Expression.Lambda<T>(merge(first.Body, secondBody), first.Parameters);
        }

        public static Expression<Func<T, bool>> Compose<T>(
            this Expression<Func<T, bool>> first,
            Expression<Func<T, bool>> second,
            Func<Expression, Expression, Expression> merge)
        {
            if (first.IsEquals(t => true)) return second;
            if (second.IsEquals(t => true)) return first;
            return ComposeExpression(first, second, merge);
        }

        public static Expression<Func<T, bool>> AndAlso<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
        {
            return first.Compose(second, Expression.AndAlso);
        }

        public static Expression<Func<T, bool>> AndAlso<T>(this Expression<Func<T, bool>> first, params Expression<Func<T, bool>>[] seconds)
        {
            if (seconds == null || seconds.All(t => t == null)) return first;
            return seconds.Where(t => t != null).Aggregate(first, (current, expression) => current.AndAlso(expression));
        }

        public static Expression<Func<T, bool>> OrElse<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
        {
            return first.Compose(second, Expression.OrElse);
        }

        public static Expression<Func<T, bool>> OrElse<T>(this Expression<Func<T, bool>> first, params Expression<Func<T, bool>>[] seconds)
        {
            if (seconds == null || seconds.All(t => t == null)) return first;
            return seconds.Where(t => t != null).Aggregate(first, (current, expression) => current.OrElse(expression));
        }

        public static IQueryable<T> SortBy<T>(this IQueryable<T> query, SortDirection sortDirection, params Expression<Func<T, object>>[] sortExpressions)
        {
            if (sortExpressions == null || sortExpressions.All(t => t == null)) return query;
            IOrderedQueryable<T> orderedQuery = null;
            for (var i = 0; i < sortExpressions.Length; i++)
            {
                var sortExpItem = sortExpressions[i];
                if (sortExpItem == null) continue;
                if (sortDirection == SortDirection.Descending)
                {
                    orderedQuery = i == 0 ? query.OrderByDescending(sortExpItem) : orderedQuery.ThenByDescending(sortExpItem);
                }
                else
                {
                    orderedQuery = i == 0 ? query.OrderBy(sortExpItem) : orderedQuery.ThenBy(sortExpItem);
                }
            }
            return orderedQuery ?? query;
        }

        public static IQueryable<T> SortBy<T>(this IQueryable<T> query, SortDirection sortDirection, IEnumerable<Expression<Func<T, object>>> sortExpressions)
        {
            return query.SortBy(sortDirection, sortExpressions?.ToArray());
        }

        public static IQueryable<T> IncludeAll<T>(this IQueryable<T> queryable, params Expression<Func<T, object>>[] includes) where T : class
        {
            if (includes == null || includes.All(t => t == null)) return queryable;
            return includes.Where(include => include != null).Aggregate(queryable, (current, include) => current.Include(include));
        }
    }

    public class ParameterRebinder : ExpressionVisitor
    {
        private readonly Dictionary<ParameterExpression, ParameterExpression> _map;

        public ParameterRebinder(Dictionary<ParameterExpression, ParameterExpression> map)
        {
            _map = map ?? new Dictionary<ParameterExpression, ParameterExpression>();
        }

        public static Expression ReplaceParameters(Dictionary<ParameterExpression, ParameterExpression> map, Expression exp)
        {
            return new ParameterRebinder(map).Visit(exp);
        }

        protected override Expression VisitParameter(ParameterExpression p)
        {
            if (_map.TryGetValue(p, out var replacement))
            {
                p = replacement;
            }
            return base.VisitParameter(p);
        }
    }
}