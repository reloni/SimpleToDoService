using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace SimpleToDoService.Entities
{
	[Table("todouser")]
	public class User
	{
		[Key]
		[Column("uuid")]
		[Required]
		public Guid Uuid { get; set; }

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
		[Column("uuid")]
		public Guid Uuid { get; set; }

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

		[XmlIgnore]
		[JsonIgnore]
		[IgnoreDataMember]
		[Column("creationdate")]
		public DateTime CreationDate { get; set; }

		[Column("targetdate")]
		public DateTime? TargetDate { get; set; }

		[XmlIgnore]
		[JsonIgnore]
		[IgnoreDataMember]
		[Column("owner")]
		[ForeignKey("User")]
		public Guid UserUuid { get; set; }

		[XmlIgnore]
		[JsonIgnore]
		[IgnoreDataMember]
		public User User { get; set; }
	}
}
