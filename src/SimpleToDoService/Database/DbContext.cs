using System;
using Microsoft.EntityFrameworkCore;
using SimpleToDoService.Entities;

namespace SimpleToDoService.Context
{
	public interface IToDoDbContext
	{
		DbSet<User> Users { get; }
		DbSet<ToDoEntry> ToDoEntries { get; }

		int SaveChanges();
	}

	public class ToDoDbContext : DbContext, IToDoDbContext
	{
		public ToDoDbContext(DbContextOptions<ToDoDbContext> options) : base(options) { }

		public DbSet<User> Users { get; set; }

		public DbSet<ToDoEntry> ToDoEntries { get; set; }
	}
}
