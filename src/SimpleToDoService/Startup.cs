using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
			services.AddMvc();

			var connectionString = Configuration["DbContextSettings:ConnectionString"];
			Console.WriteLine(connectionString);
			//services.AddDbContext<DatabaseContext>(opts => opts.UseNpgsql(connectionString));
		}

		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseMvc();
			//app.UseMiddleware<HeaderCheckMiddleware>();
		}
	}
}