using System;
using Microsoft.AspNetCore.Mvc;
using SimpleToDoService.Entities;
using SimpleToDoService.Middleware;
using SimpleToDoService.Repository;

namespace SimpleToDoService
{
	[Route("api/v1/[controller]")]
	public class UserController : Controller
	{
		private readonly IToDoRepository repository;

		public Guid CurrentUserUuid
		{
			get { return (Guid)HttpContext.Items["UserUuid"]; }
		}

		public UserController(IToDoRepository repository)
		{
			this.repository = repository;
		}

		//[MiddlewareFilter(typeof(BasicAuthMiddleware))]
		[HttpGet(Name = "GetUserInfo")]
		public IActionResult Get()
		{
			var user = repository.User(CurrentUserUuid);

			if (user == null)
				return NotFound();

			return Ok(user);
		}

		//[MiddlewareFilter(typeof(BasicAuthMiddleware))]
		[HttpDelete]
		public IActionResult Delete()
		{
			var user = repository.User(CurrentUserUuid);

			if (user == null)
				return NotFound();			

			var deleted = repository.DeleteUser(user);

			if (deleted)
				return new StatusCodeResult(204);

			return new NotFoundResult();
		}

		[HttpPost]
		public IActionResult Post([FromBody] User user)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			if (user == null)
				return BadRequest();

			User created;
			try
			{
				created = repository.CreateUser(user);
			}
			catch (EmailExistedException)
			{
				return BadRequest(new ServiceError() { Message = "User with same email already exists" });
			}
			return CreatedAtRoute("GetUserInfo", created);
		}

		//[MiddlewareFilter(typeof(BasicAuthMiddleware))]
		[HttpPut]
		public IActionResult Put([FromBody] User user)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			user.Uuid = CurrentUserUuid;

			var updated = repository.UpdateUser(user);

			if (updated == null)
				return NotFound(user);

			return Ok(updated);
		}
	}
}
