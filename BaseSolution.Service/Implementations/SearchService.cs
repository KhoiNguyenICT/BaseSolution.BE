using System.Threading.Tasks;
using BaseSolution.Core.Service.Interfaces;
using BaseSolution.Service.Interfaces;

namespace BaseSolution.Service.Implementations
{
    public class SearchService : ISearchService
    {
        private readonly IEsClientProvider _esClient;
        private readonly IEElasticSearchService _eElasticSearchService;

        public SearchService(IEsClientProvider esClient, 
            IEElasticSearchService eElasticSearchService)
        {
            _esClient = esClient;
            _eElasticSearchService = eElasticSearchService;
        }

        public async Task ReIndex()
        {
            // delete index
            await _esClient.DeleteIndex();
            // re-create index
            await _esClient.CreateIndex();
            // index data
            await _eElasticSearchService.IndexAllItems();
        }
    }
}