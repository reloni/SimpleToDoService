using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SimpleToDoService.DB;

namespace SimpleToDoServiceTests
{
	class Utils
	{
		public static DbContextOptions<ToDoDbContext> InMemoryContextOptions(string dbName) 
		{
			return new DbContextOptionsBuilder<ToDoDbContext>()
					  .UseInMemoryDatabase(dbName)
					  .Options;
		}

		public static ToDoDbContext InMemoryContext()
		{
			var options = new DbContextOptionsBuilder<ToDoDbContext>()
					  .UseInMemoryDatabase(Guid.NewGuid().ToString())
					  .Options;
			return new ToDoDbContext(options);
		}
	}

	static class Extensions
	{
		public static void AddTestData(this ToDoDbContext context)
		{
			context.Add(new User() { ProviderId = "TestUser1", Uuid = Guid.NewGuid() });
			context.Add(new User() { ProviderId = "TestUser2", Uuid = Guid.NewGuid() });

			context.TaskPrototypes.Add(new TaskPrototype() { CronExpression = "", Uuid = Guid.NewGuid() });
			context.TaskPrototypes.Add(new TaskPrototype() { CronExpression = "", Uuid = Guid.NewGuid() });
			context.TaskPrototypes.Add(new TaskPrototype() { CronExpression = "", Uuid = Guid.NewGuid() });
			context.TaskPrototypes.Add(new TaskPrototype() { CronExpression = "", Uuid = Guid.NewGuid() });

			context.SaveChanges();

			context.Add(new Task()
			{
				Uuid = Guid.NewGuid(),
				Completed = false,
				CreationDate = DateTime.Now,
				UserUuid = context.Users.First().Uuid,
				Description = "Task 1",
				TaskPrototypeUuid = context.TaskPrototypes.First().Uuid
			});
			context.Add(new Task()
			{
				Uuid = Guid.NewGuid(),
				Completed = false,
				CreationDate = DateTime.Now,
				UserUuid = context.Users.First().Uuid,
				Description = "Task 2",
				TaskPrototypeUuid = context.TaskPrototypes.Skip(1).First().Uuid
			});

			context.Add(new Task()
			{
				Uuid = Guid.NewGuid(),
				Completed = false,
				CreationDate = DateTime.Now,
				User = context.Users.Skip(1).First(),
				Description = "Task 3",
				TaskPrototypeUuid = context.TaskPrototypes.Skip(2).First().Uuid
			});
			context.Add(new Task()
			{
				Uuid = Guid.NewGuid(),
				Completed = false,
				CreationDate = DateTime.Now,
				User = context.Users.Skip(1).First(),
				Description = "Task 4",
				TaskPrototypeUuid = context.TaskPrototypes.Skip(3).First().Uuid
			});

			context.SaveChanges();
		}
	}
}