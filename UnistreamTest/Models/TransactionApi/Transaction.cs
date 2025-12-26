namespace UnistreamTest.Models.TransactionApi
{
    public record Transaction
    {
        public required Guid Id { get; set; }

        public required DateTime TransactionDate { get; set; }

        public required decimal Amount { get; set; }
    }
}
