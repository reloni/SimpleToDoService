﻿using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleToDoService.Middleware;
using SimpleToDoService.Repository;
using SimpleToDoService.Common;
using System.Net;

namespace SimpleToDoService.Controllers
{
	[Authorize]
	[MiddlewareFilter(typeof(CheckUserMiddleware))]
	[Route("api/v{version:apiVersion}/[controller]")]
	[ApiVersion("1.0")]
	public class UsersController : Controller
	{
		private readonly IToDoRepository repository;

		public Guid CurrentUserUuid
		{
			get { return (Guid)HttpContext.Items["UserUuid"]; }
		}

		public UsersController(IToDoRepository repository)
		{
			this.repository = repository;
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
#if DEBUG
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message);
				return StatusCode((int)HttpStatusCode.InternalServerError, new ServiceError() { Message = "Error while deleting user" });
			}
#else
			catch 
			{
				return StatusCode((int)HttpStatusCode.InternalServerError, new ServiceError() { Message = "Error while deleting user" });
			}
#endif

			repository.DeleteUser(user);

			return new StatusCodeResult(204);
		}
	}
}
