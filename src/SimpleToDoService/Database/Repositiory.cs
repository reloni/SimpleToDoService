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
		ToDoEntry CreateEntry(ToDoEntry entry);
		ToDoEntry UpdateEntry(ToDoEntry entry);
		bool DeleteEntry(int id);
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

		public IEnumerable<User> Users()
		{
			return context.Users;
		}

		public ToDoEntry Entry(int userId, int entryId)
		{
			return context.ToDoEntries.Where(o => o.User.Id == userId && o.Id == entryId).FirstOrDefault();
		}

		public ToDoEntry CreateEntry(ToDoEntry entry)
		{
			entry.CreationDate = DateTime.Now.ToUniversalTime();
			var entity = context.ToDoEntries.Add(entry);

			if(context.SaveChanges() == 1)
				return entity.Entity;

			return null;
		}

		public ToDoEntry UpdateEntry(ToDoEntry entry)
		{
			var updated = context.UpdateToDoEntry(entry);
			context.Entry(entry).Property(p => p.CreationDate).IsModified = false;
			if(context.SaveChanges() == 1)
				return updated;

			return null;
		}

		public User User(int id)
		{
			return context.Users.Where(o => o.Id == id).FirstOrDefault();
		}

		public bool DeleteEntry(int id)
		{
			var entry = new ToDoEntry() { Id = id };
			context.ToDoEntries.Attach(entry);
			context.ToDoEntries.Remove(entry);
			try
			{
				context.SaveChanges();
			}
			catch (DbUpdateConcurrencyException)
			{
				// entity doesn't exists in this case
				return false;
			}
			return true;
		}
	}
}
