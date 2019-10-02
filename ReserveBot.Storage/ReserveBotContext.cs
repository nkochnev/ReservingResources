using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using ReserveBot.Storage.Tables;
using Microsoft.EntityFrameworkCore;

namespace ReserveBot.Storage
{
    public class ReserveBotContext : DbContext 
    {
        public DbSet<TeamEntity> Teams { get; set; }
        public DbSet<AccountEntity> Accounts { get; set; }
        public DbSet<AccountInTeamEntity> AccountInTeams { get; set; }
        public DbSet<ResourceEntity> Resources { get; set; }
        public DbSet<ReserveEntity> Reserves { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseLazyLoadingProxies()
                .UseNpgsql("Database=ReserveBot;Password=postgres;Username=postgres;Host=localhost;ApplicationName=Booking");
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            DefaultValuesForIdFields(modelBuilder);
        }

        private static void DefaultValuesForIdFields(ModelBuilder modelBuilder)
        {
            foreach (var entity in modelBuilder.Model.GetEntityTypes()
                .Where(t =>
                    t.ClrType.GetProperties()
                        .Any(p => p.CustomAttributes.Any(a => a.AttributeType == typeof(DatabaseGeneratedAttribute)))))
            {
                foreach (var property in entity.ClrType.GetProperties()
                    .Where(p => p.PropertyType == typeof(Guid) &&
                                p.CustomAttributes.Any(a => a.AttributeType == typeof(DatabaseGeneratedAttribute))))
                {
                    modelBuilder
                        .Entity(entity.ClrType)
                        .Property(property.Name)
                        .HasDefaultValueSql("uuid_generate_v4()");
                }
            }
        }
    }
}