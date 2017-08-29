using System;
using Microsoft.EntityFrameworkCore;
using SimpleToDoService.DB;
using Xunit;
using System.Linq;
using SimpleToDoService.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace SimpleToDoServiceTests
{
	public class InMemoryDBTests
	{
		ToDoDbContext ContextWithData()
		{
			var options = new DbContextOptionsBuilder<ToDoDbContext>()
					  .UseInMemoryDatabase(Guid.NewGuid().ToString())
					  .Options;
			var context = new ToDoDbContext(options);


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

			return context;
		}

		//[Fact]
		//public void TestRepo()
		//{
		//	var context = ContextWithData();

		//	var repo = new ToDoRepository(context) as IToDoRepository;

		//	//var guid = context.Users.First().Uuid;
		//	//var task = 
		//	//var result = context.Tasks.Include(o => o.Prototype).Where(o => o.UserUuid == context.Users.First().Uuid);
		//	//Assert.Equal(2, context.Tasks.Include(o => o.Prototype).Where(o => o.UserUuid == context.Users.First().Uuid).Count());
		//	//Assert.Equal(2, result.Count());
		//	//var t = context.Tasks.Where(o => o.UserUuid == context.Users.First().Uuid).OrderBy(o => o.CreationDate).Where(o => !o.Completed);

		//	//var result = context.Tasks.Where(o => o.UserUuid == context.Users.First().Uuid).OrderBy(o => o.CreationDate).Where(o => !o.Completed).Count();
		//	var dbUser = context.Users.Include(o => o.Tasks).First();
		//	//var result = repo.Tasks(dbUser.Uuid).OrderBy(o => o.CreationDate).Where(o => !o.Completed).FirstOrDefault().Description;
		//	var result = context.Tasks
		//	                    .Include(o => o.Prototype)
		//	                    .Where(o => o.UserUuid == context.Users.First().Uuid)
		//	                    .OrderBy(o => o.CreationDate)
		//	                    .Where(o => !o.Completed)
		//	                    .First()
		//	                    .Description;
		//	//Assert.Equal(2, repo.Tasks(context.Users.First().Uuid).Count());
		//	//Assert.Equal(2, result);
		//	Assert.Equal("Task 1", result);
		//}

		[Fact]
		public void GetTaskByGuid()
		{
			var context = ContextWithData();

			var repo = new ToDoRepository(context);

			var controller = new TasksController(repo, new MockPushNotificationScheduler(repo), null);
			controller.ControllerContext = new ControllerContext();
			controller.ControllerContext.HttpContext = new DefaultHttpContext();

			var dbUser = context.Users.Where(o => o.ProviderId == "TestUser2").Include(o => o.Tasks).First();

			controller.ControllerContext.HttpContext.Items.Add("UserUuid", dbUser.Uuid);

			var taskGuid = dbUser.Tasks.Skip(1).First().Uuid;
			var count = repo.Tasks(dbUser.Uuid).OrderBy(o => o.CreationDate).Where(o => !o.Completed).Count();
			var result = controller.Get(taskGuid) as OkObjectResult;

			Assert.NotNull(result);

			var task = (result?.Value as Task);
			Assert.Equal(taskGuid, task?.Uuid);
			Assert.Equal(dbUser.Uuid, task?.User.Uuid);
			Assert.Equal(dbUser.Uuid, task?.UserUuid);
			Assert.Equal("Task 4", task?.Description);
		}
	}
}
