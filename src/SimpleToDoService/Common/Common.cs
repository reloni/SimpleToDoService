using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SimpleToDoService.Entities;

namespace SimpleToDoService.Common
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
		
		return task.TargetDate;
	}
}