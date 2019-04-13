using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dela.Sample.ApiClient
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

            // IdentityServer
            services.AddMvcCore().AddAuthorization().AddJsonFormatters();
            services.AddAuthentication("Bearer")
                .AddIdentityServerAuthentication(options =>
                {
                    options.RequireHttpsMetadata = false; // for dev env
                    options.Authority = $"http://47.99.36.29:8086";
                    options.ApiName = "clientservice"; // match with configuration in IdentityServer
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            // authentication
            app.UseAuthentication();
            //cors跨域
            app.UseCors("cors");



            app.UseMvc();
        }
    }
}
