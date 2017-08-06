using System;
using System.Collections.Generic;
using System.Linq;
using SimpleToDoService.Entities;
using SimpleToDoService.Repository;

namespace SimpleToDoServiceTests
{
class MockToDoRepository : IToDoRepository
	{
		List<User> _users { get; set; }

		public MockToDoRepository(List<User> users)
		{
			_users = users;
		}

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
			return _users.FirstOrDefault(o => o.Uuid == userUuid)?.Tasks ?? Enumerable.Empty<Task>();
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
}
