using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Hosting;

namespace SimpleToDoService
{
	[Route("/")]
	public class RootController : Controller
	{
		private readonly IHostEnvironment _hostingEnvironment;

		public RootController(IHostEnvironment hostingEnvironment)
		{
			_hostingEnvironment = hostingEnvironment;
		}

		[HttpGet]
		public IActionResult Get()
		{
			return Ok();
		}
	}
}
