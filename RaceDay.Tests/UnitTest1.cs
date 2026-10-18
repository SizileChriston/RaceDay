
using Xunit;

namespace RaceDay.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void BasicTest_Passes()
        {
            // Arrange
            int expected = 2;

            // Act
            int actual = 1 + 1;

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}
