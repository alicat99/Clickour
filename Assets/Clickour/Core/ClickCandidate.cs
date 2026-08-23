using UnityEngine;

namespace Clickour.Core
{
    public readonly struct ClickCandidate
    {
        public ClickCandidate(ICursorClickTarget target, Collider2D collider, bool touching, float distance)
        {
            Target = target;
            Collider = collider;
            Touching = touching;
            Distance = distance;
        }

        public ICursorClickTarget Target { get; }
        public Collider2D Collider { get; }
        public bool Touching { get; }
        public float Distance { get; }
    }
}
