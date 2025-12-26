using Microsoft.EntityFrameworkCore;
using UnistreamTest.Domain.Entities;
using UnistreamTest.Infrastructure.Database;
using UnistreamTest.Infrastructure.Redis.Abstract;
using UnistreamTest.Models.Common;
using UnistreamTest.Models.TransactionApi;
using UnistreamTest.RequestHandlers.Abstract;

namespace UnistreamTest.RequestHandlers
{
    public class CreatePaymentTransactionHandler : ICreatePaymentTransactionHandler
    {
        private readonly AppDbContext _dbContext;
        private readonly IRedisTransactionsCountChecker _redisTransactionsCountChecker;

        public CreatePaymentTransactionHandler(
            AppDbContext dbContext,
            IRedisTransactionsCountChecker redisTransactionsCountChecker)
        {
            _dbContext = dbContext;
            _redisTransactionsCountChecker = redisTransactionsCountChecker;
        }

        public async Task<AppResult<CreateTransactionResponse>> HandleAsync(Transaction request, CancellationToken cancellationToken)
        {
            var existingTransactionCreateDate = await _dbContext.PaymentTransactions
                .AsNoTracking()
                .Where(x => x.Id == request.Id)
                .Select(x => x.CreateDate)
                .FirstOrDefaultAsync(cancellationToken);

            if (existingTransactionCreateDate != default)
                return new CreateTransactionResponse { InsertDateTime = existingTransactionCreateDate };

            var paymentTransactionResult = PaymentTransactionEntity.Create(request);
            if (!paymentTransactionResult.Success)
                return paymentTransactionResult.Error ?? throw new NullReferenceException("paymentTransactionResult.Error");

            var canAddMore = await _redisTransactionsCountChecker.IncrementAndCheckTransactionsCountAsync(cancellationToken);
            if (!canAddMore)
            {
                return AppResult.CreateValidationError("Достигнут лимит транзакций");
            }
            try
            {
                _dbContext.Add(paymentTransactionResult.Data ?? throw new NullReferenceException("paymentTransactionResult.Data"));

                await _dbContext.SaveChangesAsync(cancellationToken);

                return new CreateTransactionResponse { InsertDateTime = paymentTransactionResult.Data.CreateDate };
            }
            catch
            {
                await _redisTransactionsCountChecker.DecrementTransactionsCountAsync(cancellationToken);
                throw;
            }
        }
    }
}
