using NUnit.Framework;

namespace Clickour.Core.Tests
{
    public sealed class DirectionalInputTests
    {
        [TestCase(false, false, 0)]
        [TestCase(true, false, -1)]
        [TestCase(false, true, 1)]
        [TestCase(true, true, 0)]
        public void ComposeAxisCombinesDirectionalKeys(bool negative, bool positive, float expected)
        {
            Assert.That(DirectionalInput.ComposeAxis(negative, positive), Is.EqualTo(expected));
        }
    }
}
