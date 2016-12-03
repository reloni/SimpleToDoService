using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SimpleToDoService.Entities;
using System.Linq;
using SimpleToDoService.Context;

namespace SimpleToDoService.Repository
{
	public interface IToDoRepository
	{
		IEnumerable<User> Users();
		User User(int id);
		IEnumerable<ToDoEntry> Entries(int userId);
		ToDoEntry Entry(int userId, int entryId);
	}

	public class ToDoRepository : IToDoRepository
	{
		private readonly IToDoDbContext context;

		public ToDoRepository(IToDoDbContext context)
		{
			this.context = context;
		}

		public IEnumerable<ToDoEntry> Entries(int userId)
		{
			return context.ToDoEntries.Where(o => o.User.Id == userId);
		}

		public ToDoEntry Entry(int userId, int entryId)
		{
			return context.ToDoEntries.Where(o => o.User.Id == userId && o.Id == entryId).FirstOrDefault();
		}

		public User User(int id)
		{
			return context.Users.Where(o => o.Id == id).FirstOrDefault();
		}

		public IEnumerable<User> Users()
		{
			return context.Users;
		}
	}
}
