﻿using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace SimpleToDoService.DB
{
	public interface IToDoDbContext
	{
		DbSet<User> Users { get; }
		DbSet<Task> Tasks { get; }
		DbSet<TaskPrototype> TaskPrototypes { get; }
		DbSet<PushNotification> PushNotifications { get; }

		int SaveChanges();

		Task UpdateTask(Task entry);
		User UpdateUser(User user);
		EntityEntry<TEntity> Entry<TEntity>(TEntity entry) where TEntity : class;
	}

	public class ToDoDbContext : DbContext, IToDoDbContext
	{
		public ToDoDbContext(DbContextOptions<ToDoDbContext> options) : base(options) { }

		public DbSet<User> Users { get; set; }

		public DbSet<Task> Tasks { get; set; }

		public DbSet<TaskPrototype> TaskPrototypes { get; set; }

		public DbSet<PushNotification> PushNotifications { get; set; }

		public Task UpdateTask(Task task)
		{
			if(Tasks.Where(o => o.Uuid == task.Uuid).Count() == 1)
				return Update(task).Entity;

			return null;
		}

		public User UpdateUser(User user)
		{
			if (Users.Where(o => o.Uuid == user.Uuid).Count() == 1)
				return Update(user).Entity;

			return null;
		}
	}
}
