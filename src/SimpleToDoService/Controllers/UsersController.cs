using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleToDoService.Middleware;
using SimpleToDoService.Repository;

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
		public IActionResult DeleteUser()
		{
			var user = repository.User(CurrentUserUuid);

			if (user == null)
				return NotFound();

			repository.DeleteUser(user);

			return new StatusCodeResult(204);
		}
	}
}
