using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

namespace Dela.Sample.ApiGateway
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
            //设置跨域请求
            services.AddCors(
                option => option.AddPolicy("cors",
                    policy => policy.AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()
                    .AllowAnyOrigin()
                )
            );

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            Action<IdentityServerAuthenticationOptions> isaOptClient = option =>
            {
                option.Authority = "http://47.99.36.29:8086";// Configuration["IdentityService:Uri"];
                option.ApiName = "clientservice";
                option.RequireHttpsMetadata = false;// Convert.ToBoolean(Configuration["IdentityService:UseHttps"]);
                option.SupportedTokens = SupportedTokens.Both;
                option.ApiSecret = "clientsecret";// Configuration["IdentityService:ApiSecrets:clientservice"];
            };
            Action<IdentityServerAuthenticationOptions> isaOptProduct = option =>
            {
                option.Authority = "http://47.99.36.29:8086";// Configuration["IdentityService:Uri"];
                option.ApiName = "productservice";
                option.RequireHttpsMetadata = false;// Convert.ToBoolean(Configuration["IdentityService:UseHttps"]);
                option.SupportedTokens = SupportedTokens.Both;
                option.ApiSecret = "productsecret";// Configuration["IdentityService:ApiSecrets:clientservice"];
            };

            services.AddAuthentication()
            .AddIdentityServerAuthentication("ClientServiceKey", isaOptClient)
            .AddIdentityServerAuthentication("ProductServiceKey", isaOptProduct);

            //services.AddMvc();
            services.AddOcelot(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            //cors跨域
            app.UseCors("cors");

            app.UseMvc();

            app.UseOcelot().Wait();
        }
    }
}
