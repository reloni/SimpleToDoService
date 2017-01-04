using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SimpleToDoService.Entities;

namespace SimpleToDoService.Context
{
	public interface IToDoDbContext
	{
		DbSet<User> Users { get; }
		DbSet<ToDoEntry> ToDoEntries { get; }

		int SaveChanges();

		ToDoEntry UpdateToDoEntry(ToDoEntry entry);
		EntityEntry<TEntity> Entry<TEntity>(TEntity entry) where TEntity : class;
	}

	public class ToDoDbContext : DbContext, IToDoDbContext
	{
		public ToDoDbContext(DbContextOptions<ToDoDbContext> options) : base(options) { }

		public DbSet<User> Users { get; set; }

		public DbSet<ToDoEntry> ToDoEntries { get; set; }

		public ToDoEntry UpdateToDoEntry(ToDoEntry entry)
		{
			if(ToDoEntries.Where(o => o.Id == entry.Id).Count() == 1)
				return Update(entry).Entity;

			return null;
		}
	}
}
