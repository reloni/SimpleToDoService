using System;
using Microsoft.AspNetCore.Mvc;

namespace SimpleToDoService
{
	[Route("/")]
	public class RootController : Controller
	{
		[HttpGet]
		public IActionResult Get()
		{
			return Ok();
		}
	}
}
