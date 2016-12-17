using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
						o.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
						o.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
					});



			var connectionString = Configuration[string.Format("DbContextSettings:{0}", 
			                                                   Environment.GetEnvironmentVariable("db-connection-string"))];

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

			app.UseMiddleware<BasicAuthMiddleware>();

			app.UseMvc();
		}
	}
}