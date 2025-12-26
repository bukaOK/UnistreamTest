using Microsoft.EntityFrameworkCore;
using UnistreamTest.Infrastructure.Database;
using UnistreamTest.Models.Common;
using UnistreamTest.Models.TransactionApi;
using UnistreamTest.RequestHandlers.Abstract;

namespace UnistreamTest.RequestHandlers
{
    public class GetPaymentTransactionHandler : IGetPaymentTransactionHandler
    {
        private readonly AppDbContext _dbContext;

        public GetPaymentTransactionHandler(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<AppResult<Transaction>> HandleAsync(Guid id, CancellationToken cancellationToken)
        {
            var trans = await _dbContext.PaymentTransactions
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new Transaction
                {
                    Id = x.Id,
                    TransactionDate = x.TransactionDate,
                    Amount = x.Amount
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (trans == null)
            {
                return new ErrorResultInfo(ECommonErrorReasons.NotFound, "Транзакция не найдена");
            }

            return trans;
        }
    }
}
