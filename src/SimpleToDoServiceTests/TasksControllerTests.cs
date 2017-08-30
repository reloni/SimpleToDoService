using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SimpleToDoService.Common;
using SimpleToDoService.Controllers;
using SimpleToDoService.DB;
using Xunit;
using Microsoft.EntityFrameworkCore;

namespace SimpleToDoServiceTests
{
	public class TasksControllerTests
	{
		
		[Fact]
		public void TestGetTasksForUser()
		{
			var context = Utils.InMemoryContext();
			context.AddTestData();
			var repo = new ToDoRepository(context);
			var controller = new TasksController(repo, new MockPushNotificationScheduler(repo), null);
			controller.ControllerContext = new ControllerContext();
			controller.ControllerContext.HttpContext = new DefaultHttpContext();
			controller.ControllerContext.HttpContext.Items.Add("UserUuid", context.Users.First().Uuid);

			var result = controller.Get(null) as OkObjectResult;
			Assert.NotNull(result);
			Assert.Equal(2, (result.Value as IEnumerable<Task>)?.Count());
		}

		[Fact]
		public void TestGetTaskByGuid()
		{
			var context = Utils.InMemoryContext();
			context.AddTestData();
			var repo = new ToDoRepository(context);

			var userGuid = context.Users.Where(o => o.ProviderId == "TestUser2").First().Uuid;
			var expectedTask = context.Users.Include(o => o.Tasks).Where(o => o.ProviderId == "TestUser2").First().Tasks.Skip(1).First();

			var controller = new TasksController(repo, new MockPushNotificationScheduler(repo), null);
			controller.ControllerContext = new ControllerContext();
			controller.ControllerContext.HttpContext = new DefaultHttpContext();
			controller.ControllerContext.HttpContext.Items.Add("UserUuid", userGuid);

			var result = controller.Get(expectedTask.Uuid) as OkObjectResult;

			Assert.NotNull(result);

			var task = (result?.Value as Task);
			Assert.Equal(expectedTask, task);
			Assert.Equal(expectedTask.Uuid, task?.Uuid);
			Assert.Equal(userGuid, task?.User.Uuid);
			Assert.Equal(userGuid, task?.UserUuid);
			Assert.Equal("Task 4", task?.Description);
		}

		[Fact]
		public void TestGetNotExistedTaskByGuid()
		{
			var context = Utils.InMemoryContext();
			context.AddTestData();
			var repo = new ToDoRepository(context);
			var userGuid = context.Users.Where(o => o.ProviderId == "TestUser2").First().Uuid;

			var controller = new TasksController(repo, new MockPushNotificationScheduler(repo), null);
			controller.ControllerContext = new ControllerContext();
			controller.ControllerContext.HttpContext = new DefaultHttpContext();
			controller.ControllerContext.HttpContext.Items.Add("UserUuid", userGuid);

			var result = controller.Get(Guid.NewGuid());
			Assert.IsType<NotFoundResult>(result);
		}

		[Fact]
		public async System.Threading.Tasks.Task TestBatchUpdate_Empty()
		{
			var context = Utils.InMemoryContext();
			context.AddTestData();
			var repo = new ToDoRepository(context);
			var user = context.Users.Where(o => o.ProviderId == "TestUser2").First();

			var controller = new TasksController(repo, new MockPushNotificationScheduler(repo), null);
			controller.ControllerContext = new ControllerContext();
			controller.ControllerContext.HttpContext = new DefaultHttpContext();
			controller.ControllerContext.HttpContext.Items.Add("UserUuid", user.Uuid);

			var instruction = new BatchUpdateInstruction() { ToCreate = new List<Task>(), ToUpdate = new List<Task>(), ToDelete = new List<Guid>() };
			var result = await controller.BatchUpdate(instruction) as OkObjectResult;

			Assert.NotNull(result);

			var objects = (result.Value as IEnumerable<Task>).ToList();
			Assert.NotNull(objects);
			Assert.Equal(2, objects?.Count());
			Assert.Equal(user.Tasks, objects);
		}

