using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System;


namespace JaegerOpenTelemetryDotnetExample.ServiceA
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            var defaultResource = ResourceBuilder.CreateDefault().AddService("ServiceB");

            // This must be set before creating a GrpcChannel/HttpClient when calling an insecure service
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            services.AddLogging(builder =>
            {
                builder.AddOpenTelemetry(options => {
                    options.SetResourceBuilder(defaultResource);
                    options.AddOtlpExporter(o =>
                    {
                        o.Endpoint = new Uri("http://otel-collector:4317");
                        o.ExportProcessorType = ExportProcessorType.Simple;
                    });
                });
                builder.AddConsole();
                builder.AddDebug();
            });
            services.AddOpenTelemetry()
                .ConfigureResource(builder => builder.AddService(serviceName: "MyService"))
                .WithTracing(builder => builder
                    .SetResourceBuilder(defaultResource)
                    .AddSource("ExampleTracer")
                    .AddAspNetCoreInstrumentation()
                    .AddConsoleExporter()
                    .AddOtlpExporter(o =>
                    {
                        o.Endpoint = new Uri("http://otel-collector:4317");
                        o.ExportProcessorType = ExportProcessorType.Simple;
                    }));
                    
        }


        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
