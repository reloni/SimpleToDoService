using System;
using SimpleToDoService.Entities;
using SimpleToDoService.Repository;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Net;
using System.IO;

namespace SimpleToDoService.Common
{
	public class PushNotificationHelper
	{
		private readonly IToDoRepository repository;

		public PushNotificationHelper(IToDoRepository repository)
		{
			this.repository = repository;
		}

		public async System.Threading.Tasks.Task SchedulePushNotification(Task task)
		{
			var currentNotification = task.PushNotifications.FirstOrDefault();//repository.PushNotifications(task).FirstOrDefault();
			DeletePushNotification(currentNotification);
			await CreatePushNotification(task);
		}

		void DeletePushNotification(PushNotification notification)
		{
			if (notification == null)
				return;
		}

		async System.Threading.Tasks.Task CreatePushNotification(Task task)
		{
			var notificationDate = TaskDate(task);
			if (!notificationDate.HasValue)
				return;

			var request = WebRequest.Create("https://onesignal.com/api/v1/notifications") as HttpWebRequest;

			request.Method = "POST";
			request.ContentType = "application/json; charset=utf-8";

			request.Headers["authorization"] = String.Format("Basic {0}", Environment.GetEnvironmentVariable("ONE_SIGNAL_KEY"));

			var sendJson = JObject.FromObject(new
			{
				app_id = Environment.GetEnvironmentVariable("ONE_SIGNAL_APP_ID"),
				contents = new { en = task.Description },
				filters = new[] { new { field = "tag", key = "user_id", relation = "=", value = repository.User(task.UserUuid).FirebaseId } },
				send_after = notificationDate.Value.ToString("yyyy-MM-dd HH:mm:ss 'GMT'zzzz")
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
					var notificationId = json["id"].ToString();
					if (notificationId != null && notificationId.Length > 0)
					{
						var notification = new PushNotification()
						{
							ServiceId = new Guid(notificationId),
							TaskUuid = task.Uuid,
							UserUuid = task.UserUuid,
							DueDate = notificationDate.Value
						};

						repository.CreatePushNotification(notification);
					}
				}
			}
#if DEBUG
			catch (WebException ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message);
				System.Diagnostics.Debug.WriteLine(new StreamReader(ex.Response.GetResponseStream()).ReadToEnd());
			}
#else
			catch { }
#endif
		}

		private static DateTime? TaskDate(Task task)
		{
			if (!task.TargetDate.HasValue)
				return null;

			var notificationDate = task.TargetDate.Value.ToUniversalTime();

			if (!task.TargetDateIncludeTime)
				return new DateTime(notificationDate.Year, notificationDate.Month, notificationDate.Day, 0, 0, 0, notificationDate.Kind);

			return task.TargetDate;
		}
	}
}
