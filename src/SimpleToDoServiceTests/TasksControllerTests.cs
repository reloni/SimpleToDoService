using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SimpleToDoService.Common;
using SimpleToDoService.Controllers;
using SimpleToDoService.Entities;
using SimpleToDoService.Repository;
using Xunit;

namespace SimpleToDoServiceTests
{
	public class TasksControllerTests
	{
		[Fact]
		public void TestGetTasksForUser()
		{
			var userGuid = Guid.NewGuid();
			var user = Utils.NewUser(userGuid);
			user.AddTask("test task 1");
			user.AddTask("test task 2");

			var user2 = Utils.NewUser();
			user2.AddTask("other 1");
			user2.AddTask("other 2");

			var repo = new MockToDoRepository(new List<User>() { user, user2 });
			var controller = new TasksController(repo, new MockPushNotificationScheduler(repo));
			controller.ControllerContext = new ControllerContext();
			controller.ControllerContext.HttpContext = new DefaultHttpContext();
			controller.ControllerContext.HttpContext.Items.Add("UserUuid", userGuid);

			var result = controller.Get(null);
			Assert.Equal(21, result.Count());
		}

		[Fact]
		public void TestGetTaskByGuid()
		{
			var userGuid = Guid.NewGuid();
			var user = Utils.NewUser(userGuid);
			var expectedGuid = Guid.NewGuid();
			user.AddTask("test task 1");
			user.AddTask("test task 2", expectedGuid);

			var user2 = Utils.NewUser();
			user2.AddTask("other 1");
			user2.AddTask("other 2");

			var repo = new MockToDoRepository(new List<User>() { user, user2 });
			var controller = new TasksController(repo, new MockPushNotificationScheduler(repo));
			controller.ControllerContext = new ControllerContext();
			controller.ControllerContext.HttpContext = new DefaultHttpContext();
			controller.ControllerContext.HttpContext.Items.Add("UserUuid", userGuid);

			var result = controller.Get(expectedGuid);
			Assert.Equal(1, result.Count());
			Assert.Equal(expectedGuid, result.FirstOrDefault()?.Uuid);
			Assert.Equal(userGuid, result.FirstOrDefault()?.User.Uuid);
			Assert.Equal(userGuid, result.FirstOrDefault()?.UserUuid);
			Assert.Equal("test task 2", result.FirstOrDefault()?.Description);
		}

		[Fact]
		public async void TestBatchUpdate_Empty()
		{
			var userGuid = Guid.NewGuid();
			var user = Utils.NewUser(userGuid);
			user.AddTask("test task 1");
			user.AddTask("test task 2");

			var user2 = Utils.NewUser();
			user2.AddTask("other 1");
			user2.AddTask("other 2");

			var repo = new MockToDoRepository(new List<User>() { user, user2 });
			var controller = new TasksController(repo, new MockPushNotificationScheduler(repo));
			controller.ControllerContext = new ControllerContext();
			controller.ControllerContext.HttpContext = new DefaultHttpContext();
			controller.ControllerContext.HttpContext.Items.Add("UserUuid", userGuid);

			var instruction = new BatchUpdateInstruction() { ToCreate = new List<Task>(), ToUpdate = new List<Task>(), ToDelete = new List<Guid>() };
			var result = await controller.BatchUpdate(instruction) as OkObjectResult;

			Assert.NotNull(result);

			var objects = (result.Value as IEnumerable<Task>).ToList();
			Assert.NotNull(objects);
			Assert.Equal(2, objects?.Count());
			Assert.Equal(user.Tasks, objects);
		}

