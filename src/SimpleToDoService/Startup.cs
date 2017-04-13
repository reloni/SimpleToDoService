using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SimpleToDoService.Context;
using SimpleToDoService.Middleware;
using SimpleToDoService.Repository;

namespace SimpleToDoService
{
	public class Startup
	{
		public IConfigurationRoot Configuration { get; }

		public Startup(IHostingEnvironment env)
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath)
				.AddJsonFile("AppSettings.json", optional: false, reloadOnChange: true)
				.AddEnvironmentVariables();
			Configuration = builder.Build();
		}

		public void ConfigureServices(IServiceCollection services)
		{
			
			services.AddMvc()
			        
				.AddXmlSerializerFormatters()
				.AddXmlDataContractSerializerFormatters()
				.AddJsonOptions(o => 
					{
						o.SerializerSettings.DateFormatString = "yyyy-MM-dd'T'HH:mm:ss.fffzz";
						o.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc;
						o.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
						o.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
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
		}

		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseJwtBearerAuthentication(new JwtBearerOptions
			{
				AutomaticAuthenticate = true,
				Authority = "https://securetoken.google.com/simpletaskmanager-9c565",
				TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuer = true,
					ValidIssuer = "https://securetoken.google.com/simpletaskmanager-9c565",
					ValidateAudience = true,
					ValidAudience = "simpletaskmanager-9c565",
					ValidateLifetime = true
				}
			});

			app.UseMiddleware<CheckUserMiddleware>();

			app.UseMvc();
		}
	}
}
