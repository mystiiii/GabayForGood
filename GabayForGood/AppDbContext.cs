using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GabayForGood.DataModel
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=MAIBENBEN-PC\\SQLEXPRESS;Database=GabayforGood;UID=sa;PWD=stbenilde;TrustServerCertificate=true");
        }

        protected override void OnModelCreating(ModelBuilder mb)
        {
            base.OnModelCreating(mb);

            mb.Entity<Organization>().ToTable("Organizations");
            mb.Entity<Organization>().Property(p => p.OrganizationId).ValueGeneratedOnAdd();
            mb.Entity<Organization>().Property(p => p.Description).HasColumnType("nvarchar(100)");
            mb.Entity<Organization>().Property(p => p.YearFounded).HasColumnType("int");
            mb.Entity<Organization>().Property(p => p.Address).HasColumnType("nvarchar(150)");
            mb.Entity<Organization>().Property(p => p.Email).HasColumnType("nvarchar(50)");
            mb.Entity<Organization>().Property(p => p.ContactNo).HasColumnType("nvarchar(50)");
            mb.Entity<Organization>().Property(p => p.ContactPerson).HasColumnType("nvarchar(50)");
            mb.Entity<Organization>().Property(p => p.OrgLink).HasColumnType("nvarchar(50)").IsRequired(false);
            mb.Entity<Organization>().Property(p => p.CreatedAt).HasColumnType("DateTime2(7)");

            mb.Entity<Project>().ToTable("Projects");
            mb.Entity<Project>().Property(p => p.OrganizationId).ValueGeneratedOnAdd();
            mb.Entity<Project>().Property(p => p.Title).HasColumnType("nvarchar50");
            mb.Entity<Project>().Property(p => p.Title).HasColumnType("nvarchar50");


        }

        public DbSet<Organization> Organizations { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectUpdate> ProjectUpdates { get; set; }
        public DbSet<Donation> Donations { get; set; }

    }
}
