﻿using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleToDoService.Middleware;
using SimpleToDoService.Repository;
using SimpleToDoService.Common;
using System.Net;
using System.IO;
using Microsoft.Extensions.Logging;

namespace SimpleToDoService.Controllers
{
	[Authorize]
	[MiddlewareFilter(typeof(CheckUserMiddleware))]
	[Route("api/v{version:apiVersion}/[controller]")]
	[ApiVersion("1.0")]
	public class UsersController : Controller
	{
		private readonly IToDoRepository repository;
		private readonly ILogger<TasksController> logger;

		public Guid CurrentUserUuid
		{
			get { return (Guid)HttpContext.Items["UserUuid"]; }
		}

		public UsersController(IToDoRepository repository, ILogger<TasksController> logger)
		{
			this.repository = repository;
			this.logger = logger;
		}

		[HttpDelete()]
		public async System.Threading.Tasks.Task<IActionResult> DeleteUser()
		{
			var user = repository.User(CurrentUserUuid);

			if (user == null)
				return NotFound();

			try
			{
				await Auth0Client.DeleteUser(user.ProviderId);
			}
			catch (WebException ex)
			{
				logger.LogError(0,
								ex,
								"Error while deleting Auth0 user with response {0}",
								new StreamReader(ex.Response.GetResponseStream()).ReadToEnd());
				return StatusCode((int)HttpStatusCode.InternalServerError, new ServiceError() { Message = "Error while deleting user" });
			}
			catch (Exception ex)
			{
				logger.LogError(0, ex, "Error while deleting Auth0 user");
				return StatusCode((int)HttpStatusCode.InternalServerError, new ServiceError() { Message = "Error while deleting user" });
			}

			repository.DeleteUser(user);

			return new StatusCodeResult(204);
		}

		[HttpPost("LogOut")]
		public async System.Threading.Tasks.Task<IActionResult> LogOut([FromBody] LogOutParameters refreshToken) 
		{
			try
			{
				await Auth0Client.RevokeRefreshToken(refreshToken.RefreshToken);
			}
			catch (WebException ex)
			{
				logger.LogError(0,
								ex,
								"Error while revoking refresh token with response {0}",
								new StreamReader(ex.Response.GetResponseStream()).ReadToEnd());
			}
			catch (Exception ex)
			{
				logger.LogError(0, ex, "Error while revoking refresh token");
			}

			return new StatusCodeResult(200);
		}
	}
}
