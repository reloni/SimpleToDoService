using SimpleToDoService.Common;
using SimpleToDoService.DB;

namespace SimpleToDoServiceTests
{
	public class MockPushNotificationScheduler : IPushNotificationScheduler
	{
		private readonly IToDoRepository repo;

		public MockPushNotificationScheduler(IToDoRepository repository)
		{
			this.repo = repository;
		}

		public System.Threading.Tasks.Task SchedulePushNotifications(SimpleToDoService.DB.Task task)
		{
			return System.Threading.Tasks.Task.FromResult(0);
		}
	}
}
