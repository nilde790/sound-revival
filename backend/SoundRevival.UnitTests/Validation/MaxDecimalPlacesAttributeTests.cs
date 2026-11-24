using SoundRevival.Dto.Validation;

namespace SoundRevival.UnitTests.Validation
{
    public class MaxDecimalPlacesAttributeTests
    {
        public static TheoryData<decimal> ValidPrices => [1m, 1.01m, decimal.MaxValue, 0m, 1.50m, 1000000m, decimal.MinValue];
        public static TheoryData<decimal> InvalidPrices => [1.001m, 1.1111m, 1.111m, 0.001m];


        [Theory]
        [MemberData(nameof(ValidPrices))]
        public void IsValid_AtMostTwoDecimals_ReturnsTrue(decimal price)
        {         
            var maxDecimal = new MaxDecimalPlacesAttribute(2);
            var result = maxDecimal.IsValid(price);

            Assert.True(result);         
        }
        [Theory]
        [MemberData(nameof(InvalidPrices))]
        public void IsValid_MoreThanTwoDecimals_ReturnsFalse(decimal price)
        {
            var maxDecimal = new MaxDecimalPlacesAttribute(2);
            var result = maxDecimal.IsValid(price);

            Assert.False(result);
        }
        [Fact]
        public void IsValid_NullValue_ReturnsTrue()
        {
            var maxDecimal = new MaxDecimalPlacesAttribute(2);
            var result = maxDecimal.IsValid(null);

            Assert.True(result);
        }


    }
}
