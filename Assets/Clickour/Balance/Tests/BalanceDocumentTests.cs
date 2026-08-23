using NUnit.Framework;

namespace Clickour.Balance.Tests
{
    public sealed class BalanceDocumentTests
    {
        [Test]
        public void ParseAndSerializeKeepsFlatKeysInOrder()
        {
            var document = BalanceDocument.Parse("speed: 4\nmap_data: YWJj\n");
            document.Set("speed", "5");

            Assert.That(document.Get("map_data"), Is.EqualTo("YWJj"));
            Assert.That(document.ToYaml(), Is.EqualTo("speed: 5\nmap_data: YWJj\n"));
        }
    }
}
