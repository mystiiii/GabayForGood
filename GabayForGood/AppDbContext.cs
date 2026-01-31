using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
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
            //SQL Connection here
        }

        protected override void OnModelCreating(ModelBuilder mb)
        {
            base.OnModelCreating(mb);

            mb.Entity<Project>()
                .HasOne(p => p.Organization)
                .WithMany(p => p.Project)
                .HasForeignKey(p => p.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            mb.Entity<ProjectUpdate>()
                .HasOne(p => p.Project)
                .WithMany(p => p.ProjectUpdates)
                .HasForeignKey(p => p.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            mb.Entity<Donation>()
                .HasOne(d => d.User)
                .WithMany(p => p.Donations)
                .HasForeignKey(d => d.UserId);

            mb.Entity<Donation>()
                .HasOne(d => d.Project)
                .WithMany(p => p.Donations)
                .HasForeignKey(d => d.ProjectId);

            mb.Entity<ApplicationUser>()
               .HasOne(p => p.Organization)
               .WithMany()
               .HasForeignKey(u => u.OrganizationID)
               .OnDelete(DeleteBehavior.Restrict);

            mb.Entity<Organization>()
            .Property(o => o.Password)
            .HasDefaultValue("GFGOrg123!");
        }

        public DbSet<Donation> Donations { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectUpdate> ProjectUpdates { get; set; }

    }
}
