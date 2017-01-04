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

		public int CurrentUserId
		{
			get { return (int)HttpContext.Items["UserId"]; }
		}

		public ToDoEntriesController(IToDoRepository repository)
		{
			this.repository = repository;
		}

		[HttpGet("{id:int?}", Name = "GetToDoEntry")]
		public IEnumerable<ToDoEntry> Get(int? id)
		{
			var entries = repository.Entries(CurrentUserId).Where(o => !o.Completed);

			if (id != null)
				entries = entries.Where(o => o.Id == id);

			return entries;
		}


		[HttpPost]
		public IActionResult Post([FromBody] ToDoEntry entry)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);
			
			if (entry == null)
				return BadRequest();

			entry.UserId = CurrentUserId;
			entry.Id = 0;
			var created = repository.CreateEntry(entry);
			return CreatedAtRoute("GetToDoEntry", new { Id = created.Id }, created);
		}

		[HttpPut("{id:int?}")]
		public IActionResult Put(int? id, [FromBody] ToDoEntry entry)
		{
			if (!id.HasValue)
				return BadRequest(new ServiceError() { Message = "Object Id not specified" });

			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			entry.Id = id.Value;
			entry.UserId = CurrentUserId;

			var updated = repository.UpdateEntry(entry);

			if (updated == null)
				return NotFound(entry);

			return Ok(updated);
		}

		[HttpDelete("{id:int?}")]
		public IActionResult Delete(int? id)
		{
			if (!id.HasValue)
				return BadRequest(new ServiceError() { Message = "Object Id not specified" });

			var deleted = repository.DeleteEntry(id.Value);

			if (deleted)
				return new StatusCodeResult(204);

			return new NotFoundResult();
		}
	}
}
