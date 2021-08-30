using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using NLog.Extensions.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;

namespace SimpleToDoService
{
	public class Program
	{
		public static void Main(string[] args)
		{
			CreateWebHostBuilder(args)
				.UseKestrel()
				.Build()
				.Run();
		}

		public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
			WebHost.CreateDefaultBuilder(args)
				.ConfigureLogging((hostingContext, logging) =>
				{
					logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
					logging.AddDebug();
					logging.AddNLog("nlog.config");
				})
				.UseStartup<Startup>();
	}
}

