using BaseSolution.Core.Models.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BaseSolution.Core.Commons.Extensions
{
    public static class DbSetExtension
    {
        public static async Task<T> Get<T>(this DbSet<T> dbSet, Guid id, params Expression<Func<T, object>>[] includes)
            where T : class, IEntity
        {
            return includes == null ?
                await dbSet.FindAsync(id) :
                await dbSet.Where(t => t.Id == id).IncludeAll(includes).FirstOrDefaultAsync();
        }

        public static async Task<T> GetWithDetach<T>(this DbSet<T> dbSet, Guid id, params Expression<Func<T, object>>[] includes)
            where T : class, IEntity
        {
            return await dbSet.AsNoTracking().Where(t => t.Id == id).IncludeAll(includes).FirstOrDefaultAsync();
        }

        public static async Task<T> GetOne<T>(this DbSet<T> dbSet, Expression<Func<T, bool>> filterExpression, params Expression<Func<T, object>>[] includes)
            where T : class
        {
            return await dbSet.Where(filterExpression).IncludeAll(includes).FirstOrDefaultAsync();
        }

        public static async Task<T> GetOneWithDetach<T>(this DbSet<T> dbSet, Expression<Func<T, bool>> filterExpression, params Expression<Func<T, object>>[] includes)
            where T : class
        {
            return await dbSet.AsNoTracking().IncludeAll(includes).FirstOrDefaultAsync(filterExpression);
        }

        public static async Task<List<T>> GetMany<T>(this DbSet<T> dbSet, Expression<Func<T, bool>> filterExpression, params Expression<Func<T, object>>[] includes)
            where T : class, IEntity
        {
            return await dbSet.Where(filterExpression).IncludeAll(includes).ToListAsync();
        }

        public static IQueryable<T> Queryable<T>(this DbSet<T> dbSet, Expression<Func<T, bool>> filterExpression, params Expression<Func<T, object>>[] includes)
            where T : class
        {
            return dbSet.Where(filterExpression).IncludeAll(includes);
        }

        public static void UpdateObject<T>(this DbSet<T> dbSet, T entity)
            where T : class, IEntity
        {
            entity.UpdatedDate = DateTime.UtcNow;
            dbSet.Update(entity);
        }

        public static void AddObject<T>(this DbSet<T> dbSet, T entity)
            where T : class, IEntity
        {
            entity.CreatedDate = DateTime.UtcNow;
            entity.UpdatedDate = DateTime.UtcNow;
            dbSet.Add(entity);
        }

        public static void Delete<T>(this DbSet<T> dbSet, Guid id)
            where T : class, IEntity
        {
            var entity = Activator.CreateInstance<T>();
            entity.Id = id;
            dbSet.Remove(entity);
        }

        public static void Delete<T>(this DbSet<T> dbSet, T entity)
            where T : class, IEntity
        {
            dbSet.Remove(entity);
        }

        public static void DeleteAny<T>(this DbSet<T> dbSet, Expression<Func<T, bool>> fiterExpression)
            where T : class
        {
            dbSet.RemoveRange(dbSet.Where(fiterExpression));
        }
    }
}