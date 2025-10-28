// File: Data/ApplicationDbContext.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ToDoListApp.Api.Models;
// Thêm dòng này vào trên cùng
using Task = ToDoListApp.Api.Models.Task;

namespace ToDoListApp.Api.Data
{
    public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        // THÊM 3 DÒNG NÀY VÀO
        public DbSet<Department> Departments { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Models.Task> Tasks { get; set; } // Dùng Models.Task để tránh xung đột tên
        public DbSet<ProjectMember> ProjectMembers { get; set; }
        public DbSet<Comment> Comments { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Cấu hình quan hệ User <-> Task (tránh lỗi)
            builder.Entity<Task>()
                .HasOne(t => t.Assignee)
                .WithMany() // Một User có thể được gán nhiều Task
                .HasForeignKey(t => t.AssigneeId)
                .OnDelete(DeleteBehavior.Restrict); // Không xóa User khi xóa Task

            builder.Entity<Task>()
                .HasOne(t => t.Reporter)
                .WithMany() // Một User có thể report nhiều Task
                .HasForeignKey(t => t.ReporterId)
                .OnDelete(DeleteBehavior.Restrict); // Không xóa User khi xóa Task
            builder.Entity<ProjectMember>()
            .HasIndex(pm => new { pm.ProjectId, pm.UserId })
            .IsUnique();

            // Khi xóa Task -> Xóa luôn Comment (Cascade)
            builder.Entity<Comment>()
                .HasOne(c => c.Task)
                .WithMany() // Một Task có nhiều Comment
                .HasForeignKey(c => c.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            // Khi xóa User -> KHÔNG xóa Comment (Restrict), hoặc set UserId thành null tùy logic
            builder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany() // Một User có nhiều Comment
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Hoặc .SetNull nếu UserId cho phép null
        }
    }
}
