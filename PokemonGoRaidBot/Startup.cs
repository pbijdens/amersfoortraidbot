using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using RaidBot.Backend;
using RaidBot.Backend.Bot;
using RaidBot.Backend.DB;
using System;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace RaidBot
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


            services.AddMvc();

            //            services.AddEntityFrameworkSqlServer();
            //services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            var connectionString = Configuration.GetConnectionString("DefaultConnection");
            services.AddEntityFrameworkNpgsql().AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));

            // Configure the IoC container, add custom services
            PokemonRaidBotHost raidBotHost = new PokemonRaidBotHost(Configuration.GetSection("RaidBot").Get<RaidBotSettings>());
            raidBotHost.ServiceCollection = services;
            services.Add(new ServiceDescriptor(typeof(IPokemonRaidBotHost), (sp) => { return raidBotHost; }, ServiceLifetime.Singleton));

            Task.Run(() =>
            {
                Task.Delay(TimeSpan.FromSeconds(5)).ContinueWith((x) =>
                {
                    raidBotHost.Start();
                });
            });

            // Add ASP.NET Identity support
            services.AddIdentity<ApplicationUser, IdentityRole>(
                opts =>
                {
                    opts.Password.RequireDigit = true;
                    opts.Password.RequireLowercase = true;
                    opts.Password.RequireUppercase = true;
                    opts.Password.RequireNonAlphanumeric = false;
                    opts.Password.RequiredLength = 7;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddAuthentication(opts =>
            {
                opts.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(cfg =>
             {
                 cfg.RequireHttpsMetadata = false;
                 cfg.SaveToken = true;
                 cfg.TokenValidationParameters = new TokenValidationParameters()
                 {
                     // standard configuration
                     ValidIssuer = Configuration["Auth:Jwt:Issuer"],
                     IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Auth:Jwt:Key"])),
                     ValidAudience = Configuration["Auth:Jwt:Audience"],
                     ClockSkew = TimeSpan.Zero,

                     // security switches
                     RequireExpirationTime = true,
                     ValidateIssuer = true,
                     ValidateIssuerSigningKey = true,
                     ValidateAudience = true,


                 };
                 cfg.IncludeErrorDetails = true;
             });

            // Set up the claims for each of the policies
            services.AddAuthorization(options =>
            {
                options.AddPolicy(SecurityPolicy.IsAdministrator, policy => policy.RequireClaim(ClaimTypes.Role, SecurityPolicy.RoleAdministrator));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.MapWhen(context =>
            {
                // ALL files are dynamic, and all requests are routed to the API, except
                // for the paths mentioned specifically here.
                var path = context.Request.Path.Value;
                return path.StartsWith("/dist", StringComparison.OrdinalIgnoreCase)
                           || path.StartsWith("/_docs", StringComparison.OrdinalIgnoreCase)
                           //|| path.StartsWith("/app", StringComparison.OrdinalIgnoreCase)
                           ;
            }, config => config.UseStaticFiles(new StaticFileOptions()
            {
                OnPrepareResponse = (context) =>
                {
                    // Disable caching for all static files. 
                    context.Context.Response.Headers["Cache-Control"] = Configuration["StaticFiles:Headers:Cache-Control"];
                    context.Context.Response.Headers["Pragma"] = Configuration["StaticFiles:Headers:Pragma"];
                    context.Context.Response.Headers["Expires"] = Configuration["StaticFiles:Headers:Expires"];
                }
            }));
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            // Add the AuthenticationMiddleware to the pipeline
            app.UseAuthentication();

            // We'd very much appreciate it when we would not be spammed senselessly by application insights while debugging
            Microsoft.ApplicationInsights.Extensibility.Implementation.TelemetryDebugWriter.IsTracingDisabled = true;

            app.UseMvc();

            app.UseMapster();
        }
    }
}
