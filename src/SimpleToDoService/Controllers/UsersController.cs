﻿using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleToDoService.Middleware;
using SimpleToDoService.Repository;
using SimpleToDoService.Common;

namespace SimpleToDoService.Controllers
{
	[Authorize]
	[MiddlewareFilter(typeof(CheckUserMiddleware))]
	[Route("api/v1/[controller]")]
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
				await Common.Auth0Client.DeleteUser(user.ProviderId);
			}
#if DEBUG
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message);
				return StatusCode(500, new ServiceError() { Message = "Error while deleting user" });
			}
#else
			catch 
			{
				return StatusCode(500, new ServiceError() { Message = "Error while deleting user" });
			}
#endif

			repository.DeleteUser(user);

			return new StatusCodeResult(204);
		}
	}
}
