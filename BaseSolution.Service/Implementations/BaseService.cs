using AutoMapper;
using BaseSolution.Core.Commons.Enums;
using BaseSolution.Core.Commons.Errors;
using BaseSolution.Core.Commons.Extensions;
using BaseSolution.Core.Models.Entities;
using BaseSolution.Core.Models.Interfaces;
using BaseSolution.Core.Service.Dtos;
using BaseSolution.Core.Service.Interfaces;
using BaseSolution.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BaseSolution.Service.Implementations
{
    public abstract class BaseService<T, TDto> : IService<T, TDto>
        where T : BaseEntity, IEntity, new()
        where TDto : BaseDto, IDto, new()
    {
        protected readonly AppDbContext _dbContext;
        protected readonly DbSet<T> _dbSet;
        protected readonly IMapper _mapper;
        protected readonly IEsClientProvider _esClient;

        protected BaseService(AppDbContext dbContext, IMapper mapper, IEsClientProvider esClient)
        {
            _dbContext = dbContext;
            _dbSet = dbContext.Set<T>();
            _mapper = mapper;
            _esClient = esClient;
        }

        public virtual async Task<QueryResult<TDto>> QueryByElasticSearch(int skip, int take, string[] sorts, IEnumerable<KeyValuePair<Expression<Func<TDto, object>>, object>> terms, string searchQuery, params Expression<Func<TDto, object>>[] searchFields)
        {
            string sortByField = string.Empty;
            SortDirection sortDirection = SortDirection.Ascending;
            if (sorts?.Length > 0)
            {
                string sort = sorts[0];
                sortByField = sort.IndexOf('-') >= 0 ? sort.Substring(1) : sort;
                sortDirection = sort.IndexOf('-') >= 0 ? SortDirection.Descending : SortDirection.Ascending;
            }
            var rs = await _esClient.QueryAsync(skip, take, sortByField, sortDirection, terms, searchQuery, searchFields);
            return _mapper.Map<QueryResult<TDto>>(rs);
        }

        public virtual async Task<QueryResult<TDto>> Query(int skip, int take, string[] sorts, string searchQuery, params Expression<Func<T, bool>>[] extraPredicates)
        {
            var exp = GetSearchExpression(searchQuery ?? "").AndAlso(extraPredicates);
            var queryable = _dbSet.Queryable(exp, IncludeExpressions());
            var sortedQuery = sorts.Aggregate(queryable, OrderBy);
            var items = await sortedQuery
                        .ToQueryResult(skip, take);
            return _mapper.Map<QueryResult<TDto>>(items);
        }

        public virtual async Task<TDto> Create(TDto entityDto, bool index = true)
        {
            var entity = _mapper.Map<T>(entityDto);
            var validationErrors = await ValidateCreate(entity);
            if (validationErrors?.Count > 0)
            {
                throw new CoreException(validationErrors);
            }
            _dbSet.AddObject(entity);
            await _dbContext.SaveChangesAsync();
            var resultDto = _mapper.Map<TDto>(entity);
            if (index) await _esClient.IndexAsync(resultDto);
            return resultDto;
        }

        public virtual async Task<TDto> Get(Guid id)
        {
            if (id == Guid.Empty) return null;
            var entity = await _dbSet.GetWithDetach(id, IncludeExpressions());
            return _mapper.Map<T, TDto>(entity);
        }

        public virtual async Task UpdateIndex(Guid id, params Expression<Func<T, object>>[] includes)
        {
            var entity = await _dbSet.GetWithDetach(id, includes);
            if (entity != null) await _esClient.UpdateAsync(_mapper.Map<TDto>(entity));
        }

        public virtual async Task<TDto> Update(TDto entityDto, bool index = true)
        {
            var entity = _mapper.Map<T>(entityDto);
            var validationErrors = await ValidateUpdate(entity);
            if (validationErrors?.Count > 0)
            {
                throw new CoreException(validationErrors);
            }
            _dbSet.UpdateObject(entity);
            _dbContext.Entry(entity).Property(x => x.CreatedDate).IsModified = false;
            await _dbContext.SaveChangesAsync();
            await _dbContext.Entry(entity).ReloadAsync();
            var resultDto = _mapper.Map<TDto>(entity);
            if (index) await _esClient.UpdateAsync(resultDto);
            return resultDto;
        }

        public virtual async Task Delete(Guid id, bool index = true)
        {
            var validationErrors = await ValidateDelete(id);
            if (validationErrors?.Count > 0)
                throw new CoreException(validationErrors);
            _dbSet.Delete(id);
            await _dbContext.SaveChangesAsync();
            if (index) await _esClient.DeleteAsync<TDto>(id);
        }

        public virtual async Task IndexAllItems()
        {
            var allItems = await _dbSet.AsNoTracking()
                .IncludeAll(IncludeExpressions())
                .ToListAsync();
            var allItemDtos = _mapper.Map<IEnumerable<TDto>>(allItems);
            await _esClient.IndexManyAsync(allItemDtos);
        }

        public virtual async Task ReIndexItem(Guid id)
        {
            var dto = await Get(id);
            if (dto == null)
                return;
            await _esClient.CreateOrUpdateIndexAsync(dto);
        }

        public virtual async Task DeleteIndexItem(Guid id)
        {
            await _esClient.DeleteAsync<TDto>(id);
        }

        public virtual QueryResult<T> ToQueryResult(IList<object> source, long count)
        {
            return new QueryResult<T>
            {
                Items = _mapper.Map<IList<T>>(source),
                Count = count
            };
        }

        protected virtual Expression<Func<T, object>>[] IncludeExpressions()
        {
            return new Expression<Func<T, object>>[] { };
        }

        protected virtual Expression<Func<T, bool>> GetSearchExpression(string searchQuery)
        {
            return t => true;
        }

        protected virtual IEnumerable<Expression<Func<T, object>>> GetSortBy(string sortByField)
        {
            switch ((sortByField ?? "").ToLower())
            {
                case "updateddate":
                    return new List<Expression<Func<T, object>>> { t => t.UpdatedDate };

                case "createddate":
                    return new List<Expression<Func<T, object>>> { t => t.CreatedDate };

                default:
                    return new List<Expression<Func<T, object>>> { t => t.UpdatedDate };
            }
        }

        protected virtual async Task<IList<CoreValidationError>> ValidateCreate(T entity)
        {
            return await ValidateChange(entity);
        }

        protected virtual async Task<IList<CoreValidationError>> ValidateUpdate(T entity)
        {
            return await ValidateChange(entity);
        }

        protected virtual async Task<IList<CoreValidationError>> ValidateChange(T entity)
        {
            return await Task.FromResult(default(IList<CoreValidationError>));
        }

        protected virtual async Task<IList<CoreValidationError>> ValidateDelete(Guid id)
        {
            return await Task.FromResult(default(IList<CoreValidationError>));
        }

        protected virtual IQueryable<T> OrderBy(IQueryable<T> queryable, string field)
        {
            return field.StartsWith("-") ? queryable.OrderByDescending(GetSortField(field.Substring(1))) : queryable.OrderBy(GetSortField(field));
        }

        protected virtual Expression<Func<T, object>> GetSortField(string field)
        {
            switch (field.ToUpper())
            {
                case "CREATEDDATE":
                    return t => t.CreatedDate;

                default:
                    return t => t.UpdatedDate;
            }
        }
    }
}