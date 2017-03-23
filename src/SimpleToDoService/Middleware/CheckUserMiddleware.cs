using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SimpleToDoService.Repository;
using SimpleToDoService.Context;
using Microsoft.AspNetCore.Builder;
using System.Security.Claims;

namespace SimpleToDoService.Middleware
{
	public class CheckUserMiddleware
	{
		private readonly RequestDelegate next;
		private IToDoRepository repository;

		public CheckUserMiddleware() { }

		public CheckUserMiddleware(RequestDelegate next, IToDoRepository repository)
		{
			this.next = next;
			this.repository = repository;
		}

		public void Configure(IApplicationBuilder applicationBuilder)
		{
			applicationBuilder.UseMiddleware<CheckUserMiddleware>();
		}

		public async Task Invoke(HttpContext context)
		{
			var test = context.Items["UserUuid"];
			var userId = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
			context.Items.Add("UserUuid", userId);

			await next.Invoke(context);
		}
	}

}