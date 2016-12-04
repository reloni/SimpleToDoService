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
			var entries = repository.Entries(CurrentUserId);

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
			var created = repository.CreateEntry(entry);
			return CreatedAtRoute("GetToDoEntry", new { Id = created.Id }, created);
		}

		[HttpPut("{id:int}")]
		public IActionResult Put(int id, [FromBody] ToDoEntry entry)
		{
			//var v = ModelState.Values.FirstOrDefault();
			entry.Id = id;
			entry.UserId = CurrentUserId;

			var updated = repository.UpdateEntry(entry);

			if (updated == null)
				return NotFound(entry);

			return Ok(updated);
			/*
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			if (entry == null)
				return BadRequest();

			entry.UserId = CurrentUserId;
			var created = repository.CreateEntry(entry);
			return CreatedAtRoute("GetToDoEntry", new { Id = created.Id }, created);
			*/
		}
	}
}
