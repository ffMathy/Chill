using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Chill.ExampleApplication.Dal
{
    public class StudentContext : DbContext
    {
        public StudentContext() : this("StudentContext")
        {
        }

        public StudentContext(string nameOrConnectionString) : base(nameOrConnectionString)
        {
        }

        public DbSet<StudentEntity> Students { get; set; }
    }

    [Table("Students")]
    public class StudentEntity
    {
        public Guid Id { get; set; }

        
        public string Name { get; set; }
        public DateTime? BirthDate { get; set; }

        public string Address { get; set; }

        public string City { get; set; }

        public string ZipCode { get; set; }
    }
}