using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UnistreamTest.Domain.Entities;

namespace UnistreamTest.Infrastructure.Database.EntityConfigs
{
    public class PaymentTransactionEntityConfiguration : IEntityTypeConfiguration<PaymentTransactionEntity>
    {
        public void Configure(EntityTypeBuilder<PaymentTransactionEntity> builder)
        {
            builder.HasKey(x => x.Id);
        }
    }
}
