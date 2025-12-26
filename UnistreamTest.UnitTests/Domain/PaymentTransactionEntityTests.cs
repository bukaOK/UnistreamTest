using UnistreamTest.Domain.Entities;
using UnistreamTest.Models.Common;
using UnistreamTest.Models.TransactionApi;

namespace UnistreamTest.UnitTests.Domain
{
    public class PaymentTransactionEntityTests
    {
        [Fact]
        public void Create_WithValidTransaction_ReturnsSuccessAndSetsProperties()
        {
            // Arrange
            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                TransactionDate = DateTime.UtcNow.AddMinutes(-5),
                Amount = 150.75m
            };

            // Act
            var result = PaymentTransactionEntity.Create(transaction);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            var entity = result.Data!;
            Assert.Equal(transaction.Id, entity.Id);
            Assert.Equal(transaction.Amount, entity.Amount);
            Assert.Equal(transaction.TransactionDate, entity.TransactionDate);
            Assert.NotEqual(default, entity.CreateDate);
            // CreateDate should be recent (within a few seconds)
            Assert.True((DateTime.UtcNow - entity.CreateDate).TotalSeconds < 10);
        }

        [Fact]
        public void Create_WithFutureTransactionDate_ReturnsValidationError()
        {
            // Arrange
            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                TransactionDate = DateTime.UtcNow.AddMinutes(10), // future
                Amount = 10m
            };

            // Act
            var result = PaymentTransactionEntity.Create(transaction);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Error);
            Assert.IsType<FieldValidationErrorResultInfo>(result.Error);

            var validation = (FieldValidationErrorResultInfo)result.Error!;
            Assert.True(validation.Errors.ContainsKey("transactionDate"));
            var messages = validation.Errors["transactionDate"];
            Assert.Contains("Дата не может быть позже текущей", messages);
        }

        [Fact]
        public void Create_WithNegativeAmount_ReturnsValidationError()
        {
            // Arrange
            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                TransactionDate = DateTime.UtcNow.AddMinutes(-1),
                Amount = -5m
            };

            // Act
            var result = PaymentTransactionEntity.Create(transaction);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Error);
            Assert.IsType<FieldValidationErrorResultInfo>(result.Error);

            var validation = (FieldValidationErrorResultInfo)result.Error!;
            Assert.True(validation.Errors.ContainsKey("amount"));
            var messages = validation.Errors["amount"];
            Assert.Contains("Сумма должна быть положительной", messages);
        }

        [Fact]
        public void Create_WithMultipleValidationErrors_ReturnsAllErrors()
        {
            // Arrange
            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                TransactionDate = DateTime.UtcNow.AddMinutes(30), // future
                Amount = -100m // negative
            };

            // Act
            var result = PaymentTransactionEntity.Create(transaction);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Error);
            Assert.IsType<FieldValidationErrorResultInfo>(result.Error);

            var validation = (FieldValidationErrorResultInfo)result.Error!;
            Assert.True(validation.Errors.ContainsKey("transactionDate"));
            Assert.True(validation.Errors.ContainsKey("amount"));

            var dateMessages = validation.Errors["transactionDate"];
            var amountMessages = validation.Errors["amount"];

            Assert.Contains("Дата не может быть позже текущей", dateMessages);
            Assert.Contains("Сумма должна быть положительной", amountMessages);
        }
    }
}
