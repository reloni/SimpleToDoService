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

namespace SimpleToDoService
{
	[Route("/")]
	public class RootController : Controller
	{
		private readonly IHostingEnvironment _hostingEnvironment;

		public RootController(IHostingEnvironment hostingEnvironment)
		{
			_hostingEnvironment = hostingEnvironment;
		}

		[HttpGet]
		public IActionResult Get()
		{
			return Ok();
		}

		[HttpPost("push2")]
		public async Task<IActionResult> Push2() 
		{
			var request = WebRequest.Create("https://onesignal.com/api/v1/notifications") as HttpWebRequest;

			request.Method = "POST";
			request.ContentType = "application/json; charset=utf-8";
			request.Headers["authorization"] = "Basic NTNmZWI0MjktOWI2My00M2FiLTk1ZDQtMDljYjBhMjhhM2Vh";

			var sendJson = JObject.FromObject(new {
				app_id = "ffe9789a-e9bc-4789-9cbb-4552664ba3fe",
				contents = new { en = "Test message" },
				filters = new [] { new { field = "tag", key = "custom", relation = "=", value = "tag" } },
				send_after = "2018-04-23 17:58:00 GMT+0300"
			});

			var byteArray = Encoding.UTF8.GetBytes(sendJson.ToString());

			try
			{
				using (var writer = await request.GetRequestStreamAsync())
				{
					writer.Write(byteArray, 0, byteArray.Length);
				}

				using (var response = await request.GetResponseAsync())
				using (var reader = new StreamReader(response.GetResponseStream()))
				{
					var json = JObject.Parse(reader.ReadToEnd());
					System.Diagnostics.Debug.WriteLine(String.Format("id = {0}", json["id"]));
				}
			}
			catch (WebException ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message);
				System.Diagnostics.Debug.WriteLine(new StreamReader(ex.Response.GetResponseStream()).ReadToEnd());
			}

			return Ok();
		}

		[HttpGet("push")]
		public async Task<IActionResult> Push()
		{
			var deviceId = "LySqbodo7WMqWO24vEULM1x4na/sm43fYuNs9/eZHCs=";
			int port = 2195;
			string hostname = "gateway.sandbox.push.apple.com";
			//var hostname = "gateway.push.apple.com";

			//I have removed certificate. Keep your certificate in wwwroot/certificate location. This location is not mandatory
			string certificatePath = "/Users/AntonEfimenko/Documents/Programming/todo-cert/cert-prod.pfx";
			//var clientCertificate = new X509Certificate2(certificatePath, "11", X509KeyStorageFlags.PersistKeySet);
			X509Certificate2 clientCertificate = new X509Certificate2(System.IO.File.ReadAllBytes(certificatePath), "");
			X509Certificate2Collection certificatesCollection = new X509Certificate2Collection(clientCertificate);

			TcpClient client = new TcpClient(AddressFamily.InterNetwork);
			await client.ConnectAsync(hostname, port);


			//SslStream sslStream = new SslStream(client.GetStream(), false);
			SslStream sslStream = new SslStream(
				client.GetStream(), false,
				new RemoteCertificateValidationCallback(ValidateServerCertificate),
				null);

			try
			{
				await sslStream.AuthenticateAsClientAsync(hostname, certificatesCollection, SslProtocols.Tls, false);
				MemoryStream memoryStream = new MemoryStream();
				BinaryWriter writer = new BinaryWriter(memoryStream);
				writer.Write((byte)0);
				writer.Write((byte)0);
				writer.Write((byte)32);

				writer.Write(Convert.FromBase64String(deviceId));
				string payload = "{\"aps\":{\"alert\":\"" + "Test message" + "\",\"badge\":0,\"sound\":\"default\"}}";
				writer.Write((byte)0);
				writer.Write((byte)payload.Length);
				byte[] b1 = System.Text.Encoding.UTF8.GetBytes(payload);
				writer.Write(b1);
				writer.Flush();
				byte[] array = memoryStream.ToArray();
				sslStream.Write(array);
				sslStream.Flush();
				client.Dispose();
			}
			catch (Exception ex)
			{
				var m = ex.Message;
				throw ex;
			}
			finally 
			{
				client.Dispose();	
			}
			//catch// (AuthenticationException ex)
			//{
			//	client.Dispose();
			//}
			//catch// (Exception e)
			//{
			//	client.Dispose();
			//}

			return Content("Notification sent. Check your device.");
		}

		private static bool ValidateServerCertificate(
			  object sender,
			  X509Certificate certificate,
			  X509Chain chain,
			  SslPolicyErrors sslPolicyErrors)
		{
			if (sslPolicyErrors == SslPolicyErrors.None)
				return true;

			Console.WriteLine("Certificate error: {0}", sslPolicyErrors);

			// Do not allow this client to communicate with unauthenticated servers.
			return false;
		}
	}
}
