using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace SimpleToDoService.Common
{
	public class Auth0Client
	{
		internal static async Task DeleteUser(string userId)
		{
			var token = await Auth0Client.GetToken();

			if (token == null)
				throw new Exception("Unable to get Authentication token");

			var request = WebRequest.Create(new Uri(string.Format("https://reloni.eu.auth0.com/api/v2/users/{0}", userId))) as HttpWebRequest;

			request.Method = "DELETE";
			request.Headers["authorization"] = String.Format("Bearer {0}", token);

			using(var response = await request.GetResponseAsync()) { }
		}

		private static async Task<String> GetToken()
		{
			var request = WebRequest.Create("https://reloni.eu.auth0.com/oauth/token") as HttpWebRequest;

			request.Method = "POST";
			request.ContentType = "application/json; charset=utf-8";

			var requestJson = JObject.FromObject(new
			{
				client_id = Environment.GetEnvironmentVariable("AUTH0_CLIENT_ID"),
				client_secret = Environment.GetEnvironmentVariable("AUTH0_CLIENT_SECRET"),
				audience = Environment.GetEnvironmentVariable("AUTH0_AUDIENCE"),
				grant_type = Environment.GetEnvironmentVariable("AUTH0_GRANT_TYPE")
			});

			var byteArray = Encoding.UTF8.GetBytes(requestJson.ToString());

			using (var writer = await request.GetRequestStreamAsync())
			{
				writer.Write(byteArray, 0, byteArray.Length);
			}

			using (var response = await request.GetResponseAsync())
			using (var reader = new StreamReader(response.GetResponseStream()))
			{
				var json = JObject.Parse(reader.ReadToEnd());
				return json["access_token"]?.ToString();
			}
		}
	}
}
