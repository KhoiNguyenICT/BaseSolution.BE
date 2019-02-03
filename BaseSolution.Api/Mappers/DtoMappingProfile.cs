using AutoMapper;
using BaseSolution.Service.Dtos.EElasticSearch;

namespace BaseSolution.Api.Mappers
{
    public class DtoMappingProfile: Profile
    {
        public DtoMappingProfile()
        {
            CreateMap<EElasticSearchCreateDto, EElasticSearchDto>();
        }
    }
}