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
		User CreateUser(User user);
		bool DeleteUser(User user);
		User UpdateUser(User user);
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
			return context.Tasks.Where(o => o.User.Uuid == userUuid);
		}

		public IEnumerable<User> Users()
		{
			return context.Users;
		}

		public Task Task(Guid userUuid, Guid entryUuid)
		{
			return context.Tasks.Where(o => o.User.Uuid == userUuid && o.Uuid == entryUuid).FirstOrDefault();
		}

		public Task CreateTask(Task task)
		{
			task.Uuid = new Guid();
			task.CreationDate = DateTime.Now.ToUniversalTime();
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
	}
}
