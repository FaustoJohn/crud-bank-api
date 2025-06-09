using NUnit.Framework;

namespace CrudBankApi.Tests
{
    [TestFixture]
    public class SimpleTest
    {
        [Test]
        public void SimpleTest_ShouldPass()
        {
            Assert.That(1 + 1, Is.EqualTo(2));
        }
    }
}
