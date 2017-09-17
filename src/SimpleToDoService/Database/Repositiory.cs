using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace SimpleToDoService.DB
{
	public interface IToDoRepository
	{
		IQueryable<User> Users();
		User User(Guid uuid);
		User UserByProviderId(string providerId);
		IQueryable<Task> Tasks(Guid userUuid);
		Task Task(Guid userUuid, Guid taskUuid);
		Task ReloadTask(Task task);
		Task CreateTask(Task task);
		Task UpdateTask(Task task);
		void DetachTask(Task task);
		void DetachPrototype(TaskPrototype prototype);
		bool DeleteTask(Task task);
		User CreateUser(User user);
		bool DeleteUser(User user);
		User UpdateUser(User user);
		PushNotification CreatePushNotification(PushNotification notification);
		IQueryable<PushNotification> PushNotifications(Task task);
		bool DeletePushNotification(PushNotification notification);
	}

	public class ToDoRepository : IToDoRepository
	{
		private readonly IToDoDbContext context;

		public ToDoRepository(IToDoDbContext context)
		{
			this.context = context;
		}

		public IQueryable<Task> Tasks(Guid userUuid)
		{
			return context.Tasks.Where(o => o.User.Uuid == userUuid)
				          .Include(o => o.Prototype);
		}

		//public IQueryable<Task> Tasks2(Guid userUuid)
		//{
		//	return context.Tasks.Where(o => o.User.Uuid == userUuid)
		//				  .Include(o => o.Prototype);
		//}

		public IQueryable<User> Users()
		{
			return context.Users;
		}

		public Task Task(Guid userUuid, Guid entryUuid)
		{
			return context.Tasks.Where(o => o.User.Uuid == userUuid && o.Uuid == entryUuid)
				          .Include(o => o.PushNotifications)
				          .Include(o => o.Prototype)
				          .FirstOrDefault();
		}

		public Task ReloadTask(Task task)
		{
			DetachTask(task);
			return Task(task.UserUuid, task.Uuid);
		}

		public void DetachTask(Task task)
		{
			context.Entry(task).State = EntityState.Detached;
		}

		public void DetachPrototype(TaskPrototype prototype)
		{
			if (prototype != null)
				context.Entry(prototype).State = EntityState.Detached;
		}

		public Task CreateTask(Task task)
		{
			if (task.Uuid == null) 
				task.Uuid = new Guid();
			
			task.CreationDate = DateTime.Now.ToUniversalTime();
			task.PushNotifications = new List<PushNotification>();
			var prototype = context.TaskPrototypes.Find(task.Prototype.Uuid);
			if (prototype != null)
			{
				prototype.CronExpression = task.Prototype.CronExpression;
				task.Prototype = prototype;
			}

			var entity = context.Tasks.Add(task);

			var tasksToComplete = context.Tasks.Where(o => o.TaskPrototypeUuid == task.TaskPrototypeUuid && o.Completed == false).ToList();
			tasksToComplete.ForEach(o => o.Completed = true);

			try
			{
				if (context.SaveChanges() > 0)
				{
					tasksToComplete.ForEach(o => context.Entry(o).State = EntityState.Detached);
					return entity.Entity;
				}
			}
			catch(DbUpdateException ex)
			{
				var innerException = ex.InnerException as Npgsql.PostgresException;
				if (innerException != null && innerException.SqlState == "23505")
				{
					// generate new uuid if it's duplicated
					task.Uuid = Guid.NewGuid();

					DetachTask(entity.Entity);
					entity = context.Tasks.Add(task);

					if (context.SaveChanges() > 0)
					{
						tasksToComplete.ForEach(o => context.Entry(o).State = EntityState.Detached);
						return entity.Entity;
					}
				}
				else
				{
					throw;
				}
			}

			tasksToComplete.ForEach(o => context.Entry(o).State = EntityState.Detached);
			return null;
		}

		public Task UpdateTask(Task task)
		{
			var updated = context.UpdateTask(task);
			context.Entry(task).Property(p => p.CreationDate).IsModified = false;
			if(context.SaveChanges() > 0)
				return updated;

			return null;
		}

		public User User(Guid uuid)
		{
			return context.Users.Where(o => o.Uuid == uuid).FirstOrDefault();
		}

		public User UserByProviderId(string providerId)
		{
			return context.Users.Where(o => o.ProviderId == providerId).FirstOrDefault();
		}

		public bool DeleteTask(Task task)
		{
			context.Tasks.Remove(task);

			try
			{
				context.SaveChanges();

				// after task deletion delete task prototype if it has no children)
				if (task.Prototype != null)
				{
					DeleteTaskPrototypeIfEmpty(task.Prototype.Uuid);
					context.SaveChanges();
				}
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
			if (context.SaveChanges() > 0)
				return updated;

			return null;
		}

		public PushNotification CreatePushNotification(PushNotification notification)
		{
			notification.Uuid = new Guid();

			var entity = context.PushNotifications.Add(notification);

			if (context.SaveChanges() > 0)
				return entity.Entity;

			return null;
		}

		public IQueryable<PushNotification> PushNotifications(Task task)
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

		void DeleteTaskPrototypeIfEmpty(Guid uuid)
		{
			var prototype = context.TaskPrototypes.Where(o => o.Uuid == uuid).FirstOrDefault();
			if (prototype == null)
				return;
			
			if (context.Tasks.Where(o => o.TaskPrototypeUuid == uuid).Count() == 0)
			{
				context.TaskPrototypes.Remove(prototype);
				try
				{
					context.SaveChanges();
				}
				catch { }
			}
		}
	}
}
