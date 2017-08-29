using System;
using System.Collections.Generic;
using System.Linq;
using SimpleToDoService.DB;

namespace SimpleToDoServiceTests
{
class MockToDoRepository : IToDoRepository
	{
		List<User> _users { get; set; }

		public MockToDoRepository(List<User> users)
		{
			_users = users;
		}

		private User GetUser(Guid uuid)
		{
			var user = _users.Where(o => o.Uuid == uuid).FirstOrDefault();
			if (user == null)
				throw new Exception($"User with uid {uuid.ToString()} not found");
			return user;
		}

		public PushNotification CreatePushNotification(PushNotification notification)
		{
			throw new NotImplementedException();
		}

		public Task CreateTask(Task task)
		{
			var user = GetUser(task.UserUuid);

			var newTask = task.Copy();
			if (newTask.Uuid == null)
				newTask.Uuid = new Guid();
			newTask.CreationDate = DateTime.Now;
			newTask.PushNotifications = new List<PushNotification>();
			(user.Tasks as List<Task>).Add(newTask);
			return newTask;
		}

		public User CreateUser(User user)
		{
			throw new NotImplementedException();
		}

		public bool DeletePushNotification(PushNotification notification)
		{
			throw new NotImplementedException();
		}

		public bool DeleteTask(Task task)
		{
			var user = GetUser(task.UserUuid);
			var toDelete = user.Tasks.Where(o => o.Uuid == task.Uuid).FirstOrDefault();
			if (toDelete == null)
				return false;
			return (user.Tasks as List<Task>).Remove(toDelete);
		}

		public bool DeleteUser(User user)
		{
			throw new NotImplementedException();
		}

		public void DetachTask(Task task)
		{

		}

		public IEnumerable<PushNotification> PushNotifications(Task task)
		{
			throw new NotImplementedException();
		}

		public Task ReloadTask(Task task)
		{
			return task;
		}

		public Task Task(Guid userUuid, Guid taskUuid)
		{
			return _users.Where(o => o.Uuid == userUuid).FirstOrDefault()?.Tasks.Where(o => o.Uuid == taskUuid).FirstOrDefault();
		}

		public IEnumerable<Task> Tasks(Guid userUuid)
		{
			return _users.FirstOrDefault(o => o.Uuid == userUuid)?.Tasks ?? Enumerable.Empty<Task>();
		}

		public Task UpdateTask(Task task)
		{
			var user = GetUser(task.UserUuid);

			var toUpdate = user.Tasks.Where(o => o.Uuid == task.Uuid).FirstOrDefault();
			if (toUpdate == null)
				throw new Exception($"Task with uid {task.Uuid.ToString()} not found");

			toUpdate.Completed = task.Completed;
			toUpdate.Description = task.Description;
			toUpdate.Notes = task.Notes;
			toUpdate.TargetDate = task.TargetDate;
			toUpdate.TargetDateIncludeTime = task.TargetDateIncludeTime;

			return toUpdate;
		}

		public User UpdateUser(User user)
		{
			throw new NotImplementedException();
		}

		public User User(Guid uuid)
		{
			throw new NotImplementedException();
		}

		public User UserByProviderId(string providerId) 
		{
			throw new NotImplementedException();
		}

		public IEnumerable<User> Users()
		{
			throw new NotImplementedException();
		}

		public void DeleteTaskPrototypeIfNoUncompletedChildren(Guid uuid)
		{
			throw new NotImplementedException();
		}
	}
}
