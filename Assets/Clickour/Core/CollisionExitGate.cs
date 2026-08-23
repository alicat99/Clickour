using System.Collections.Generic;
using UnityEngine;

namespace Clickour.Core
{
    public sealed class CollisionExitGate : MonoBehaviour
    {
        sealed class Gate
        {
            public Collider2D ExitZone;
            public Collider2D[] Solids;
        }

        [SerializeField] Collider2D moving_collider;
        readonly List<Gate> gates = new();

        public void Configure(Collider2D moving_collider) => this.moving_collider = moving_collider;

        public void IgnoreUntilExited(Collider2D[] solids, Collider2D exit_zone)
        {
            foreach (var solid in solids)
                Physics2D.IgnoreCollision(moving_collider, solid, true);
            gates.Add(new Gate { ExitZone = exit_zone, Solids = solids });
        }

        void FixedUpdate()
        {
            for (var i = gates.Count - 1; i >= 0; i--)
            {
                var gate = gates[i];
                if (moving_collider.bounds.Intersects(gate.ExitZone.bounds))
                    continue;

                foreach (var solid in gate.Solids)
                    Physics2D.IgnoreCollision(moving_collider, solid, false);
                gates.RemoveAt(i);
            }
        }
    }
}
