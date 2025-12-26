using UnistreamTest.Models.Common;
using UnistreamTest.Models.TransactionApi;

namespace UnistreamTest.Domain.Entities
{
    public class PaymentTransactionEntity
    {
        public Guid Id { get; private set; }

        // Таймзону добавлять не стал, хранится в UTC
        public DateTime TransactionDate { get; private set; }

        public decimal Amount { get; private set; }

        public DateTime CreateDate { get; private set; }

        public static AppResult<PaymentTransactionEntity> Create(Transaction transaction)
        {
            var validationError = new FieldValidationErrorResultInfo();
            var universalTransactionDate = transaction.TransactionDate.ToUniversalTime();

            if (universalTransactionDate > DateTime.UtcNow)
                validationError.AddError("transactionDate", "Дата не может быть позже текущей");
            if (transaction.Amount < 0)
                validationError.AddError("amount", "Сумма не может быть меньше 0");

            if (validationError.Errors.Count > 0)
                return validationError;

            return new PaymentTransactionEntity
            {
                Id = transaction.Id,
                Amount = transaction.Amount,
                TransactionDate = transaction.TransactionDate,
                CreateDate = DateTime.UtcNow
            };
        }
    }
}
