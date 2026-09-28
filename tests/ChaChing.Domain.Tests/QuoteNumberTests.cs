using ChaChing.Domain.Billing.Quotes;

namespace ChaChing.Domain.Tests
{
    public class QuoteNumberTests
    {
        [Theory]
        [InlineData(2026, 42, "DEV-2026-0042")]
        [InlineData(2026, 1, "DEV-2026-0001")]
        [InlineData(2026, 12345, "DEV-2026-12345")]
        public void ToString_IsTheDisplayedReference(int year, int sequence, string expected)
        {
            Assert.Equal(expected, new QuoteNumber(year, sequence).ToString());
        }

        [Fact]
        public void SameYearAndSequence_AreEqual()
        {
            Assert.Equal(new QuoteNumber(2026, 42), new QuoteNumber(2026, 42));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Create_WithSequenceBelowOne_Throws(int sequence)
        {
            var action = () => new QuoteNumber(2026, sequence);

            Assert.Throws<ArgumentOutOfRangeException>(action);
        }

        [Theory]
        [InlineData(1999)]
        [InlineData(10000)]
        public void Create_WithYearOutOfRange_Throws(int year)
        {
            var action = () => new QuoteNumber(year, 1);

            Assert.Throws<ArgumentOutOfRangeException>(action);
        }
    }
}
