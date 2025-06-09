using NUnit.Framework;

namespace CrudBankApi.Tests
{
    [TestFixture]
    public class BasicTests
    {
        [Test]
        public void SampleTest_ShouldPass()
        {
            // Arrange
            var expected = 4;
            
            // Act
            var result = 2 + 2;
            
            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        [Category("UnitTest")]
        public void AnotherTest_ShouldAlsoPass()
        {
            // Arrange
            var text = "Hello World";
            
            // Act & Assert
            Assert.That(text, Does.Contain("World"));
        }
    }
}
