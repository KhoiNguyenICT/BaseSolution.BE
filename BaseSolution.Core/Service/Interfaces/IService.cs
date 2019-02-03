using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using BaseSolution.Core.Commons.Extensions;
using BaseSolution.Core.Models.Interfaces;

namespace BaseSolution.Core.Service.Interfaces
{
    public interface IService<T, TDto> : IIndexService<TDto, Guid>
        where T : IEntity
        where TDto : class, IDto, new()
    {
        Task<QueryResult<TDto>> QueryByElasticSearch(int skip, int take, string[] sorts, IEnumerable<KeyValuePair<Expression<Func<TDto, object>>, object>> terms, string searchQuery, params Expression<Func<TDto, object>>[] searchFields);

        Task<QueryResult<TDto>> Query(int skip, int take, string[] sorts, string searchQuery, params Expression<Func<T, bool>>[] predicates);

        Task<TDto> Create(TDto entityDto, bool index = true);

        Task<TDto> Get(Guid id);

        Task UpdateIndex(Guid id, params Expression<Func<T, object>>[] includes);

        Task<TDto> Update(TDto entityDto, bool index = true);

        Task Delete(Guid id, bool index = true);

        QueryResult<T> ToQueryResult(IList<object> source, long count);
    }
}