using System;
using SimpleToDoService.Entities;
using SimpleToDoService.Repository;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Net;
using System.IO;
using Microsoft.Extensions.Logging;

namespace SimpleToDoService.Common
{
	public interface IPushNotificationScheduler 
	{
		System.Threading.Tasks.Task SchedulePushNotifications(Task task);
	}

	public class DummyPushNotificationScheduler : IPushNotificationScheduler
	{
		readonly ILogger<DummyPushNotificationScheduler> Logger;

		public DummyPushNotificationScheduler(ILogger<DummyPushNotificationScheduler> logger) 
		{
			this.Logger = logger;	
		}

		public System.Threading.Tasks.Task SchedulePushNotifications(Task task)
		{
			Logger.LogInformation(0, "SchedulePushNotifications invoked");
			return System.Threading.Tasks.Task.FromResult(0);
		}
	}

	public class OneSignalPushNotificationScheduler : IPushNotificationScheduler
	{
		readonly IToDoRepository Repository;
		readonly ILogger<OneSignalPushNotificationScheduler> Logger;

		public OneSignalPushNotificationScheduler(IToDoRepository repository, ILogger<OneSignalPushNotificationScheduler> logger)
		{
			this.Repository = repository;
			this.Logger = logger;
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

		private async System.Threading.Tasks.Task DeletePushNotification(PushNotification notification)
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
			catch (WebException ex)
			{
				Logger.LogError(0,
								ex,
								"Error while deleting push notification in OneSignal with response {0}",
								new StreamReader(ex.Response.GetResponseStream()).ReadToEnd());
			}
			catch(Exception ex)
			{
				Logger.LogError(0, ex, "Error while deleting push notification in OneSignal with response {0}");
			}

			Repository.DeletePushNotification(notification);
		}

		private async System.Threading.Tasks.Task CreatePushNotification(Task task)
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
				headings = new { en = "Task notification" },
				content_available = true,
				mutable_content = true,
				filters = new[] { new { field = "tag", key = "user_id", relation = "=", value = Repository.User(task.UserUuid).ProviderId } },
				send_after = notificationDate.Value.ToString("yyyy-MM-dd HH:mm:ss 'GMT'zzzz")
			});

			var byteArray = Encoding.UTF8.GetBytes(sendJson.ToString());

			try
			{
				//throw new Exception("OLOLO error");
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

						Repository.CreatePushNotification(notification);
					}
				}
			}
			catch (WebException ex)
			{				
				Logger.LogError(0, 
				                ex, 
				                "Error while creating push notification in OneSignal with response {0}", 
				                new StreamReader(ex.Response.GetResponseStream()).ReadToEnd());
			}
			catch(Exception ex) 
			{
				Logger.LogError(0, ex, "Error while creating push notification in OneSignal");
			}
		}
	}
}
