using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SimpleToDoService
{
	[Route("api/[controller]")]
	public class ToDoController : Controller
	{
		private ToDoContext context;

		public int CurrentUserId
		{
			get { return (int)HttpContext.Items["UserId"]; }
		}
		
		public ToDoController(ToDoContext databaseContext)
		{
			context = databaseContext;
		}

		public JsonResult Get()
		{
			return Json(context.ToDoEntries.Where(o => o.User.Id == CurrentUserId).Select(o => new { o.Id, o.Completed, o.Description, o.Notes, UserId = CurrentUserId }));
		}
	}
}
