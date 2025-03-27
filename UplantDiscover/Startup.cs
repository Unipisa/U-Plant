using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UplantDiscover.Models.DB;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using UplantDiscover.Models;
using Microsoft.Extensions.FileProviders;
using System.IO;
using Microsoft.AspNetCore.StaticFiles;
using System.Reflection;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using System.Text.Json.Serialization;

// Progetto UplantDiscover ,rappresentazione dei dati inseriti su UPlant per la fruibilità pubblica ideato e scritto da Pietro Picconi pietro.picconi@unipi.it


namespace UplantDiscover
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
            services.AddControllersWithViews();
            services.AddControllers()
                .AddJsonOptions(options =>
                 {
                     options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;///sostituito options.JsonSerializerOptions.IgnoreNullValues = true; perchè obsoleto
                     options.JsonSerializerOptions.WriteIndented = true;
                });



            services.AddDbContext<Entities>(o => {
                o.UseSqlServer(Configuration.GetConnectionString("UPlant"));
            });

            services.Configure<IConfiguration>(Configuration);

            //path per le immagini
            services.Configure<Images>(Configuration.GetSection("Images"));
            //path per la credenziali per creare token su wso2
            services.Configure<Wso2>(Configuration.GetSection("Wso2"));
            services.Configure<GoogleMap>(Configuration.GetSection("GoogleMap"));
            //codice per multilingua inizio
            services.AddSingleton<LanguageService>();

            services.AddLocalization(options => options.ResourcesPath = "Resources");

            services.AddMvc()
                .AddViewLocalization()
                .AddDataAnnotationsLocalization(options =>
                {
                    options.DataAnnotationLocalizerProvider = (type, factory) =>
                    {

                        var assemblyName = new AssemblyName(typeof(ShareResource).GetTypeInfo().Assembly.FullName);

                        return factory.Create("ShareResource", assemblyName.Name);

                    };

                });



            services.Configure<RequestLocalizationOptions>(
                options =>
                {
                    var supportedCultures = new List<CultureInfo>
                        {
                            new CultureInfo("en-US"),
                            new CultureInfo("it-IT"),
                            
                        };



                    options.DefaultRequestCulture = new RequestCulture(culture: "it-IT", uiCulture: "it-IT");

                    options.SupportedCultures = supportedCultures;
                    options.SupportedUICultures = supportedCultures;
                    options.RequestCultureProviders.Insert(0, new QueryStringRequestCultureProvider());

                });
            //codice per multilingua fine



        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            var locOptions = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
            var tipodefault = locOptions.Value.DefaultRequestCulture.Culture;
            app.UseRequestLocalization(locOptions.Value);
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            

            app.UseStaticFiles(new StaticFileOptions    {             


                FileProvider = new PhysicalFileProvider(

                Configuration.GetSection("Images").GetSection("Percorsofisico").Value),
                RequestPath = "/Immagini"
            });
            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                 endpoints.MapControllerRoute(
                  name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapControllers();

               
            });
        }

        private async Task JsonResponseWriter(HttpContext context, HealthReport report)
        {
            context.Response.ContentType = "application/json";
            await JsonSerializer.SerializeAsync(context.Response.Body, new { Status = report.Status.ToString() },
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }




    }
}
