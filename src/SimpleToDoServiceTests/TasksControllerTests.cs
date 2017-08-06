using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
			var controller = new TasksController(repo);
			controller.ControllerContext = new ControllerContext();
			controller.ControllerContext.HttpContext = new DefaultHttpContext();
			controller.ControllerContext.HttpContext.Items.Add("UserUuid", userGuid);

			var result = controller.Get(null);
			Assert.Equal(2, result.Count());
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
			var controller = new TasksController(repo);
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
	}
}