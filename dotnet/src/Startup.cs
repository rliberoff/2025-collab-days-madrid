/*
 The MIT License (MIT)

Copyright (c) 2020 Microsoft Corporation

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
 */

using System;

using Demo.Data;
using Demo.Services;
using Demo.Utils;

using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;

namespace Demo;

public class Startup(IConfiguration configuration)
{
    public IConfiguration Configuration { get; } = configuration;

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.Configure<CookiePolicyOptions>(options =>
        {
            // This lambda determines whether user consent for non-essential cookies is needed for a given request.
            options.CheckConsentNeeded = context => true;
            options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
            // Handling SameSite cookie according to https://docs.microsoft.com/en-us/aspnet/core/security/samesite?view=aspnetcore-3.1
            options.HandleSameSiteCookieCompatibility();
        });


        services.AddOptions();

        //Add DB support
        services.AddDbContext<DemoDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("AppDBConnStr")));

        // Add Microsoft Graph support
        services.AddScoped<IMSGraphService, MSGraphService>();

        services.AddHttpClient();  //Enable direct HTTP client calls

        AddMicrosoftIdentityAuthenticationService(services);

        services.AddControllersWithViews().AddMicrosoftIdentityUI();

        services.AddRazorPages();

        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });

        //validate anti forgery token by default for all requests
        services.AddMvc(options => { options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()); });
    }

    // This function takes some of the configuration items in appsettings.json and use those to configure MicrosoftIdentityWebApp
    // and EnableTokenAcquisitionToCallDownstreamApi, as well as enabling in memory token cache
    //
    //Authentication basics: https://docs.microsoft.com/en-us/aspnet/core/security/authentication/?view=aspnetcore-6.0
    //
    private void AddMicrosoftIdentityAuthenticationService(IServiceCollection services)
    {
        //To use certificates: https://aka.ms/ms-id-web-certificates and update appsettings.json

        // Sign-in users with the Microsoft identity platform
        services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApp(options =>
                {
                    //Documentation for MicrosoftIdentityOptions
                    //https://learn.microsoft.com/en-us/dotnet/api/microsoft.identity.web.microsoftidentityoptions?view=azure-dotnet
                    Configuration.Bind("AzureAd", options);
                }
                ).EnableTokenAcquisitionToCallDownstreamApi(options =>
                {
                    //Takes _configuration options such as ClientSecret, ClientId.
                    //Documentation for ConfidentialClientApplicationOptions
                    //See https://docs.microsoft.com/en-us/dotnet/api/microsoft.identity.client.confidentialclientapplicationoptions?view=azure-dotnet
                    Configuration.Bind("AzureAd", options);
                },
                GraphScope.InitialPermissions
                )
                //In memory token is suitable for app permissions for development. Consider using a different cache
                //for production: https://docs.microsoft.com/en-us/azure/active-directory/develop/msal-net-token-cache-serialization?tabs=aspnetcore
                .AddInMemoryTokenCaches();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {

        using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
        {
            var context = serviceScope.ServiceProvider.GetRequiredService<DemoDbContext>();            
            context.Database.EnsureCreated();
        }

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

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseCookiePolicy();

        app.UseAuthentication();
        app.UseRouting();
        app.UseAuthorization();

        app.UseSession();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            endpoints.MapRazorPages();
        });
    }
}
