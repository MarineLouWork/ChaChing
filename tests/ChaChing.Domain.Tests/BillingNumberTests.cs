using ChaChing.Domain.Billing;

namespace ChaChing.Domain.Tests
{
    public class BillingNumberTests
    {
        [Theory]
        [InlineData(2026, 42, "QUO-2026-0042")]
        [InlineData(2026, 1, "QUO-2026-0001")]
        [InlineData(2026, 12345, "QUO-2026-12345")]
        public void ToString_IsTheDisplayedReference(int year, int sequence, string expected)
        {
            Assert.Equal(expected, new BillingNumber(TypeDocument.QUO, year, sequence).ToString());
        }

        [Fact]
        public void ToString_OfAnInvoiceNumber_StartsWithTheInvoicePrefix()
        {
            Assert.Equal("INV-2026-0042", new BillingNumber(TypeDocument.INV, 2026, 42).ToString());
        }

        [Fact]
        public void SameYearAndSequence_AreEqual()
        {
            Assert.Equal(new BillingNumber(TypeDocument.QUO, 2026, 42), new BillingNumber(TypeDocument.QUO, 2026, 42));
        }

        [Fact]
        public void SameYearAndSequence_OfAnotherDocumentType_AreNotEqual()
        {
            Assert.NotEqual(new BillingNumber(TypeDocument.QUO, 2026, 42), new BillingNumber(TypeDocument.INV, 2026, 42));
        }

        [Fact]
        public void Create_WithUndefinedDocumentType_Throws()
        {
            var action = () => new BillingNumber((TypeDocument)999, 2026, 42);

            Assert.Throws<ArgumentException>(action);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Create_WithSequenceBelowOne_Throws(int sequence)
        {
            var action = () => new BillingNumber(TypeDocument.QUO, 2026, sequence);

            Assert.Throws<ArgumentOutOfRangeException>(action);
        }

        [Theory]
        [InlineData(1999)]
        [InlineData(10000)]
        public void Create_WithYearOutOfRange_Throws(int year)
        {
            var action = () => new BillingNumber(TypeDocument.QUO, year, 1);

            Assert.Throws<ArgumentOutOfRangeException>(action);
        }
    }
}
