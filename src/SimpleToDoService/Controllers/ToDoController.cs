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

		[HttpGet("{id:int?}")]
		public IEnumerable<ToDoEntry> Get(int? id)
		{
			var entries = repository.Entries(CurrentUserId);

			if (id != null)
				entries = entries.Where(o => o.Id == id);

			return entries;
		}
	}
}
