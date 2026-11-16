using SoundRevival.Repository.Repository;

namespace SoundRevival.Tests.Repository
{
    public class LikePatternTests
    {
        [Theory]
        [InlineData("fender", "fender")]
        [InlineData("50%", @"50\%")]
        [InlineData("f_nder", @"f\_nder")]
        [InlineData(@"C:\amp", @"C:\\amp")]
        public void Escape_TextWithSpecialCharacters_EscapesEachOne(string input, string expected)
        {
            Assert.Equal(expected, LikePattern.Escape(input));
        }

        [Fact]
        public void Escape_BackslashBeforeWildcard_EscapesBothWithoutDoublingTwice()
        {
            // Guards the replacement order: "\%" must become "\\\%", not "\\\\%".
            Assert.Equal(@"\\\%", LikePattern.Escape(@"\%"));
        }

        [Fact]
        public void Contains_PlainText_WrapsEscapedTextInWildcards()
        {
            Assert.Equal(@"%100\%%", LikePattern.Contains("100%"));
        }
    }
}
