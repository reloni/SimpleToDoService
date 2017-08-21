using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace SimpleToDoService.Entities
{
	[Table("taskuser")]
	public class User
	{
		[Key]
		[Column("uuid")]
		[Required]
		public Guid Uuid { get; set; }

		[Column("providerid")]
		[XmlIgnore]
		[JsonIgnore]
		[IgnoreDataMember]
		public string ProviderId { get; set; }

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

		[XmlIgnore]
		[JsonIgnore]
		[IgnoreDataMember]
		[Column("creationdate")]
		public DateTime CreationDate { get; set; }

		[XmlIgnore]
		[JsonIgnore]
		[IgnoreDataMember]
		[ForeignKey("UserUuid")]
		public IEnumerable<Task> Tasks { get; set; }
	}

	[Table("task")]
	public class Task
	{
		[Key]
		[Column("uuid")]
		[Required]
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

		[Column("creationdate")]
		public DateTime CreationDate { get; set; }

		[Column("targetdate")]
		public DateTime? TargetDate { get; set; }

		[Column("targetdateincludetime")]
		public bool TargetDateIncludeTime { get; set; } = true;

		[XmlIgnore]
		[JsonIgnore]
		[IgnoreDataMember]
		[Column("owner")]
		public Guid UserUuid { get; set; }

		[XmlIgnore]
		[JsonIgnore]
		[IgnoreDataMember]
		[ForeignKey("UserUuid")]
		public User User { get; set; }

		[XmlIgnore]
		[JsonIgnore]
		[IgnoreDataMember]
		[ForeignKey("TaskUuid")]
		public IEnumerable<PushNotification> PushNotifications { get; set; }
	}

	[Table("pushnotification")]
	public class PushNotification
	{
		[Key]
		[Column("uuid")]
		public Guid Uuid { get; set; }

		[Column("serviceid")]
		public Guid ServiceId { get; set; }

		[XmlIgnore]
		[JsonIgnore]
		[IgnoreDataMember]
		[Column("userid")]
		public Guid UserUuid { get; set; }

		[XmlIgnore]
		[JsonIgnore]
		[IgnoreDataMember]
		[ForeignKey("UserUuid")]
		public User User { get; set; }

		[XmlIgnore]
		[JsonIgnore]
		[IgnoreDataMember]
		[Column("taskid")]
		public Guid TaskUuid { get; set; }

		[XmlIgnore]
		[JsonIgnore]
		[IgnoreDataMember]
		[ForeignKey("TaskUuid")]
		public User Task { get; set; }

		[Column("duedate")]
		public DateTime DueDate { get; set; }
	}
}

