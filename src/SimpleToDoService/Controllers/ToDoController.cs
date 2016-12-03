using System.Linq;
using Microsoft.AspNetCore.Mvc;
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

		public JsonResult Get()
		{
			return Json(repository.Entries(CurrentUserId).Select(o => new { o.Id, o.Completed, o.Description, o.Notes, UserId = CurrentUserId }));
		}
	}
}
