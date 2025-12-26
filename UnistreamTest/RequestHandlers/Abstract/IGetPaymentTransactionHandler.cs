using UnistreamTest.Models.Common;
using UnistreamTest.Models.TransactionApi;

namespace UnistreamTest.RequestHandlers.Abstract
{
    public interface IGetPaymentTransactionHandler
    {
        Task<AppResult<Transaction>> HandleAsync(Guid id, CancellationToken cancellationToken);
    }
}
