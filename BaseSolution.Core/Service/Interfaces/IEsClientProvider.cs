using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using BaseSolution.Core.Commons.Enums;
using BaseSolution.Core.Commons.Extensions;
using Nest;

namespace BaseSolution.Core.Service.Interfaces
{
    public interface IEsClientProvider
    {
        IElasticClient Client { get; }

        string PrefixIndex { get; }

        Task CreateIndex();

        Task DeleteIndex();

        Task<IEnumerable<T>> SearchAsync<T>(string term, string sortByField = "", SortDirection sortDirection = SortDirection.Ascending, params Expression<Func<T, object>>[] searchFields) where T : class;

        Task<ISearchResponse<T>> SearchAsync<T>(int? skip = null, int? take = null, string sortByField = "", SortDirection sortDirection = SortDirection.Ascending, IEnumerable<KeyValuePair<Expression<Func<T, object>>, object>> terms = null, string searchQuery = null, params Expression<Func<T, object>>[] searchFields) where T : class;

        Task<QueryResult<T>> QueryAsync<T>(int skip, int take, string sortByField = "", SortDirection sortDirection = SortDirection.Ascending, IEnumerable<KeyValuePair<Expression<Func<T, object>>, object>> terms = null, string searchQuery = null, params Expression<Func<T, object>>[] searchFields) where T : class;

        Task IndexAsync<T>(T document) where T : class;

        Task IndexManyAsync<T>(IEnumerable<T> documents) where T : class;

        Task DeleteAsync<T>(Guid id) where T : class;

        Task DeleteAsync<T>(string id) where T : class;

        Task UpdateAsync<T>(T document) where T : class, IDto;

        Task<T> GetAsync<T>(Guid id) where T : class;

        Task<T> GetAsync<T>(string id) where T : class;

        Task CreateOrUpdateIndexAsync<T>(T entity) where T : class, IDto;
    }
}