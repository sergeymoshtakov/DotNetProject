using DotNetProject.Data.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetProject.Data
{
    public class DataContext : DbContext
    {
        public DbSet<Entity.Gender> Genders { get; set; }
        public DbSet<Entity.User> Users { get; set; }
        public DbSet<Entity.Subscription> Subscriptions { get; set; }
        public DbSet<Entity.Message> Messages { get; set; }
        public DataContext() : base() { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(
                @"Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=DotNetProject;Integrated Security=True"
                );
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Entity.User>() //
                .HasOne(u => u.Gender) // name of property
                .WithMany(g => g.Users) // type of connection one to many
                .HasForeignKey(u => u.IdGender) // foreign key
                .HasPrincipalKey(g => g.Id); //main key
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
