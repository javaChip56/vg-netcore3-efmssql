using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using HealthChecks.UI.Client;

using ClientAPI.Domain;
using ClientAPI.Domain.Queries;
using ClientAPI.Domain.Queries.Interfaces;
namespace ClientAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddHsts(options =>
            {
                options.Preload = true;
                options.IncludeSubDomains = true;
                options.MaxAge = TimeSpan.FromDays(1);
                options.ExcludedHosts.Add("bumusg.com");
                options.ExcludedHosts.Add("www.bumusg.com");
            });
            
            services.AddEntityFrameworkSqlServer()
                .AddDbContext<ClientContext>(options =>
                {
                    options.UseSqlServer(Configuration["ConnectionStrings:ClientDB"],
                        sqlServerOptionsAction: sqlOptions =>
                        {
                            sqlOptions.EnableRetryOnFailure(maxRetryCount: 10, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                        });
                },
                ServiceLifetime.Scoped  //Showing explicitly that the DbContext is shared across the HTTP request scope (graph of objects started in the HTTP request)
            );

            services.AddTransient<IClientQueries>(sp => {
                var context = sp.GetRequiredService<ClientContext>();
                return new ClientQueries(context);
            });
            
            // new ClientQueries(Configuration["ConnectionStrings:ClientDB"]));

            services.AddSwaggerGen(c =>
            {
                 c.SwaggerDoc(
                    Configuration["SwaggerConfig:FriendlyName"] + Configuration["SwaggerConfig:Version"],
                        new OpenApiInfo
                        {
                            Title = Configuration["SwaggerConfig:Title"],
                            Version = Configuration["SwaggerConfig:Version"]
                        }
                    );
            });

            services.AddHealthChecks()
                .AddCheck("Client Database", new Diagnostics.HealthChecks.DatabaseConnectionHealthCheck(Configuration["ConnectionStrings:ClientDB"]));

            services.AddHttpsRedirection(options =>
            {
                options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
                options.HttpsPort = 443;
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint(
                    string.Format(
                        Configuration["SwaggerConfig:Endpoint"],
                        Configuration["SwaggerConfig:FriendlyName"] + Configuration["SwaggerConfig:Version"]), 
                        Configuration["SwaggerConfig:Title"] + " " + Configuration["SwaggerConfig:Version"]);
            });

            app.UseHealthChecks(path: "/hc", new HealthCheckOptions() {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
            
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

        }
    }
}
