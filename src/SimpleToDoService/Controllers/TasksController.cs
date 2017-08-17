using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SimpleToDoService.Common;
using SimpleToDoService.Entities;
using SimpleToDoService.Middleware;
using SimpleToDoService.Repository;

namespace SimpleToDoService.Controllers
{
	[Authorize]
	[MiddlewareFilter(typeof(CheckUserMiddleware))]
	[Route("api/v1/[controller]")]
	public class TasksController : Controller
	{
		private readonly IToDoRepository repository;
		private readonly IPushNotificationScheduler pushScheduler;
		private readonly ILogger<TasksController> logger;

		public Guid CurrentUserUuid
		{
			get { return (Guid)HttpContext.Items["UserUuid"]; }
		}

		public TasksController(IToDoRepository repository, IPushNotificationScheduler pushScheduler, ILogger<TasksController> logger)
		{
			this.repository = repository;
			this.pushScheduler = pushScheduler;
			this.logger = logger;
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
			if (entry == null)
				return BadRequest();

			var created = await CreateTask(entry);

			return CreatedAtRoute("GetTask", new { Uuid = created.Uuid }, created);
		}

		[HttpPut("{uuid:Guid?}")]
		public async System.Threading.Tasks.Task<IActionResult> Put(Guid? uuid, [FromBody] Task entry)
		{
			if (!uuid.HasValue)
				return BadRequest(new ServiceError() { Message = "Object Uuid not specified" });

			entry.Uuid = uuid.Value;

			var updated = await UpdateTask(entry);

			if (updated == null)
				return NotFound(entry);

			return Ok(updated);
		}

		[HttpDelete("{uuid:Guid?}")]
		public async System.Threading.Tasks.Task<IActionResult> Delete(Guid? uuid)
		{
			if (!uuid.HasValue)
				return BadRequest(new ServiceError() { Message = "Object Uuid not specified" });

			var deleted = await DeleteTask(uuid.Value);

			if (!deleted)
				return new NotFoundResult();

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

			await pushScheduler.SchedulePushNotifications(entry);

			return Ok(entry);
		}

		[HttpPost("BatchUpdate")]
		public async System.Threading.Tasks.Task<IActionResult> BatchUpdate([FromBody] BatchUpdateInstruction instruction) {
			foreach(var task in instruction.ToCreate)
			{
				var t = await CreateTask(task);
				repository.DetachTask(t);
			}

			foreach(var task in instruction.ToUpdate)
			{
				var t = await UpdateTask(task);
				repository.DetachTask(t);
			}

			foreach(var uuid in instruction.ToDelete)
			{
				await DeleteTask(uuid);
			}

			return Ok(Get(null));
		}

		private async System.Threading.Tasks.Task<Task> CreateTask(Task task) 
		{
			task.UserUuid = CurrentUserUuid;

			if (!task.TargetDate.HasValue)
				task.TargetDateIncludeTime = false;
			
			var created = repository.CreateTask(task);
			await pushScheduler.SchedulePushNotifications(created);
			return created;
		}

		private async System.Threading.Tasks.Task<Task> UpdateTask(Task task)
		{
			task.UserUuid = CurrentUserUuid;

			if (!task.TargetDate.HasValue)
				task.TargetDateIncludeTime = false;

			var updated = repository.UpdateTask(task);

			if (updated == null)
				return null;

			var reloaded = repository.ReloadTask(updated);

			await pushScheduler.SchedulePushNotifications(reloaded);

			return reloaded;
		}

		private async System.Threading.Tasks.Task<bool> DeleteTask(Guid uuid)
		{
			var toDelete = repository.Task(CurrentUserUuid, uuid);
			if (toDelete == null)
				return false;

			toDelete.TargetDate = null;
			await pushScheduler.SchedulePushNotifications(toDelete);

			repository.DeleteTask(toDelete);

			return true;
		}
	}
}
