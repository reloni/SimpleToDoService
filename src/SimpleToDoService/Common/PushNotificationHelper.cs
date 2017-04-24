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
        
		public async void SchedulePushNotification(Task task) 
		{
			await System.Threading.Tasks.Task.Factory.StartNew(() => 
			{
				var currentNotification = task.PushNotifications.Where(o => o.TaskUuid == task.Uuid).FirstOrDefault();

				DeletePushNotification(currentNotification);

				CreatePushNotification(task);	
			});
		}

		void DeletePushNotification(PushNotification notification)
		{
			if (notification == null)
				return;
		}

		async void CreatePushNotification(Task task)
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
				filters = new[] { new { field = "tag", key = "user_id", relation = "=", value = task.UserUuid.ToString() } },
				send_after = notificationDate.Value.ToString("yyyy-MM-dd HH:mm:ss GMTzzzz")//"2018-04-23 17:58:00 GMT+0300"
			});
			;
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
					var notificationId = json["id"].Value<String>();
					if (notificationId != null && notificationId.Length > 0)
					{
						var notification = new PushNotification()
						{
							ServiceId = new Guid(notificationId),
							TaskUuid = task.Uuid,
							UserUuid = task.UserUuid
						};

						repository.CreatePushNotification(notification);
					}
				}
			}
			catch (WebException ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message);
				System.Diagnostics.Debug.WriteLine(new StreamReader(ex.Response.GetResponseStream()).ReadToEnd());
			}
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
