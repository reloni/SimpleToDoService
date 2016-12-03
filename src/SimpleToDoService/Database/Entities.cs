using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimpleToDoService.Entities
{
	[Table("todouser")]
	public class User
	{
		[Key]
		[Column("id")]
		[Required]
		public int Id { get; set; }

		[MaxLength(255)]
		[Column("firstname")]
		[Required]
		public string FirstName { get; set; }

		[MaxLength(255)]
		[Column("lastname")]
		[Required]
		public string LastName { get; set; }

		[MaxLength(255)]
		[Column("email")]
		[Required]
		public string Email { get; set; }

		[MaxLength(255)]
		[Column("password")]
		[Required]
		public string Password { get; set; }
	}


	[Table("todoentry")]
	public class ToDoEntry
	{
		[Key]
		[Column("id")]
		[Required]
		public int Id { get; set; }

		[Column("completed")]
		[Required]
		public bool Completed { get; set; }

		[Column("description")]
		[Required]
		[MaxLength(255)]
		public string Description { get; set; }

		[Column("notes")]
		[MaxLength(4000)]
		public string Notes { get; set; }

		[ForeignKey("owner")]
		[Required]
		public User User { get; set; }
	}
}
