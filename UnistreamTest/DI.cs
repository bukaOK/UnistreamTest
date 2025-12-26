using StackExchange.Redis;
using UnistreamTest.Infrastructure.Redis;
using UnistreamTest.Infrastructure.Redis.Abstract;
using UnistreamTest.RequestHandlers;
using UnistreamTest.RequestHandlers.Abstract;

namespace UnistreamTest
{
    public static class DI
    {
        public static IServiceCollection RegisterHandlers(this IServiceCollection services)
        {
            services.AddScoped<ICreatePaymentTransactionHandler, CreatePaymentTransactionHandler>();
            services.AddScoped<IGetPaymentTransactionHandler, GetPaymentTransactionHandler>();

            return services;
        }

        public static IServiceCollection RegisterRedis(this IServiceCollection services, string redisHost)
        {
            services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisHost));
            services.AddScoped<IRedisTransactionsCountChecker, RedisTransactionsCountChecker>();
            return services;
        }
    }
}
