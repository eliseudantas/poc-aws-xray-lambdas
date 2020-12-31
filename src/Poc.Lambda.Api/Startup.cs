using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Core.Sampling.Local;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Poc.Lambda.Api
{


    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            var recorder = new AWSXRayRecorderBuilder().WithSamplingStrategy(new LocalizedSamplingStrategy("sampling-rules.json")).Build();

            
            AWSXRayRecorder.InitializeInstance(configuration, recorder);

            AWSSDKHandler.RegisterXRayForAllServices();
        }

        public static IConfiguration Configuration { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            app.UseExceptionHandler("/Error");
            app.UseXRay("Entry Point API");
            app.UseStaticFiles();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            //app.UseAuthorization();
           
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                //endpoints.MapGet("/", async context =>
                //{
                //    await context.Response.WriteAsync("Welcome to running ASP.NET Core on AWS Lambda");
                //});
            });
        }
    }
}
