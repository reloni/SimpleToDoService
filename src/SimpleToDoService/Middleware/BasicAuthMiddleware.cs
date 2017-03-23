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

namespace SimpleToDoService.Middleware
{
	//public class BasicAuthMiddleware
	//{
	//	private readonly RequestDelegate next;
	//	private IToDoRepository repository;

	//	public BasicAuthMiddleware() { }

	//	public BasicAuthMiddleware(RequestDelegate next, IToDoRepository repository)
	//	{
	//		this.next = next;
	//		this.repository = repository;
	//	}

	//	public void Configure(IApplicationBuilder applicationBuilder)
	//	{
	//		applicationBuilder.UseMiddleware<BasicAuthMiddleware>();
	//	}

	//	public async Task Invoke(HttpContext context)
	//	{
	//		var authHeader = (string)context.Request.Headers["Authorization"];
	//		if (authHeader != null && authHeader.StartsWith("basic", StringComparison.OrdinalIgnoreCase))
	//		{
	//			var token = authHeader.Substring("Basic ".Length).Trim();
	//			var credentialstring = Encoding.UTF8.GetString(Convert.FromBase64String(token));
	//			var credentials = credentialstring.Split(':');
	//			var user = credentials.FirstOrDefault();
	//			var password = credentials.Skip(1).FirstOrDefault();

	//			var currentUser = repository.Users().Where(o => o.Email == user && o.Password == password).FirstOrDefault();

	//			if (currentUser != null)
	//			{
	//				context.Items.Add("UserUuid", currentUser.Uuid);
	//			}
	//			else
	//			{
	//				context.Response.StatusCode = 401; //Unauthorized
	//				var jsonString = "{ \"Error\" : \"Incorrect user name or password\" }";
	//				context.Response.ContentType = new MediaTypeHeaderValue("application/json").ToString();
	//				await context.Response.WriteAsync(jsonString, Encoding.UTF8);
	//				return;
	//			}
	//		}
	//		else
	//		{
	//			context.Response.StatusCode = 401; //Unauthorized
	//			var jsonString = "{ \"Error\" : \"Authorization header missed\" }";
	//			context.Response.ContentType = new MediaTypeHeaderValue("application/json").ToString();
	//			await context.Response.WriteAsync(jsonString, Encoding.UTF8);
	//			return;
	//		}

	//		await next.Invoke(context);
	//	}
	//}

}