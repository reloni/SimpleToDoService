using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
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

	public class LogOutParameters
	{
		[Required]
		public string RefreshToken { get; set; }
	}

	public class ServiceError
	{
		public string Message { get; set; }
	}

	public class ValidateModelAttribute : ActionFilterAttribute
	{
		public override void OnActionExecuting(ActionExecutingContext context)
		{
			if (!context.ModelState.IsValid)
			{
				context.Result = new BadRequestObjectResult(context.ModelState);
			}
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
}
