using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimpleToDoService.Middleware;
using SimpleToDoService.DB;
using SimpleToDoService.Common;
using NLog.Extensions.Logging;
using NLog.Web;
using NLog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace SimpleToDoService
{
	public class Startup
	{
		IConfigurationRoot Configuration { get; }
		IHostingEnvironment HostingEnvironment { get; set; }

		public Startup(IHostingEnvironment env)
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath)
				.AddJsonFile("AppSettings.json", optional: false, reloadOnChange: true)
				.AddEnvironmentVariables();
			Configuration = builder.Build();
			HostingEnvironment = env;
		}

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddApiVersioning(options =>
			{
				options.ReportApiVersions = true;
				options.ApiVersionReader = new QueryStringApiVersionReader();
				options.AssumeDefaultVersionWhenUnspecified = true;
				options.DefaultApiVersion = new ApiVersion(1, 0);
				options.ApiVersionSelector = new CurrentImplementationApiVersionSelector(options);
			});

			services.AddResponseCompression();

			services.AddMvc(options =>{ options.Filters.Add(typeof(ValidateModelAttribute)); })
			        .AddXmlSerializerFormatters()
			        .AddXmlDataContractSerializerFormatters()
			        .AddJsonOptions(o =>
					{
						o.SerializerSettings.DateFormatString = "yyyy-MM-dd'T'HH:mm:ss.fffzz";
						o.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc;
						o.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
						o.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
					});

			services.AddAuthorization(options =>
			{
				options.DefaultPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
					.RequireAuthenticatedUser()
					.Build();
			});

			services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o =>
			{
				o.Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");
				o.Authority = Environment.GetEnvironmentVariable("JWT_AUTHORITY");
				o.TokenValidationParameters.ValidateLifetime = true;
				o.TokenValidationParameters.ValidateIssuer = true;
				o.TokenValidationParameters.ValidateAudience = true;
			});

			var connectionString = Configuration["DbContextSettings:ConnectionString_Postgres"];
			connectionString = connectionString.Replace("{USER_ID}", Environment.GetEnvironmentVariable("POSTGRES_USER"))
							   .Replace("{PASSWORD}", Environment.GetEnvironmentVariable("POSTGRES_PASSWORD"))
							   .Replace("{DB}", Environment.GetEnvironmentVariable("POSTGRES_DB"))
							   .Replace("{HOST}", Environment.GetEnvironmentVariable("POSTGRES_HOST"))
							   .Replace("{PORT}", Environment.GetEnvironmentVariable("POSTGRES_PORT"));

			services.AddDbContext<ToDoDbContext>(opts => opts.UseNpgsql(connectionString));
			services.AddScoped<IToDoDbContext, ToDoDbContext>();
			services.AddScoped<IToDoRepository, ToDoRepository>();
			services.AddScoped<IPushNotificationScheduler, OneSignalPushNotificationScheduler>();

			if (HostingEnvironment.IsDevelopment())
			{
				services.AddScoped<IPushNotificationScheduler, DummyPushNotificationScheduler>();
			} else {
				services.AddScoped<IPushNotificationScheduler, OneSignalPushNotificationScheduler>();
			}
		}

		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			//add NLog to ASP.NET Core
			loggerFactory.AddNLog();

			//add NLog.Web
			//app.AddNLogWeb();

			//configure nlog.config in your project root.
			env.ConfigureNLog("nlog.config");

			LogManager.Configuration.Variables["logdir"] = Environment.GetEnvironmentVariable("LOGS_DIRECTORY");

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseAuthentication();

			app.UseResponseCompression();

			app.UseMiddleware<CheckUserMiddleware>();

			app.UseMvc();
		}
	}
}
