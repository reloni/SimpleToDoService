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
	public class PushNotificationScheduler
	{
		private readonly IToDoRepository repository;

		public PushNotificationScheduler(IToDoRepository repository)
		{
			this.repository = repository;
		}

		bool HasEqualDueDatest(Task task, PushNotification notification)
		{
			var taskDate = task.CheckedTargetDate();
			var notificationDate = notification?.DueDate;

			if (taskDate.HasValue && notificationDate.HasValue)
			{
				return taskDate.Value.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss.fffzz") ==
							   notificationDate.Value.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss.fffzz");
			}
			else
				return false;
		}

		public async System.Threading.Tasks.Task SchedulePushNotifications(Task task)
		{
			var currentNotification = task.PushNotifications.FirstOrDefault();

			if (!task.Completed && HasEqualDueDatest(task, currentNotification))
				return;

			await DeletePushNotification(currentNotification);

			if (!task.Completed)
				await CreatePushNotification(task);
		}

		async System.Threading.Tasks.Task DeletePushNotification(PushNotification notification)
		{
			if (notification == null)
				return;

			var url = String.Format("https://onesignal.com/api/v1/notifications/{0}?app_id={1}",
			                        notification.ServiceId.ToString(), Environment.GetEnvironmentVariable("ONE_SIGNAL_APP_ID"));
			var request = WebRequest.Create(url) as HttpWebRequest;

			request.ContentType = "application/json; charset=utf-8";
			request.Method = "DELETE";
			request.Headers["authorization"] = String.Format("Basic {0}", Environment.GetEnvironmentVariable("ONE_SIGNAL_KEY"));

			try
			{
				using (var writer = await request.GetRequestStreamAsync())
				{
					writer.Write(new Byte[0], 0, 0);
				}
				await request.GetResponseAsync();
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

			repository.DeletePushNotification(notification);
		}

		async System.Threading.Tasks.Task CreatePushNotification(Task task)
		{
			var notificationDate = task.CheckedTargetDate();
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
	}
}
