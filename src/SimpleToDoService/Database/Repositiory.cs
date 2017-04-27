using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SimpleToDoService.Entities;
using System.Linq;
using SimpleToDoService.Context;

namespace SimpleToDoService.Repository
{
	public interface IToDoRepository
	{
		IEnumerable<User> Users();
		User User(Guid uuid);
		IEnumerable<Task> Tasks(Guid userUuid);
		Task Task(Guid userUuid, Guid taskUuid);
		Task CreateTask(Task task);
		Task UpdateTask(Task task);
		bool DeleteTask(Guid uuid);
		bool DeleteTask(Task task);
		User CreateUser(User user);
		bool DeleteUser(User user);
		User UpdateUser(User user);
		PushNotification CreatePushNotification(PushNotification notification);
		IEnumerable<PushNotification> PushNotifications(Task task);
		bool DeletePushNotification(PushNotification notification);
	}

	public class ToDoRepository : IToDoRepository
	{
		private readonly IToDoDbContext context;

		public ToDoRepository(IToDoDbContext context)
		{
			this.context = context;
		}

		public IEnumerable<Task> Tasks(Guid userUuid)
		{
			return context.Tasks.Where(o => o.User.Uuid == userUuid)
				          .Include(o => o.PushNotifications);
		}

		public IEnumerable<User> Users()
		{
			return context.Users;
		}

		public Task Task(Guid userUuid, Guid entryUuid)
		{
			return context.Tasks.Where(o => o.User.Uuid == userUuid && o.Uuid == entryUuid)
				          .Include(o => o.PushNotifications)
				          .FirstOrDefault();
		}

		public Task CreateTask(Task task)
		{
			task.Uuid = new Guid();
			task.CreationDate = DateTime.Now.ToUniversalTime();
			task.PushNotifications = new List<PushNotification>();
			var entity = context.Tasks.Add(task);

			if(context.SaveChanges() == 1)
				return entity.Entity;

			return null;
		}

		public Task UpdateTask(Task task)
		{
			var updated = context.UpdateTask(task);
			context.Entry(task).Property(p => p.CreationDate).IsModified = false;
			if(context.SaveChanges() == 1)
				return updated;

			return null;
		}

		public User User(Guid uuid)
		{
			return context.Users.Where(o => o.Uuid == uuid).FirstOrDefault();
		}

		public bool DeleteTask(Guid uuid)
		{
			var task = new Task() { Uuid = uuid };
			context.Tasks.Attach(task);
			context.Tasks.Remove(task);
			try
			{
				context.SaveChanges();
			}
			catch (DbUpdateConcurrencyException)
			{
				// entity doesn't exists in this case
				return false;
			}
			return true;
		}

		public bool DeleteTask(Task task)
		{
			context.Tasks.Remove(task);
			try
			{
				context.SaveChanges();
			}
			catch (DbUpdateConcurrencyException)
			{
				// entity doesn't exists in this case
				return false;
			}
			return true;
		}

		public User CreateUser(User user)
		{
			user.Uuid = new Guid();
			user.CreationDate = DateTime.Now.ToUniversalTime();
			var entity = context.Users.Add(user);

			context.SaveChanges();
			return entity.Entity;
		}

		public bool DeleteUser(User user)
		{
			context.Users.Remove(user);

			try
			{
				context.SaveChanges();
			}
			catch
			{
				return false;
			}

			return true;
		}

		public User UpdateUser(User user)
		{
			var updated = context.UpdateUser(user);
			context.Entry(user).Property(p => p.CreationDate).IsModified = false;
			if (context.SaveChanges() == 1)
				return updated;

			return null;
		}

		public PushNotification CreatePushNotification(PushNotification notification)
		{
			notification.Uuid = new Guid();

			var entity = context.PushNotifications.Add(notification);

			if (context.SaveChanges() == 1)
				return entity.Entity;

			return null;
		}

		public IEnumerable<PushNotification> PushNotifications(Task task)
		{
			return context.PushNotifications.Where(o => o.TaskUuid == task.Uuid);

		}

		public bool DeletePushNotification(PushNotification notification)
		{
			context.PushNotifications.Remove(notification);

			try
			{
				context.SaveChanges();
			}
			catch
			{
				return false;
			}

			return true;
		}
	}
}
