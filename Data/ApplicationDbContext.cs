using _2001_microservice.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace _2001_microservice.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
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

            // Configure the schema for each DbSet
            modelBuilder.Entity<Users>().ToTable("Users", "CW1")
                        .HasKey(u => u.UserId);

            modelBuilder.Entity<UserProfiles>().ToTable("UserProfiles", "CW1")
                        .HasKey(up => up.ProfileId);
                        

            modelBuilder.Entity<UserPreferences>().ToTable("UserPreferences", "CW1").HasNoKey();
            modelBuilder.Entity<UserCreationLog>().ToTable("UserCreationLog", "CW1").HasNoKey();
            modelBuilder.Entity<Admin>().ToTable("Admin", "CW1")
                .HasKey(a => a.AdminId);

            modelBuilder.Entity<UserTrails>().ToTable("UserTrails", "CW1").HasKey(ut => ut.TrailId);


            // Add any additional configurations or relationships here
        }
    }
}
