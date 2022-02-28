using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System;
using System.Reflection;
using Microsoft.Extensions.Caching.Distributed;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Azure;
using Plan.Identity;
using Plan.Core.Models;
using Microsoft.OpenApi.Models;
using Quartz;
using Quartz.Impl;
using Plan.Services;
using StackExchange.Redis;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using MongoDB.Driver;
using RedLockNet;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.IO;

namespace Plan.Web
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

            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });
            services.AddApplicationInsightsTelemetry(Configuration["APPINSIGHTSCONNECTIONSTRING"]);
            services.AddSignalR()
                .AddAzureSignalR(opt =>
                {
                    opt.ConnectionString = Configuration["ConnectionStrings:Signalr"];
                });
            services.AddDbContext<PlanDbContext>((x, y) =>
            {
                var config = x.GetRequiredService<IConfiguration>();
#if DBSELECT_MSSQL
                y.UseSqlServer(config.GetDbConnect());
#elif DBSELECT_SQLITE
                y.UseSqlite(config.GetDbConnect());
#endif
            }, optionsLifetime: ServiceLifetime.Singleton)
            .AddDbContextPool<PlanDbContext>((x, y) =>
            {
                var config = x.GetRequiredService<IConfiguration>();
#if DBSELECT_MSSQL
                y.UseSqlServer(config.GetDbConnect());
#elif DBSELECT_SQLITE
                y.UseSqlite(config.GetDbConnect());
#endif
            }).AddIdentity<PlanUser, PlanRole>(x =>
            {
                x.Password.RequireDigit = false;
                x.Password.RequiredUniqueChars = 0;
                x.Password.RequireLowercase = false;
                x.Password.RequireNonAlphanumeric = false;
                x.Password.RequireUppercase = false;
            })
            .AddEntityFrameworkStores<PlanDbContext>();
            services.AddSingleton<IDistributedCache, RedisCache>();
            services.AddSingleton<IDistributedLockFactory>(provider =>
            {
                var config = provider.GetRequiredService<IConfiguration>();
                var conn = provider.GetRequiredService<IConnectionMultiplexer>();
                return RedLockFactory.Create(new[]
                {
                    new RedLockMultiplexer(conn)
                });
            });
            services.AddOptions<RedisCacheOptions>()
                .Configure(x => x.Configuration = Configuration.GetRedisConnect());
            services.AddSingleton<IConnectionMultiplexer>(provider =>
            {
                var config = provider.GetRequiredService<IConfiguration>();
                return ConnectionMultiplexer.Connect(Configuration.GetRedisConnect());
            });
            services.AddScoped(p => p.GetRequiredService<IConnectionMultiplexer>().GetDatabase());
            services.AddResponseCompression();
            services.AddScoped<UserService>();
            services.AddScoped<UserIdentityService>();
            services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition(PlanAuthenticationHandler.APPKeyHeader, new OpenApiSecurityScheme
                {
                    Name= PlanAuthenticationHandler.APPKeyHeader,
                    In = ParameterLocation.Header,
                    Type= SecuritySchemeType.Http,
                    Scheme = PlanAuthenticationHandler.APPKeyHeader
                });
                c.AddSecurityDefinition(PlanAuthenticationHandler.AccessHeader, new OpenApiSecurityScheme
                {
                    Name = PlanAuthenticationHandler.AccessHeader,
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme= PlanAuthenticationHandler.AccessHeader
                });
                c.AddSecurityDefinition(PlanAuthenticationHandler.AuthHeader, new OpenApiSecurityScheme
                {
                    Name = PlanAuthenticationHandler.AuthHeader,
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = PlanAuthenticationHandler.AuthHeader
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference=new OpenApiReference
                            {
                                Type= ReferenceType.SecurityScheme,
                                Id=PlanAuthenticationHandler.AccessHeader
                            },
                            Scheme= PlanAuthenticationHandler.AccessHeader,
                            In= ParameterLocation.Header
                        },Array.Empty<string>()
                    },
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference=new OpenApiReference
                            {
                                Type= ReferenceType.SecurityScheme,
                                Id=PlanAuthenticationHandler.APPKeyHeader
                            },
                            Scheme= PlanAuthenticationHandler.APPKeyHeader,
                            In= ParameterLocation.Header
                        },Array.Empty<string>()
                    },
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference=new OpenApiReference
                            {
                                Type= ReferenceType.SecurityScheme,
                                Id=PlanAuthenticationHandler.AuthHeader
                            },
                            Scheme= PlanAuthenticationHandler.AuthHeader,
                            In= ParameterLocation.Header
                        },Array.Empty<string>()
                    }
                });
                c.SwaggerDoc("Anf", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Anf API"
                });

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
            services.AddMemoryCache();
            services.AddAuthorization();
            services.AddAuthentication(options =>
            {
                options.AddScheme<PlanAuthenticationHandler>(PlanAuthenticationHandler.SchemeName, "default scheme");
                options.AddScheme<PlanAuthenticationHandler>(PlanAuthenticationHandler.SkipAuthSchemeName, "skip auth scheme");
                //options.DefaultChallengeScheme = PlanAuthenticationHandler.SchemeName;
            });

            services.AddScoped<PlanAuthenticationHandler>();

            AddQuartzAsync(services);
            services.AddAzureClients(builder =>
            {
                builder.AddBlobServiceClient(Configuration["Azure:Store:Blob:blob"], preferMsi: true);
                builder.AddQueueServiceClient(Configuration["Azure:Store:Blob:queue"], preferMsi: true);
            });

            var mongoConn = Configuration.GetMongoConnect();
            var mongoUri = MongoUrl.Create(mongoConn);
            var mongoClient = new MongoClient(mongoUri);
            services.AddSingleton<IMongoClient>(mongoClient);

            services.AddScoped<AppService>();
            services.AddScoped<PlanGroupService>();
            services.AddScoped<PlanItemService>();
            services.AddSingleton<RecyclableMemoryStreamManager>();

            services.AddScoped<LogJob>();
        }
        private void AddQuartzAsync(IServiceCollection services)
        {
            services.AddQuartz((config) =>
            {
                config.UseMicrosoftDependencyInjectionJobFactory();
                config.UsePersistentStore(opt =>
                {
                    opt.UseJsonSerializer();
#if DBSELECT_MSSQL
                    opt.UseSqlServer(Configuration.GetDbConnect());
#elif DBSELECT_SQLITE
                    opt.UseSQLite(Configuration.GetDbConnect());
#endif
                });
            });
        }
        private async Task CreateDb(IServiceProvider provider,IWebHostEnvironment env)
        {
            using (var s = provider.CreateScope())
            {
                var db = s.ServiceProvider.GetRequiredService<PlanDbContext>();
                db.Database.SetCommandTimeout(TimeSpan.FromMinutes(5));
                if (db.Database.EnsureCreated())
                {
#if DBSELECT_MSSQL
                    var fn = "mssql.sql";
#elif DBSELECT_SQLITE
                    var fn = "sqlite.sql";
#endif
                    var str = File.ReadAllText(Path.Combine(env.ContentRootPath, "Sql", fn));
                    db.Database.ExecuteSqlRaw(str);
                }

                var fc = s.ServiceProvider.GetRequiredService<ISchedulerFactory>();
                var scheduler = await fc.GetScheduler();
                await scheduler.Start();
            }
        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            CreateDb(app.ApplicationServices,env).GetAwaiter().GetResult();
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
            app.UseResponseCompression();
            app.UseStaticFiles();
            if (!env.IsDevelopment())
            {
                app.UseSpaStaticFiles();
            }

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseAzureSignalR(builder =>
            {
            });
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/Anf/swagger.json", "Anf API");
            });
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "ClientApp";

                //if (env.IsDevelopment())
                //{
                //    spa.UseProxyToSpaDevelopmentServer("http://localhost:4200");
                //    //spa.UseAngularCliServer(npmScript: "start");
                //}
            });

            
        }
    }

}
