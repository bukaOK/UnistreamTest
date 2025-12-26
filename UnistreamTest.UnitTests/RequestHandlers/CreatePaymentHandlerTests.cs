using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using UnistreamTest.Domain.Entities;
using UnistreamTest.Infrastructure.Database;
using UnistreamTest.Infrastructure.Redis.Abstract;
using UnistreamTest.Models.Common;
using UnistreamTest.Models.TransactionApi;
using UnistreamTest.RequestHandlers;

namespace UnistreamTest.Tests.RequestHandlers
{
    public class CreatePaymentTransactionHandlerTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly AppDbContext _dbContext;

        public CreatePaymentTransactionHandlerTests()
        {
            // В этом методе нет сложных запросов, поэтому можем воспользоваться юнитами
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(_connection)
                .Options;

            _dbContext = new AppDbContext(options);
            _dbContext.Database.EnsureCreated();
        }


        [Fact]
        public async Task HandleAsync_WhenTransactionAlreadyExists_ReturnsExistingCreateDate()
        {
            // Arrange
            await ClearDatabaseAsync();

            var existingTransaction = new Transaction
            {
                Id = Guid.NewGuid(),
                TransactionDate = DateTime.UtcNow.AddMinutes(-10),
                Amount = 100m
            };

            var createResult = PaymentTransactionEntity.Create(existingTransaction);
            Assert.True(createResult.Success);
            _dbContext.Add(createResult.Data!);
            await _dbContext.SaveChangesAsync(CancellationToken.None);

            var dbSaveCalled = false;
            void dbSaveEvent(object? sender, SavedChangesEventArgs e) => dbSaveCalled = true;
            _dbContext.SavedChanges += dbSaveEvent;

            var redisSub = Substitute.For<IRedisTransactionsCountChecker>();
            var handler = new CreatePaymentTransactionHandler(_dbContext, redisSub);

            // Act
            var result = await handler.HandleAsync(existingTransaction, CancellationToken.None);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(createResult.Data!.CreateDate, result.Data!.InsertDateTime);
            
            Assert.False(dbSaveCalled);

            _dbContext.SavedChanges -= dbSaveEvent;
        }

        [Fact]
        public async Task HandleAsync_WhenRedisDeniesIncrement_ReturnsValidationError()
        {
            // Arrange
            await ClearDatabaseAsync();

            var dbSaveCalled = false;
            void dbSaveEvent(object? sender, SavedChangesEventArgs e) => dbSaveCalled = true;
            _dbContext.SavedChanges += dbSaveEvent;

            var request = new Transaction
            {
                Id = Guid.NewGuid(),
                TransactionDate = DateTime.UtcNow.AddMinutes(-5),
                Amount = 50m
            };

            var redisSub = Substitute.For<IRedisTransactionsCountChecker>();
            redisSub.IncrementAndCheckTransactionsCountAsync(CancellationToken.None).Returns(false);

            var handler = new CreatePaymentTransactionHandler(_dbContext, redisSub);

            // Act
            var result = await handler.HandleAsync(request, CancellationToken.None);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Error);
            Assert.Contains("Достигнут лимит транзакций", result.Error.Message);

            Assert.False(dbSaveCalled);

            _dbContext.SavedChanges -= dbSaveEvent;
        }

        [Fact]
        public async Task HandleAsync_WhenPaymentTransactionEntityValidationFails_ReturnsValidationErrors_AndDoesNotCallRedis()
        {
            // Arrange
            await ClearDatabaseAsync();

            var dbSaveCalled = false;
            void dbSaveEvent(object? sender, SavedChangesEventArgs e) => dbSaveCalled = true;
            _dbContext.SavedChanges += dbSaveEvent;

            // Создаём запрос, который не пройдёт валидацию (дата транзакции в будущем)
            var request = new Transaction
            {
                Id = Guid.NewGuid(),
                TransactionDate = DateTime.UtcNow.AddMinutes(10), // дата в будущем
                Amount = 10m
            };

            var redisSub = Substitute.For<IRedisTransactionsCountChecker>();
            redisSub.IncrementAndCheckTransactionsCountAsync(CancellationToken.None).Returns(true);

            var handler = new CreatePaymentTransactionHandler(_dbContext, redisSub);

            // Act
            var result = await handler.HandleAsync(request, CancellationToken.None);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Error);
            // FieldValidationErrorResultInfo использует сообщение базового класса
            Assert.Equal("Проверьте правильность заполнения полей", result.Error.Message);

            Assert.False(dbSaveCalled);

            // При наличии, проверяем конкретную ошибку поля
            if (result.Error is FieldValidationErrorResultInfo fieldErrors)
            {
                Assert.True(fieldErrors.Errors.ContainsKey("transactionDate"));
                Assert.Contains("Дата не может быть позже текущей", fieldErrors.Errors["transactionDate"]);
            }

            _dbContext.SavedChanges -= dbSaveEvent;
        }

        [Fact]
        public async Task HandleAsync_WhenNoExistingAndRedisAllows_CreatesAndSavesTransaction()
        {
            // Arrange
            await ClearDatabaseAsync();

            var request = new Transaction
            {
                Id = Guid.NewGuid(),
                TransactionDate = DateTime.UtcNow.AddMinutes(-1),
                Amount = 123.45m
            };

            var redisSub = Substitute.For<IRedisTransactionsCountChecker>();
            redisSub.IncrementAndCheckTransactionsCountAsync(CancellationToken.None).Returns(true);

            var handler = new CreatePaymentTransactionHandler(_dbContext, redisSub);

            var dbSaveCalled = false;
            void dbSaveEvent(object? sender, SavedChangesEventArgs e) => dbSaveCalled = true;
            _dbContext.SavedChanges += dbSaveEvent;

            // Act
            var result = await handler.HandleAsync(request, CancellationToken.None);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            var createdInsertDate = result.Data!.InsertDateTime;
            Assert.NotEqual(default, createdInsertDate);

            // Проверяем что сохранилось в бд
            var saved = await _dbContext.PaymentTransactions.FirstOrDefaultAsync(x => x.Id == request.Id, CancellationToken.None);
            Assert.NotNull(saved);
            Assert.Equal(request.Id, saved!.Id);
            Assert.Equal(saved.CreateDate, createdInsertDate);

            Assert.True(dbSaveCalled);

            _dbContext.SavedChanges -= dbSaveEvent;
        }

        public void Dispose()
        {
            _dbContext.Dispose();
            _connection.Dispose();
        }

        private async Task ClearDatabaseAsync()
        {
            await _dbContext.PaymentTransactions.ExecuteDeleteAsync();
        }
    }
}
