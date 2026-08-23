using NUnit.Framework;

namespace Clickour.GameFlow.Tests
{
    public sealed class MapCodecTests
    {
        [Test]
        public void RoundTripPreservesMapShapeAndCells()
        {
            var source = new byte[] { 0, 1, 2, 3, 4, 5 };
            var encoded = MapCodec.Encode(3, 2, source);

            var decoded = MapCodec.Decode(encoded, out var width, out var height);

            Assert.That(width, Is.EqualTo(3));
            Assert.That(height, Is.EqualTo(2));
            Assert.That(decoded, Is.EqualTo(source));
        }
    }
}
