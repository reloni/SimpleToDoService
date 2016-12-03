using System;
using Microsoft.AspNetCore.Mvc;

namespace SimpleToDoService
{
	[Route("api/[controller]")]
	public class ToDoController : Controller
	{
		public JsonResult Get()
		{
			return Json(new { Test = "response" });
		}
	}
}
