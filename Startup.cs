using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using LINQ2DB_MVC_Core_5.Auth.DB;
using LINQ2DB_MVC_Core_5.Auth;
using LinqToDB.Data;
using LINQ2DB_MVC_Core_5.DB;
using LINQ2DB_MVC_Core_5.Models;

namespace LINQ2DB_MVC_Core_5
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
            // Set up Linq2DB connection
            DataConnection.DefaultSettings = new Linq2dbSettings(Configuration);

            // configure app to use Linq2DB for the identity provider: BEGIN
            services.AddScoped<IUserStore<AspNetUsers>, AspNetUsersStore>();
            services.AddScoped<IRoleStore<AspNetRoles>, AspNetRolesStore>();
            services.AddScoped<IUserClaimsPrincipalFactory<AspNetUsers>, AspNetUsersClaimsPrincipalFactory>();
            // Tip: To access you database, inject DataConnection into the constructor, and cast it as type LinqDB in your constructor.
            //  to see this in action - look at DemoController.
            services.AddScoped<DataConnection, LinqDB>();
            services.AddTransient<IdentityRole<string>, AspNetRoles>();
            services.AddTransient<IdentityUserClaim<string>, AspNetUserClaims>();
            services.AddTransient<IdentityUserRole<string>, AspNetUserRoles>();
            services.AddTransient<IdentityUserLogin<string>, AspNetUserLogins>();
            services.AddTransient<IdentityUserToken<string>, AspNetUserTokens>();
            services.AddTransient<IdentityRoleClaim<string>, AspNetRoleClaims>();

            services.AddDefaultIdentity<AspNetUsers>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
            });

            // Notes for SSO: -
            //  1. For Google, FaceBook and Microsoft... ...All the coding has been done for you.
            //  2. you need to set the ID and Secret for each button in appsettings.json
            //  3. You have to add a Nuget package for each external authentication that you want to configure.
            //      to see these packges, use the NuGet Package manager, and search for the ones that start with: -
            //      Microsoft.AspNetCore.Authentication.<<ProviderName>>
            //  4. If you're not sure how to get an ID or a secret - then look at the instructions at
            //      https://docs.microsoft.com/en-us/aspnet/core/security/authentication/social/?view=aspnetcore-3.1&tabs=visual-studio
            //      (select the type of account in the left menu frame, and then look for the part about setting up the app)
            //  5. If you change the order of the code blocks below (put Facebook first for example) - then you will 
            //      change the order of the buttons on the logni screen.
            //
            // Finally - for keeping secrets on deploy...
            //  here's a good article if you don't know anything yet: -
            //  https://medium.com/google-cloud/adding-social-login-to-your-asp-net-core-2-1-google-cloud-platform-application-1baae89f1dc8

            // Set up Google SSO (add the ClientId and the Secret to the "Authentication" section of appsettings.json
            var oGoogleSSO = new SSOConfigModel(Configuration, "GoogleSSO", "ClientId", "Secret");
            if (oGoogleSSO.HasSettings)
            {
                services.AddAuthentication().AddGoogle(options =>
                {
                    options.ClientId = oGoogleSSO.ID;
                    options.ClientSecret = oGoogleSSO.Secret;
                });
            }

            // Set up Microsoft SSO (add the ClientId and the ClientSecret to the "Authentication" section of appsettings.json
            var oMicrosoftSSO = new SSOConfigModel(Configuration, "Microsoft", "ClientId", "ClientSecret");
            if (oMicrosoftSSO.HasSettings)
            {
                services.AddAuthentication().AddMicrosoftAccount(options =>
                {
                    options.ClientId = oMicrosoftSSO.ID;
                    options.ClientSecret = oMicrosoftSSO.Secret;
                });
            }

            // Set up Facebook SSO (add the AppId and the AppSecret to the "Authentication" section of appsettings.json
            var oFacebookSSO = new SSOConfigModel(Configuration, "Facebook", "AppId", "AppSecret");
            if (oFacebookSSO.HasSettings)
            {
                services.AddAuthentication().AddFacebook(options =>
                {
                    options.ClientId = oFacebookSSO.ID;
                    options.ClientSecret = oFacebookSSO.Secret;
                });
            }

            services.AddControllersWithViews();
            services.AddRazorPages();
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
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}
