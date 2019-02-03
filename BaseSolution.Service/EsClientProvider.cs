using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using BaseSolution.Core.Commons.Enums;
using BaseSolution.Core.Commons.Errors;
using BaseSolution.Core.Commons.Extensions;
using BaseSolution.Core.Commons.Objects;
using BaseSolution.Core.Service.Interfaces;
using BaseSolution.Service.Dtos.EElasticSearch;
using Elasticsearch.Net;
using Microsoft.Extensions.Options;
using Nest;
using Nest.JsonNetSerializer;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BaseSolution.Service
{
    public class EsClientProvider : IEsClientProvider
    {
        public EsClientProvider(IOptions<ElasticSearchOptions> options)
        {
            var pool = new SingleNodeConnectionPool(new Uri(options.Value.Uri));
            var connectionSettings = new ConnectionSettings(pool, sourceSerializer: (builtin, settings) => new JsonNetSerializer(builtin, settings, () => new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Include,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc
            }, resolver => resolver.NamingStrategy = new CamelCaseNamingStrategy()
            ));
            // .DefaultMappingFor<AccountDto>(m => m.IndexName(PrefixIndex + "-" + nameof(AccountDto).ToLower()))
            // .DefaultMappingFor<CategoryDto>(m => m.IndexName(PrefixIndex + "-" + nameof(CategoryDto).ToLower()))
            // .DefaultIndex(PrefixIndex);
            if (!string.IsNullOrEmpty(options.Value.UserName) && !string.IsNullOrEmpty(options.Value.Password))
            {
                connectionSettings.BasicAuthentication(options.Value.UserName, options.Value.Password);
            }

            connectionSettings.DisableDirectStreaming();
            Client = new ElasticClient(connectionSettings);

            PrefixIndex = options.Value.PrefixIndex;
            CreateIndex().Wait();
        }

        public IElasticClient Client { get; }

        public string PrefixIndex { get; }

        public async Task<IEnumerable<T>> SearchAsync<T>(string searchQuery, string sortByField = "", SortDirection sortDirection = SortDirection.Ascending, params Expression<Func<T, object>>[] searchFields)
            where T : class
        {
            var res = await SearchAsync(null, null, sortByField, sortDirection, null, searchQuery, searchFields);
            return res.Documents;
        }

        public async Task<ISearchResponse<T>> SearchAsync<T>(int? skip = null, int? take = null, string sortByField = "", SortDirection sortDirection = SortDirection.Ascending, IEnumerable<KeyValuePair<Expression<Func<T, object>>, object>> terms = null, string searchQuery = null, params Expression<Func<T, object>>[] searchFields)
            where T : class
        {
            var res = await Client.SearchAsync<T>(x => BuildSearchDescriptor(x, skip, take, sortByField, sortDirection, terms, searchQuery, searchFields).Index(PrefixIndex + "-" + typeof(T).Name.ToLower()));

            if (!res.IsValid) throw new CoreException(res.DebugInformation);
#if DEBUG
            var query = Encoding.UTF8.GetString(res.ApiCall.RequestBodyInBytes);
            Debug.WriteLine(query);
#endif
            return res;
        }

        public async Task<QueryResult<T>> QueryAsync<T>(int skip, int take, string sortByField = "", SortDirection sortDirection = SortDirection.Ascending, IEnumerable<KeyValuePair<Expression<Func<T, object>>, object>> terms = null, string searchQuery = null, params Expression<Func<T, object>>[] searchFields)
            where T : class
        {
            var res = await SearchAsync(skip, take, sortByField, sortDirection, terms, searchQuery, searchFields);
            return new QueryResult<T>
            {
                Count = res.Total,
                Items = res.Documents
            };
        }

        public async Task IndexAsync<T>(T document)
            where T : class
        {
            var res = await Client.IndexAsync(document, idx => idx.Index(PrefixIndex + "-" + typeof(T).Name.ToLower()));
            if (!res.IsValid)
            {
                throw new CoreException(res.DebugInformation);
            }
            await Client.RefreshAsync(Indices.All);
        }

        public async Task IndexManyAsync<T>(IEnumerable<T> documents)
            where T : class
        {
            var enumerable = documents as T[] ?? documents.ToArray();
            if (enumerable.IsNullOrEmpty()) return;
            var indexName = PrefixIndex + "-" + typeof(T).Name.ToLower();
            var response = await Client.IndexManyAsync(enumerable, indexName);
            await Client.RefreshAsync(Indices.All);
        }

        public async Task DeleteAsync<T>(Guid id)
            where T : class
        {
            await Client.DeleteAsync(DocumentPath<T>.Id(id), idx => idx.Index(PrefixIndex + "-" + typeof(T).Name.ToLower()));
            await Client.RefreshAsync(Indices.All);
        }

        public async Task DeleteAsync<T>(string id)
            where T : class
        {
            await Client.DeleteAsync(DocumentPath<T>.Id(id), idx => idx.Index(PrefixIndex + "-" + typeof(T).Name.ToLower()));
            await Client.RefreshAsync(Indices.All);
        }

        public async Task UpdateAsync<T>(T document) where T : class, IDto
        {
            await Client.UpdateAsync(DocumentPath<T>.Id(document.Id), u => u.DocAsUpsert().Doc(document).Index(PrefixIndex + "-" + typeof(T).Name.ToLower()));
            await Client.RefreshAsync(Indices.All);
        }

        public async Task<T> GetAsync<T>(Guid id) where T : class
        {
            var query = await Client.GetAsync(DocumentPath<T>.Id(id), idx => idx.Index(PrefixIndex + "-" + typeof(T).Name.ToLower()));
            return query?.Source;
        }

        public async Task<T> GetAsync<T>(string id) where T : class
        {
            var query = await Client.GetAsync(DocumentPath<T>.Id(id), idx => idx.Index(PrefixIndex + "-" + typeof(T).Name.ToLower()));
            return query.Source;
        }

        public async Task CreateOrUpdateIndexAsync<T>(T entity)
            where T : class, IDto
        {
            if (await GetAsync<T>(entity.Id) == null)
            {
                await IndexAsync(entity);
            }
            else
            {
                await UpdateAsync(entity);
            }
            await Client.RefreshAsync(Indices.All);
        }

        public async Task CreateIndex()
        {
            await CreateEElasticSearchIndex();
             
            await Client.RefreshAsync(Indices.All);
        }

        public async Task DeleteIndex()
        {
            var result = await Client.CatIndicesAsync();
            var allIndex = result.Records;
            foreach (var clusterIndicesStat in allIndex)
            {
                if (clusterIndicesStat.Index.IndexOf(PrefixIndex, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    await Client.DeleteIndexAsync(clusterIndicesStat.Index);
                }
            }


            await Client.RefreshAsync(Indices.All);
        }

        #region none-public method

        private static SearchDescriptor<T> BuildSearchDescriptor<T>(
            SearchDescriptor<T> selector = null,
            int? skip = null,
            int? take = null,
            string sortByField = "",
            SortDirection sortDirection = SortDirection.Ascending,
            IEnumerable<KeyValuePair<Expression<Func<T, object>>, object>> terms = null,
            string searchQuery = null,
            params Expression<Func<T, object>>[] searchFields
        ) where T : class
        {
            var sortOrder = sortDirection == SortDirection.Ascending ? SortOrder.Ascending : SortOrder.Descending;
            var sortField = string.IsNullOrWhiteSpace(sortByField) ? "updatedDate" : sortByField;
            var x = selector ?? new SearchDescriptor<T>();
            x = (skip != null && take != null ? x.From(skip.Value).Size(take.Value) : x)
                .Sort(sort => sort.Field(f => f.Field(sortField).Order(sortOrder).UnmappedType(FieldType.Boolean)));
            if (!string.IsNullOrWhiteSpace(searchQuery))
                x = x.Query(q => q.SimpleQueryString(qs => qs.Fields(searchFields).Query(searchQuery).DefaultOperator(Operator.And)));
            if (terms != null)
                x = x.Query(q =>
                    BuildTermQueryContainer(q, terms) &&
                    q.SimpleQueryString(qs => qs.Fields(searchFields).Query(searchQuery).DefaultOperator(Operator.And))
                 );
            return x;
        }

        private static QueryContainer BuildTermQueryContainer<T>(QueryContainerDescriptor<T> q, IEnumerable<KeyValuePair<Expression<Func<T, object>>, object>> keywords)
            where T : class
        {
            var container = default(QueryContainer);
            foreach (var keyword in keywords)
            {
                if (keyword.Value is IEnumerable && !(keyword.Value is string))
                {
                    container = container && q.Terms(x => x.Field(keyword.Key).Terms(((IEnumerable)keyword.Value).AsQueryable().Cast<object>().ToArray()));
                }
                else
                {
                    container = container && q.Term(keyword.Key, keyword.Value);
                }
            }
            return container;
        }

        #endregion

        private async Task CreateEElasticSearchIndex()
        {
            string eElasticSearchIndexName = PrefixIndex + "-" + nameof(EElasticSearchDto).ToLower();
            if (!(await Client.IndexExistsAsync(eElasticSearchIndexName)).Exists)
            {
                var descriptor = new CreateIndexDescriptor(eElasticSearchIndexName)
                    .Settings(s => s
                        .Analysis(a => a
                            .Analyzers(an => an
                                .Custom("vi", ca => ca
                                    .CharFilters("html_strip")
                                    .Tokenizer("icu_tokenizer")
                                    .Filters("lowercase", "icu_folding")
                                )
                            )
                        )
                    )
                    .Mappings(mapper => mapper
                        .Map<EElasticSearchDto>(m => m
                            .Properties(p => p
                                .Text(t => t.Name(n => n.Title).Analyzer("vi"))
                            ))
                    );
                var response = await Client.CreateIndexAsync(descriptor);
                if (!response.Acknowledged)
                {
                    Console.WriteLine($@"Cannot create {eElasticSearchIndexName} Index");
                }
            }
        }
    }
}