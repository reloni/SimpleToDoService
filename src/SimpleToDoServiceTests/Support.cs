using System;
using System.Collections.Generic;
using System.Linq;
using SimpleToDoService.Entities;

namespace SimpleToDoServiceTests
{
	class Utils
	{
		public static User NewUser(Guid? guid = null, string providerId = "", string email = "")
		{
			return new User() { Uuid = guid ?? Guid.NewGuid(), ProviderId = providerId, Email = email, CreationDate = DateTime.Now, Tasks = new List<Task>() };
		}
	}

	static class Extensions
	{
		public static Task Copy(this Task task) 
		{
			return new Task()
			{ 
				Completed = task.Completed,
				CreationDate = task.CreationDate,
				Description = task.Description,
				Notes = task.Notes,
				PushNotifications = null,
				UserUuid = task.UserUuid,
				Uuid = task.Uuid,
				TargetDate = task.TargetDate,
				TargetDateIncludeTime = task.TargetDateIncludeTime,
				User = task.User
			};
		}

		public static void AddTask(this User user, string description, Guid? guid = null, bool completed = false, string notes = "", DateTime? targetDate = null, bool targetDateIncludeTime = true)
		{
			(user.Tasks as List<Task>).Add(new Task()
			{
				Uuid = guid ?? Guid.NewGuid(),
				CreationDate = DateTime.Now,
				User = user,
				UserUuid = user.Uuid,
				Completed = completed,
				Description = description,
				Notes = notes,
				TargetDate = targetDate,
				TargetDateIncludeTime = targetDateIncludeTime,
				PushNotifications = Enumerable.Empty<PushNotification>()
			});
		}
	}
}