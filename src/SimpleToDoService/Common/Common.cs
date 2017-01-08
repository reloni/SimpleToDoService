using System;
namespace SimpleToDoService
{
	class EmailExistedException : Exception { }

	public class ServiceError
	{
		public string Message { get; set; }
	}
}
