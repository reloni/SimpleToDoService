using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SimpleToDoService.Entities;

namespace SimpleToDoService
{
	public class BatchUpdateInstruction
	{
		[Required]
		public IEnumerable<Task> ToUpdate { get; set; }
		[Required]
		public IEnumerable<Guid> ToDelete { get; set; }
		[Required]
		public IEnumerable<Task> ToCreate { get; set; }
	}

	public class ServiceError
	{
		public string Message { get; set; }
	}
}

static class Extensions 
{
	public static DateTime? CheckedTargetDate(this Task task)
	{
		if (!task.TargetDate.HasValue)
			return null;

		var notificationDate = task.TargetDate.Value.ToUniversalTime();

		if (!task.TargetDateIncludeTime)
			return new DateTime(notificationDate.Year, notificationDate.Month, notificationDate.Day, 0, 0, 0, notificationDate.Kind);

		return task.TargetDate;
	}
}