		[Fact]
		public async System.Threading.Tasks.Task TestBatchUpdate_Create()
		{
			var context = Utils.InMemoryContext();
			context.AddTestData();
			var repo = new ToDoRepository(context);
			var user = context.Users.Where(o => o.ProviderId == "TestUser2").First();

			var controller = new TasksController(repo, new MockPushNotificationScheduler(repo), null);
			controller.ControllerContext = new ControllerContext();
			controller.ControllerContext.HttpContext = new DefaultHttpContext();
			controller.ControllerContext.HttpContext.Items.Add("UserUuid", user.Uuid);

			var taskToCreate = new Task() { Uuid = Guid.NewGuid(), Description = "Created task" };
			var instruction = new BatchUpdateInstruction() {
				ToCreate = new List<Task>() { taskToCreate },
				ToUpdate = new List<Task>(),
				ToDelete = new List<Guid>()
			};

			var result = await controller.BatchUpdate(instruction) as OkObjectResult;

			Assert.NotNull(result);

			Assert.Equal(3, user.Tasks.Count());

			var objects = (result.Value as IEnumerable<Task>).ToList();
			Assert.NotNull(objects);
			Assert.Equal(3, objects?.Count());
			Assert.True(context.Tasks.Where(o => o.Uuid == taskToCreate.Uuid).Count() == 1);
			Assert.True(context.TaskPrototypes.Count() == 5);
		}

		[Fact]
		public async System.Threading.Tasks.Task TestBatchUpdate_Update()
		{
			var options = Utils.InMemoryContextOptions(Guid.NewGuid().ToString());
			var context = new ToDoDbContext(options);
			context.AddTestData();

			var repo = new ToDoRepository(new ToDoDbContext(options));
			var user = context.Users.Where(o => o.ProviderId == "TestUser2").First();

			var controller = new TasksController(repo, new MockPushNotificationScheduler(repo), null);
			controller.ControllerContext = new ControllerContext();
			controller.ControllerContext.HttpContext = new DefaultHttpContext();
			controller.ControllerContext.HttpContext.Items.Add("UserUuid", user.Uuid);
			var state = context.Entry(user.Tasks.First()).State;
			var taskToUpdate = user.Tasks.First();
			taskToUpdate.Description = "Updated task";
			var instruction = new BatchUpdateInstruction()
			{
				ToCreate = new List<Task>(),
				ToUpdate = new List<Task>() { taskToUpdate },
				ToDelete = new List<Guid>()
			};

			var result = await controller.BatchUpdate(instruction) as OkObjectResult;

			Assert.NotNull(result);

			Assert.Equal("Updated task", context.Tasks.Where(o => o.Uuid == taskToUpdate.Uuid).FirstOrDefault()?.Description);
			Assert.Equal(4, context.Tasks.Count());

			var objects = (result.Value as IEnumerable<Task>).ToList();
			Assert.NotNull(objects);
			Assert.Equal(2, objects?.Count());
		}

		[Fact]
		public async System.Threading.Tasks.Task TestBatchUpdate_Delete()
		{
			var options = Utils.InMemoryContextOptions(Guid.NewGuid().ToString());
			var context = new ToDoDbContext(options);
			context.AddTestData();

			var repo = new ToDoRepository(new ToDoDbContext(options));
			var user = context.Users.Where(o => o.ProviderId == "TestUser2").First();

			var controller = new TasksController(repo, new MockPushNotificationScheduler(repo), null);
			controller.ControllerContext = new ControllerContext();
			controller.ControllerContext.HttpContext = new DefaultHttpContext();
			controller.ControllerContext.HttpContext.Items.Add("UserUuid", user.Uuid);

			var deleteUuid = user.Tasks.First().Uuid;
			var instruction = new BatchUpdateInstruction()
			{
				ToCreate = new List<Task>(),
				ToUpdate = new List<Task>(),
				ToDelete = new List<Guid>() { deleteUuid }
			};

			var result = await controller.BatchUpdate(instruction) as OkObjectResult;

			Assert.NotNull(result);

			Assert.Null(context.Tasks.Where(o => o.Uuid == deleteUuid).FirstOrDefault());
			Assert.Equal(3, context.Tasks.Count());
			Assert.Equal(3, context.TaskPrototypes.Count());

			var objects = (result.Value as IEnumerable<Task>).ToList();
			Assert.NotNull(objects);
			Assert.Equal(1, objects?.Count());
		}
	}
}
