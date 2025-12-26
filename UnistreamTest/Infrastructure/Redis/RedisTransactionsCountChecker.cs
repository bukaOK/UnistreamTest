using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using UnistreamTest.Infrastructure.Database;
using UnistreamTest.Infrastructure.Redis.Abstract;

namespace UnistreamTest.Infrastructure.Redis
{
    public class RedisTransactionsCountChecker : IRedisTransactionsCountChecker
    {
        private const string Key = "TransactionsCount";

        private readonly IDatabase _redisDb;
        private readonly AppDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RedisTransactionsCountChecker> _logger;

        public RedisTransactionsCountChecker(
            IConnectionMultiplexer multiplexer,
            AppDbContext dbContext,
            IConfiguration configuration,
            ILogger<RedisTransactionsCountChecker> logger)
        {
            _redisDb = multiplexer.GetDatabase();
            _dbContext = dbContext;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Увеличивает количество транзакций 
        /// </summary>
        /// <returns>True если можно еще создавать транзакции, false если нельзя</returns>
        public async Task<bool> IncrementAndCheckTransactionsCountAsync(CancellationToken cancellationToken)
        {
            var isKeyExists = await _redisDb.KeyExistsAsync(Key, flags: CommandFlags.DemandMaster);
            if (!isKeyExists)
            {
                var count = await _dbContext.PaymentTransactions.CountAsync(cancellationToken);
                await _redisDb.StringSetAsync(Key, count, flags: CommandFlags.DemandMaster, when: When.NotExists);
                _logger.Log(LogLevel.Information, "Transactions count initalized");
            }
            var maxCount = _configuration.GetValue<int>("Transactions:MaxCount");
            var result = await _redisDb.StringIncrementAsync(Key, flags: CommandFlags.DemandMaster);
            _logger.Log(LogLevel.Information, "Transactions count increased to {count}", result);
            
            if(result > maxCount)
            {
                var count = await _redisDb.StringDecrementAsync(Key, flags: CommandFlags.DemandMaster);
                _logger.Log(LogLevel.Information, "Transactions count decreased to {count}", count);
                return false;
            }

            return true;
        }

        public async Task DecrementTransactionsCountAsync(CancellationToken cancellationToken)
        {
            await _redisDb.StringDecrementAsync(Key, flags: CommandFlags.DemandMaster);
        }
    }
}
