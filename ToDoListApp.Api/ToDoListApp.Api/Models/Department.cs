// File: Models/Department.cs
using System.ComponentModel.DataAnnotations;

namespace ToDoListApp.Api.Models
{
    public class Department
    {
        [Key]
        public int DepartmentId { get; set; }

        [Required]
        [StringLength(100)]
        public string DepartmentName { get; set; }

        public string Description { get; set; }

        // Một phòng ban có thể có nhiều dự án
        public virtual ICollection<Project> Projects { get; set; }
    }
}