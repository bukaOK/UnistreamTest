
namespace UnistreamTest.Infrastructure.Redis.Abstract
{
    public interface IRedisTransactionsCountChecker
    {
        Task DecrementTransactionsCountAsync(CancellationToken cancellationToken);
        Task<bool> IncrementAndCheckTransactionsCountAsync(CancellationToken cancellationToken);
    }
}
