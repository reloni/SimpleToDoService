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

			context.SaveChanges();

			context.Add(new Task() { Uuid = Guid.NewGuid(), Completed = false, UserUuid = context.Users.First().Uuid, Description = "Task 1" });
			context.Add(new Task() { Uuid = Guid.NewGuid(), Completed = false, UserUuid = context.Users.First().Uuid, Description = "Task 2" });

			context.Add(new Task() { Uuid = Guid.NewGuid(), Completed = false, User = context.Users.Skip(1).First(), Description = "Task 3" });
			context.Add(new Task() { Uuid = Guid.NewGuid(), Completed = false, User = context.Users.Skip(1).First(), Description = "Task 4" });

			context.SaveChanges();

			return context;
		}

		[Fact]
		public void TestRepo()
		{
			var context = ContextWithData();

			var repo = new ToDoRepository(context);

			//var guid = context.Users.First().Uuid;
			//var task = 
			var result = context.Tasks.Include(o => o.Prototype).Where(o => o.UserUuid == context.Users.First().Uuid);
			//Assert.Equal(2, context.Tasks.Include(o => o.Prototype).Where(o => o.UserUuid == context.Users.First().Uuid).Count());
			Assert.Equal(2, result.Count());
			//Assert.Equal(2, repo.Tasks(context.Users.First().Uuid).Count());
		}

		[Fact]
		public void GetTaskByGuid()
		{
			var context = ContextWithData();

			var repo = new ToDoRepository(context) as IToDoRepository;

			var controller = new TasksController(repo, new MockPushNotificationScheduler(repo), null);
			controller.ControllerContext = new ControllerContext();
			controller.ControllerContext.HttpContext = new DefaultHttpContext();

			var dbUser = context.Users.Include(o => o.Tasks).Skip(1).First();

			controller.ControllerContext.HttpContext.Items.Add("UserUuid", dbUser.Uuid);

			var taskGuid = dbUser.Tasks.First().Uuid;
			var result = controller.Get(taskGuid) as OkObjectResult;

			Assert.NotNull(result);

			var task = (result?.Value as Task);
			Assert.Equal(taskGuid, task?.Uuid);
			Assert.Equal(dbUser.Uuid, task?.User.Uuid);
			Assert.Equal(dbUser.Uuid, task?.UserUuid);
			Assert.Equal("Task 2", task?.Description);
		}
	}
}
