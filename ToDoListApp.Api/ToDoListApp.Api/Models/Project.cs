// File: Models/Project.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Task = ToDoListApp.Api.Models.Task;

namespace ToDoListApp.Api.Models
{
    // Thêm dòng này vào trên cùng
 
    public class Project
    {
        [Key]
        public int ProjectId { get; set; }

        [Required]
        [StringLength(100)]
        public string ProjectName { get; set; }

        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; } // Dấu ? cho phép null

        // Khóa ngoại liên kết với Phòng ban
        public int? DepartmentId { get; set; } // Dấu ? cho phép dự án không thuộc PB nào
        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; }

        // Một dự án có nhiều công việc
        public virtual ICollection<Models.Task> Tasks { get; set; }
    }
}
