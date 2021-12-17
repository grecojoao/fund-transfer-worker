using FundTransfer.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FundTransfer.Infra.Storage.Context
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) =>
            Database.EnsureCreated();

        public DbSet<Transfer> Transfers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) { }
    }
}