		[Fact]
		public async void TestBatchUpdate_Create()
		{
			var userGuid = Guid.NewGuid();
			var user = Utils.NewUser(userGuid);
			user.AddTask("test task 1");
			user.AddTask("test task 2");

			var user2 = Utils.NewUser();
			user2.AddTask("other 1");
			user2.AddTask("other 2");

			var repo = new MockToDoRepository(new List<User>() { user, user2 });
			var controller = new TasksController(repo, new MockPushNotificationScheduler(repo));
			controller.ControllerContext = new ControllerContext();
			controller.ControllerContext.HttpContext = new DefaultHttpContext();
			controller.ControllerContext.HttpContext.Items.Add("UserUuid", userGuid);

			var instruction = new BatchUpdateInstruction() {
				ToCreate = new List<Task>() { new Task() { Uuid = Guid.NewGuid(), Description = "Created task" } },
				ToUpdate = new List<Task>(),
				ToDelete = new List<Guid>()
			};

			var result = await controller.BatchUpdate(instruction) as OkObjectResult;

			Assert.NotNull(result);

			Assert.Equal(3, user.Tasks.Count());

			var objects = (result.Value as IEnumerable<Task>).ToList();
			Assert.NotNull(objects);
			Assert.Equal(3, objects?.Count());
			Assert.Equal(user.Tasks, objects);
		}

		[Fact]
		public async void TestBatchUpdate_Update()
		{
			var userGuid = Guid.NewGuid();
			var user = Utils.NewUser(userGuid);
			var updateTaskGuid = Guid.NewGuid();
			user.AddTask("test task 1", updateTaskGuid);
			user.AddTask("test task 2");

			var user2 = Utils.NewUser();
			user2.AddTask("other 1");
			user2.AddTask("other 2");

			var repo = new MockToDoRepository(new List<User>() { user, user2 });
			var controller = new TasksController(repo, new MockPushNotificationScheduler(repo));
			controller.ControllerContext = new ControllerContext();
			controller.ControllerContext.HttpContext = new DefaultHttpContext();
			controller.ControllerContext.HttpContext.Items.Add("UserUuid", userGuid);

			var instruction = new BatchUpdateInstruction()
			{
				ToCreate = new List<Task>(),
				ToUpdate = new List<Task>() { new Task() { Uuid = updateTaskGuid, Description = "Updated task" } },
				ToDelete = new List<Guid>()
			};

			var result = await controller.BatchUpdate(instruction) as OkObjectResult;

			Assert.NotNull(result);

			Assert.Equal(2, user.Tasks.Count());
			Assert.Equal("Updated task", user.Tasks.Where(o => o.Uuid == updateTaskGuid).FirstOrDefault()?.Description);

			var objects = (result.Value as IEnumerable<Task>).ToList();
			Assert.NotNull(objects);
			Assert.Equal(2, objects?.Count());
			Assert.Equal(user.Tasks, objects);
		}

		[Fact]
		public async void TestBatchUpdate_Delete()
		{
			var userGuid = Guid.NewGuid();
			var user = Utils.NewUser(userGuid);
			var deleteTaskGuid = Guid.NewGuid();
			user.AddTask("test task 1", deleteTaskGuid);
			user.AddTask("test task 2");

			var user2 = Utils.NewUser();
			user2.AddTask("other 1");
			user2.AddTask("other 2");

			var repo = new MockToDoRepository(new List<User>() { user, user2 });
			var controller = new TasksController(repo, new MockPushNotificationScheduler(repo));
			controller.ControllerContext = new ControllerContext();
			controller.ControllerContext.HttpContext = new DefaultHttpContext();
			controller.ControllerContext.HttpContext.Items.Add("UserUuid", userGuid);

			var instruction = new BatchUpdateInstruction()
			{
				ToCreate = new List<Task>(),
				ToUpdate = new List<Task>(),
				ToDelete = new List<Guid>() { deleteTaskGuid }
			};

			var result = await controller.BatchUpdate(instruction) as OkObjectResult;

			Assert.NotNull(result);

			Assert.Equal(1, user.Tasks.Count());
			Assert.Null(user.Tasks.Where(o => o.Uuid == deleteTaskGuid).FirstOrDefault());

			var objects = (result.Value as IEnumerable<Task>).ToList();
			Assert.NotNull(objects);
			Assert.Equal(1, objects?.Count());
			Assert.Equal(user.Tasks, objects);
		}
	}
}
