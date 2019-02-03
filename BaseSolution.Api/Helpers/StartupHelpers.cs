using AutoMapper;
using BaseSolution.Api.Mappers;
using BaseSolution.Common.Constants;
using BaseSolution.Core.Commons.Errors;
using BaseSolution.Core.Commons.Objects;
using BaseSolution.Core.Service.Interfaces;
using BaseSolution.Model;
using BaseSolution.Service;
using BaseSolution.Service.Implementations;
using BaseSolution.Service.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using Swashbuckle.AspNetCore.Swagger;

namespace BaseSolution.Api.Helpers
{
    public static class StartupHelpers
    {
        public static IServiceCollection ConfigSwagger(this IServiceCollection service)
        {
            service.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "Application APIs", Version = "1.0.0" });
            });
            return service;
        }

        public static void ConfigLogging(ILoggerFactory loggerFactory)
        {
            LogManager.LoadConfiguration("NLog.config");
            LogManager.Configuration.Variables.Add("connectionString", ConfigurationKeys.DefaultConnection);
            loggerFactory.AddNLog();
        }

        public static IApplicationBuilder UseConfigSwagger(this IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Application API V1");
            });
            return app;
        }

        public static IServiceCollection ConfigIoc(this IServiceCollection services, IConfiguration configuration)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new DtoMappingProfile());
            });
            services.AddSingleton(config.CreateMapper());

            services.Configure<ElasticSearchOptions>(configuration.GetSection(ConfigurationKeys.ElasticSearch));
            services.AddSingleton<IEsClientProvider, EsClientProvider>();
            services.AddScoped<ISearchService, SearchService>();
            services.AddScoped<IEElasticSearchService, EElasticSearchService>();
            services.AddScoped<CoreExceptionFilter>();
            services.AddScoped<DbContext, AppDbContext>();
            return services;
        }
    }
}