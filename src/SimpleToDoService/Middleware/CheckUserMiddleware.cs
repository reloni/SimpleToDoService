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
using SimpleToDoService.Entities;

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

		public async System.Threading.Tasks.Task Invoke(HttpContext context)
		{
			if (context.User.Identity.IsAuthenticated && !context.Items.Keys.Contains("UserUuid"))
			{
				context.Items.Add("UserUuid", LoadUserGuid(context));
			}
			await next.Invoke(context);
		}

		private Guid LoadUserGuid(HttpContext context)
		{
			var providerId = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
			var currentUser = repository.Users().Where(o => o.ProviderId == providerId).FirstOrDefault();

			if (currentUser != null)
			{
				return currentUser.Uuid;
			}

			var user = new User()
			{
				ProviderId = providerId,
				Email = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value,
				FirstName = "",
				LastName = ""
			};

			return repository.CreateUser(user).Uuid;
		}
	}

}