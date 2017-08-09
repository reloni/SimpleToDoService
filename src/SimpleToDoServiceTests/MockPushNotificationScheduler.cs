using System;
using System.Threading.Tasks;
using SimpleToDoService.Common;
using SimpleToDoService.Entities;
using SimpleToDoService.Repository;

namespace SimpleToDoServiceTests
{
	public class MockPushNotificationScheduler : IPushNotificationScheduler
	{
		private readonly IToDoRepository repo;

		public MockPushNotificationScheduler(IToDoRepository repository)
		{
			this.repo = repository;
		}

		public System.Threading.Tasks.Task SchedulePushNotifications(SimpleToDoService.Entities.Task task)
		{
			return System.Threading.Tasks.Task.FromResult(0);
		}
	}
}
