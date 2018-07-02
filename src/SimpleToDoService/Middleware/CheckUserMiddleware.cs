using System;
using Microsoft.AspNetCore.Http;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using System.Security.Claims;
using SimpleToDoService.DB;

namespace SimpleToDoService.Middleware
{
	public class CheckUserMiddleware
	{
		private readonly RequestDelegate next;

		public CheckUserMiddleware() { }

		public CheckUserMiddleware(RequestDelegate next)
		{
			this.next = next;
		}

		public void Configure(IApplicationBuilder applicationBuilder)
		{
			applicationBuilder.UseMiddleware<CheckUserMiddleware>();
		}

		public async System.Threading.Tasks.Task Invoke(HttpContext context, IToDoRepository repository)
		{
			if (context.User.Identity.IsAuthenticated && !context.Items.Keys.Contains("UserUuid"))
			{
				context.Items.Add("UserUuid", LoadUserGuid(context, repository));
			}
			await next.Invoke(context);
		}

		private Guid LoadUserGuid(HttpContext context, IToDoRepository repository)
		{
			var providerId = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
			var currentUser = repository.UserByProviderId(providerId);

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