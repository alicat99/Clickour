using System.Collections.Generic;

namespace Clickour.Core
{
    public static class ClickSelectionPolicy
    {
        public static ClickCandidate? Select(IReadOnlyList<ClickCandidate> candidates)
        {
            ClickCandidate? selected = null;
            foreach (var candidate in candidates)
            {
                if (selected == null || IsHigherPriority(candidate, selected.Value))
                    selected = candidate;
            }
            return selected;
        }

        static bool IsHigherPriority(ClickCandidate candidate, ClickCandidate selected)
        {
            if (candidate.Touching != selected.Touching)
                return candidate.Touching;
            return candidate.Distance < selected.Distance;
        }
    }
}
