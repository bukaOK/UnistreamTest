using UnistreamTest.Models.Common;
using UnistreamTest.Models.TransactionApi;

namespace UnistreamTest.RequestHandlers.Abstract
{
    public interface ICreatePaymentTransactionHandler
    {
        Task<AppResult<CreateTransactionResponse>> HandleAsync(Transaction request, CancellationToken cancellationToken);
    }
}
