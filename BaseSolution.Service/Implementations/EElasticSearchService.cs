using AutoMapper;
using BaseSolution.Core.Commons.Errors;
using BaseSolution.Core.Commons.Extensions;
using BaseSolution.Core.Service.Interfaces;
using BaseSolution.Model;
using BaseSolution.Model.Entities;
using BaseSolution.Service.Dtos;
using BaseSolution.Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseSolution.Service.Dtos.EElasticSearch;

namespace BaseSolution.Service.Implementations
{
    public class EElasticSearchService : BaseService<EElasticSearch, EElasticSearchDto>, IEElasticSearchService
    {
        public EElasticSearchService(AppDbContext dbContext, IMapper mapper, IEsClientProvider esClient) : base(dbContext, mapper, esClient)
        {
        }

        public async Task<EElasticSearchDto> Create(EElasticSearchDto elasticSearchDto)
        {
            elasticSearchDto.CreatedDate = DateTime.Now;
            elasticSearchDto.UpdatedDate = DateTime.Now;
            var elasticSearch = _mapper.Map<EElasticSearch>(elasticSearchDto);
            var validationErrors = await ValidateCreate(elasticSearch);
            if (validationErrors?.Count > 0)
            {
                throw new CoreException(validationErrors);
            }
            _dbSet.AddObject(elasticSearch);
            await _dbContext.SaveChangesAsync();
            var dto = _mapper.Map<EElasticSearchDto>(elasticSearch);
            await _esClient.IndexAsync(dto);
            return dto;
        }

        public async Task<QueryResult<EElasticSearchDto>> Reads(int skip, int take, string[] sorts, string searchQuery)
        {
            var queryable = _dbContext.EElasticSearches.AsNoTracking();
            queryable = sorts?.All(x => !string.IsNullOrEmpty(x)) == true ? sorts.Aggregate(queryable, OrderBy) : queryable.OrderByDescending(x => x.UpdatedDate);

            return new QueryResult<EElasticSearchDto>
            {
                Count = await queryable.CountAsync(),
                Items = _mapper.Map<IList<EElasticSearchDto>>(queryable.Skip(skip).Take(take))
            };
        }

        public async Task<EElasticSearchDto> Read(Guid id)
        {
            return await Get(id);
        }

        public async Task<EElasticSearchDto> Update(EElasticSearchDto elasticSearchDto)
        {
            var entity = await _dbSet.GetOne(x => x.Id == elasticSearchDto.Id);
            if (entity == null)
            {
                throw new CoreException("EElasticSearchNotFound");
            }

            _mapper.Map(elasticSearchDto, entity);
            entity.UpdatedDate = DateTime.Now;
            await _dbContext.SaveChangesAsync();
            var resultDto = _mapper.Map<EElasticSearchDto>(entity);
            await _esClient.UpdateAsync(resultDto);
            return resultDto;
        }

        public async Task Delete(Guid id)
        {
            var validationErrors = await ValidateDelete(id);
            if (validationErrors?.Count > 0)
            {
                throw new CoreException(validationErrors);
            }

            var elasticSearch = await _dbSet.SingleOrDefaultAsync(x => x.Id == id);
            if (elasticSearch == null)
            {
                throw new CoreException("EElasticNotFound");
            }
            _dbSet.Delete(elasticSearch);
            await _esClient.DeleteAsync<EElasticSearchDto>(id);
        }
    }
}