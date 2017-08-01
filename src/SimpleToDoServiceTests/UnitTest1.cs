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
	public class UnitTest1
	{
		[Fact]
		public void Test1()
		{
			Assert.Equal(1, 1);
		}

		[Fact]
		public void TestController()
		{
			var repo = new MockToDoRepository();
			var controller = new TasksController(repo);
			controller.ControllerContext = new ControllerContext();
			controller.ControllerContext.HttpContext = new DefaultHttpContext();
			controller.ControllerContext.HttpContext.Items.Add("UserUuid", Guid.NewGuid());
			var result = controller.Get(null);
			Assert.Equal(0, result.Count());
		}
	}
}

class MockToDoRepository : IToDoRepository
{
	public PushNotification CreatePushNotification(PushNotification notification)
	{
		throw new NotImplementedException();
	}

	public Task CreateTask(Task task)
	{
		throw new NotImplementedException();
	}

	public User CreateUser(User user)
	{
		throw new NotImplementedException();
	}

	public bool DeletePushNotification(PushNotification notification)
	{
		throw new NotImplementedException();
	}

	public bool DeleteTask(Guid uuid)
	{
		throw new NotImplementedException();
	}

	public bool DeleteTask(Task task)
	{
		throw new NotImplementedException();
	}

	public bool DeleteUser(User user)
	{
		throw new NotImplementedException();
	}

	public void DetachTask(Task task)
	{
		throw new NotImplementedException();
	}

	public IEnumerable<PushNotification> PushNotifications(Task task)
	{
		throw new NotImplementedException();
	}

	public Task ReloadTask(Task task)
	{
		throw new NotImplementedException();
	}

	public Task Task(Guid userUuid, Guid taskUuid)
	{
		throw new NotImplementedException();
	}

	public IEnumerable<Task> Tasks(Guid userUuid)
	{
		return Enumerable.Empty<Task>();
	}

	public Task UpdateTask(Task task)
	{
		throw new NotImplementedException();
	}

	public User UpdateUser(User user)
	{
		throw new NotImplementedException();
	}

	public User User(Guid uuid)
	{
		throw new NotImplementedException();
	}

	public IEnumerable<User> Users()
	{
		throw new NotImplementedException();
	}
}