using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

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
