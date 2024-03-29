using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using System.IO;
using Taboret.Data;
using Taboret.Models;

namespace Taboret
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
            services.AddLocalization(options =>
            {
                options.ResourcesPath = "Resources";
            });

            services.Configure<RequestLocalizationOptions>(options =>
            {
                options.AddSupportedUICultures("en", "pl");
                options.SetDefaultCulture(Configuration["Taboret:Culture"]);
                options.FallBackToParentUICultures = true;
            });

            services.AddAuthentication()
                .AddDiscord(options =>
                {
                    options.ClientId = Configuration["Discord:ClientId"];
                    options.ClientSecret = Configuration["Discord:ClientSecret"];
                    options.Scope.Add("guilds");
                    options.SaveTokens = true;
                });

            services.AddDbContext<ArchiveContext>(options =>
            {
                options.UseSqlite(
                    Configuration["ConnectionStrings:ArchiveContext"],
                    o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
            });

            services.AddDatabaseDeveloperPageExceptionFilter();
            services.AddRazorPages()
                .AddViewLocalization()
                .AddDataAnnotationsLocalization(options =>
                {
                    options.DataAnnotationLocalizerProvider = (type, factory) =>
                        factory.Create(typeof(Shared));
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRequestLocalization();

            app.UseStaticFiles();
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(env.ContentRootPath, "Assets")),
                RequestPath = "/Assets"
            });

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("default", "{controller=Issues}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}
