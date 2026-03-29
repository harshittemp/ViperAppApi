using Microsoft.EntityFrameworkCore;
using ViperAppApi.Models.Domains;

namespace ViperAppApi.Data;

public class AppDbContext:DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options):base (options){ 
     
    
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Post>Posts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder) { 
    
     base.OnModelCreating(modelBuilder);
        //User Email Unique
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Post>()
            .HasOne(p => p.User)
            .WithMany(p => p.Posts)
            .HasForeignKey(p => p.UserID);
    
    }
}
