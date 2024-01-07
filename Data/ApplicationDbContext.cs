using _2001_microservice.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace _2001_microservice.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Users> Users { get; set; }
        public DbSet<UserProfiles> UserProfiles { get; set; }
        public DbSet<UserPreferences> UserPreferences { get; set; }
        public DbSet<UserCreationLog> UserCreationLog { get; set; }
        public DbSet<Admin> Admin { get; set; }
        public DbSet<UserTrails> UserTrails { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Users>().ToTable("Users", "CW2")
                .HasKey(u => u.UserId);

            modelBuilder.Entity<UserProfiles>().ToTable("UserProfiles", "CW2")
                .HasKey(up => up.ProfileId);

            modelBuilder.Entity<UserPreferences>().ToTable("UserPreferences", "CW2")
                .HasKey(p => p.PrefID);             
                

            modelBuilder.Entity<UserCreationLog>().ToTable("UserCreationLog", "CW2").HasNoKey();
            modelBuilder.Entity<Admin>().ToTable("Admin", "CW2")
                .HasKey(a => a.AdminId);

            modelBuilder.Entity<UserTrails>().ToTable("UserTrails", "CW2").HasKey(ut => ut.TrailId);


            
        }
    }
}
