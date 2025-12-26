using Microsoft.EntityFrameworkCore;
using UnistreamTest.Domain.Entities;
using UnistreamTest.Infrastructure.Database.EntityConfigs;

namespace UnistreamTest.Infrastructure.Database
{
    public class AppDbContext : DbContext
    {
        protected AppDbContext() { }

        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<PaymentTransactionEntity> PaymentTransactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new PaymentTransactionEntityConfiguration());
        }
    }
}
