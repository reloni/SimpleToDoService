﻿using System;
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
		ToDoEntry Entry(int userId, Guid entryUuid);
		ToDoEntry CreateEntry(ToDoEntry entry);
		ToDoEntry UpdateEntry(ToDoEntry entry);
		bool DeleteEntry(Guid uuid);
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

		public ToDoEntry Entry(int userId, Guid entryUuid)
		{
			return context.ToDoEntries.Where(o => o.User.Id == userId && o.Uuid == entryUuid).FirstOrDefault();
		}

		public ToDoEntry CreateEntry(ToDoEntry entry)
		{
			entry.Uuid = new Guid();
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

		public bool DeleteEntry(Guid uuid)
		{
			var entry = new ToDoEntry() { Uuid = uuid };
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
