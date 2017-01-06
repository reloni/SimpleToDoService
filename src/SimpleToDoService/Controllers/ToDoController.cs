using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using SimpleToDoService.Entities;
using SimpleToDoService.Repository;

namespace SimpleToDoService
{
	[Route("api/[controller]")]
	public class ToDoEntriesController : Controller
	{
		private readonly IToDoRepository repository;

		public Guid CurrentUserUuid
		{
			get { return (Guid)HttpContext.Items["UserUuid"]; }
		}

		public ToDoEntriesController(IToDoRepository repository)
		{
			this.repository = repository;
		}

		[HttpGet("{uuid:Guid?}", Name = "GetToDoEntry")]
		public IEnumerable<ToDoEntry> Get(Guid? uuid)
		{
			var entries = repository.Entries(CurrentUserUuid).OrderBy(o => o.CreationDate).Where(o => !o.Completed);

			if (uuid != null)
				entries = entries.Where(o => o.Uuid == uuid);

			return entries;
		}

		[HttpGet("All")]
		public IEnumerable<ToDoEntry> GetAll()
		{
			return repository.Entries(CurrentUserUuid).OrderBy(o => o.CreationDate);
		}

		[HttpPost]
		public IActionResult Post([FromBody] ToDoEntry entry)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);
			
			if (entry == null)
				return BadRequest();

			entry.UserUuid = CurrentUserUuid;
			//entry.Id = 0;
			var created = repository.CreateEntry(entry);
			return CreatedAtRoute("GetToDoEntry", new { Uuid = created.Uuid }, created);
		}

		[HttpPut("{uuid:Guid?}")]
		public IActionResult Put(Guid? uuid, [FromBody] ToDoEntry entry)
		{
			if (!uuid.HasValue)
				return BadRequest(new ServiceError() { Message = "Object Uuid not specified" });

			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			entry.Uuid = uuid.Value;
			entry.UserUuid = CurrentUserUuid;

			var updated = repository.UpdateEntry(entry);

			if (updated == null)
				return NotFound(entry);

			return Ok(updated);
		}

		[HttpDelete("{uuid:Guid?}")]
		public IActionResult Delete(Guid? uuid)
		{
			if (!uuid.HasValue)
				return BadRequest(new ServiceError() { Message = "Object Uuid not specified" });

			var deleted = repository.DeleteEntry(uuid.Value);

			if (deleted)
				return new StatusCodeResult(204);

			return new NotFoundResult();
		}

		[HttpPost("{uuid:Guid}/ChangeCompletionStatus/")]
		public IActionResult ChangeCompletionStatus(Guid uuid, [FromQuery] bool completed)
		{
			var entry = repository.Entry(CurrentUserUuid, uuid);

			if (entry == null)
				return new NotFoundResult();

			entry.Completed = completed;
			repository.UpdateEntry(entry);

			return Ok(entry);
		}
	}
}
