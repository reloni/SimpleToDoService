using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleToDoService.Common;
using SimpleToDoService.Entities;
using SimpleToDoService.Middleware;
using SimpleToDoService.Repository;

namespace SimpleToDoService
{
	[Authorize]
	[MiddlewareFilter(typeof(CheckUserMiddleware))]
	[Route("api/v1/[controller]")]
	public class TasksController : Controller
	{
		private readonly IToDoRepository repository;

		public Guid CurrentUserUuid
		{
			get { return (Guid)HttpContext.Items["UserUuid"]; }
		}

		public string CurrentUserFirebaseId
		{
			get { return HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value; }
		}

		public TasksController(IToDoRepository repository)
		{
			this.repository = repository;
		}

		[HttpGet("{uuid:Guid?}", Name = "GetTask")]
		public IEnumerable<Task> Get(Guid? uuid)
		{
			var entries = repository.Tasks(CurrentUserUuid).OrderBy(o => o.CreationDate).Where(o => !o.Completed);
		
			if (uuid != null)
				entries = entries.Where(o => o.Uuid == uuid);
			
			return entries;
		}

		[HttpGet("All")]
		public IEnumerable<Task> GetAll([FromQuery] bool? completed,[FromQuery] int offset = 0,[FromQuery] int count = 20)
		{
			IEnumerable<Task> entries = repository.Tasks(CurrentUserUuid).OrderBy(o => o.CreationDate);

			if (completed.HasValue)
				entries = entries.Where(o => o.Completed == completed.Value);

			var totalCount = entries.Count();
			Response.Headers.Add("X-ItemsCount", totalCount.ToString());

			return entries.Skip(offset).Take(count);
		}

		[HttpPost]
		public async System.Threading.Tasks.Task<IActionResult> Post([FromBody] Task entry)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);
			
			if (entry == null)
				return BadRequest();

			entry.UserUuid = CurrentUserUuid;

			var created = repository.CreateTask(entry);

			await new PushNotificationScheduler(repository).SchedulePushNotifications(created);

			return CreatedAtRoute("GetTask", new { Uuid = created.Uuid }, created);
		}

		[HttpPut("{uuid:Guid?}")]
		public async System.Threading.Tasks.Task<IActionResult> Put(Guid? uuid, [FromBody] Task entry)
		{
			if (!uuid.HasValue)
				return BadRequest(new ServiceError() { Message = "Object Uuid not specified" });

			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			entry.Uuid = uuid.Value;
			entry.UserUuid = CurrentUserUuid;

			var updated = repository.UpdateTask(entry);

			if (updated == null)
				return NotFound(entry);

			var reloaded = repository.Task(CurrentUserUuid, entry.Uuid);

			await new PushNotificationScheduler(repository).SchedulePushNotifications(reloaded);

			return Ok(updated);
		}

		[HttpDelete("{uuid:Guid?}")]
		public async System.Threading.Tasks.Task<IActionResult> Delete(Guid? uuid)
		{
			if (!uuid.HasValue)
				return BadRequest(new ServiceError() { Message = "Object Uuid not specified" });

			var toDelete = repository.Task(CurrentUserUuid, uuid.Value);
			if(toDelete == null)
				return new NotFoundResult();

			toDelete.TargetDate = null;
			await new PushNotificationScheduler(repository).SchedulePushNotifications(toDelete);

			repository.DeleteTask(toDelete);
			return new StatusCodeResult(204);
		}

		[HttpPost("{uuid:Guid}/ChangeCompletionStatus/")]
		public async System.Threading.Tasks.Task<IActionResult> ChangeCompletionStatus(Guid uuid, [FromQuery] bool completed)
		{
			var entry = repository.Task(CurrentUserUuid, uuid);

			if (entry == null)
				return new NotFoundResult();

			entry.Completed = completed;
			repository.UpdateTask(entry);

			await new PushNotificationScheduler(repository).SchedulePushNotifications(entry);

			return Ok(entry);
		}
	}
}
