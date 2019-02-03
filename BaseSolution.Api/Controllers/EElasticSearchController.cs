using AutoMapper;
using BaseSolution.Core.Commons.Extensions;
using BaseSolution.Service.Dtos.EElasticSearch;
using BaseSolution.Service.Implementations;
using BaseSolution.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace BaseSolution.Api.Controllers
{
    public class EElasticSearchController : BaseController
    {
        private readonly IEElasticSearchService _eElasticSearchService;
        private readonly IMapper _mapper;

        public EElasticSearchController(IEElasticSearchService eElasticSearchService, IMapper mapper)
        {
            _eElasticSearchService = eElasticSearchService;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] EElasticSearchCreateDto eElasticSearchCreateDto)
        {
            ValidateModelState();
            var eElasticSearchDto = _mapper.Map<EElasticSearchDto>(eElasticSearchCreateDto);
            eElasticSearchDto.CreatedDate = DateTime.Now;
            eElasticSearchDto.UpdatedDate = DateTime.Now;
            var result = await _eElasticSearchService.Create(eElasticSearchDto);
            return Ok(_mapper.Map<EElasticSearchDto>(result));
        }

        [HttpGet]
        public async Task<IActionResult> Gets(
            [FromQuery] int skip = 0,
            [FromQuery] int take = 10,
            [FromQuery] string[] sorts = null,
            [FromQuery] string query = null)
        {
            var resultDto = await _eElasticSearchService.Reads(skip, take, sorts, query);
            return Ok(_mapper.Map<QueryResult<EElasticSearchDto>>(resultDto));
        }

        [HttpPut]
        public async Task<IActionResult> Update(Guid id, [FromBody] EElasticSearchDto eElasticSearchDto)
        {
            ValidateModelState();
            eElasticSearchDto.Id = id;
            var updatedDto = await _eElasticSearchService.Update(eElasticSearchDto);
            return Ok(updatedDto);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _eElasticSearchService.Delete(id);
            return Ok();
        }
    }
}