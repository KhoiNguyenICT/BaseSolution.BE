using System;
using System.Threading.Tasks;
using BaseSolution.Core.Commons.Extensions;
using BaseSolution.Core.Service.Interfaces;
using BaseSolution.Model.Entities;
using BaseSolution.Service.Dtos;
using BaseSolution.Service.Dtos.EElasticSearch;

namespace BaseSolution.Service.Interfaces
{
    public interface IEElasticSearchService: IIndexService<EElasticSearchDto, Guid>
    {
        Task<EElasticSearchDto> Create(EElasticSearchDto elasticSearchDto);
        Task<QueryResult<EElasticSearchDto>> Reads(int skip, int take, string[] sorts, string searchQuery);
        Task<EElasticSearchDto> Read(Guid id);
        Task<EElasticSearchDto> Update(EElasticSearchDto elasticSearchDto);
        Task Delete(Guid id);
    }
}