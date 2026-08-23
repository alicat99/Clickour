using System.Collections.Generic;
using NUnit.Framework;

namespace Clickour.Core.Tests
{
    public sealed class ClickSelectionPolicyTests
    {
        [Test]
        public void TouchingCandidateWinsOverCloserOverlap()
        {
            var overlap = new ClickCandidate(null, null, false, 0);
            var touching = new ClickCandidate(null, null, true, 1);

            var selected = ClickSelectionPolicy.Select(new List<ClickCandidate> { overlap, touching });

            Assert.That(selected.Value.Touching, Is.True);
        }
    }
}
