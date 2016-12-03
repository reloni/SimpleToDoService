using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using System.Linq;
using Microsoft.EntityFrameworkCore;


namespace SimpleToDoService
{
	public class BasicAuthMiddleware
	{
		private readonly RequestDelegate next;
		private ToDoContext dbContext;

		public BasicAuthMiddleware(RequestDelegate next, ToDoContext databaseContext)
		{
			this.next = next;
			dbContext = databaseContext;
		}

		public async Task Invoke(HttpContext context)
		{
			var authHeader = (string)context.Request.Headers["Authorization"];
			if (authHeader != null && authHeader.StartsWith("basic", StringComparison.OrdinalIgnoreCase))
			{
				var token = authHeader.Substring("Basic ".Length).Trim();
				var credentialstring = Encoding.UTF8.GetString(Convert.FromBase64String(token));
				var credentials = credentialstring.Split(':');
				var user = credentials.FirstOrDefault();
				var password = credentials.Skip(1).FirstOrDefault();

				var currentUser = dbContext.Users.Where(o => o.Email == user && o.Password == password).FirstOrDefault();
				if (currentUser != null)
				{
					context.Items.Add("UserId", currentUser.Id);
				}
				else
				{
					context.Response.StatusCode = 401; //Unauthorized
					var jsonString = "{ \"Error\" : \"Incorrect user name or password\" }";
					context.Response.ContentType = new MediaTypeHeaderValue("application/json").ToString();
					await context.Response.WriteAsync(jsonString, Encoding.UTF8);
					return;
				}
			}
			else
			{
				context.Response.StatusCode = 401; //Unauthorized
				var jsonString = "{ \"Error\" : \"Authorization header missed\" }";
				context.Response.ContentType = new MediaTypeHeaderValue("application/json").ToString();
				await context.Response.WriteAsync(jsonString, Encoding.UTF8);
				return;
			}

			await next.Invoke(context);
		}
	}

}