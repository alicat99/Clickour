using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Clickour.Core
{
    public sealed class CursorInteractor : MonoBehaviour
    {
        [SerializeField] CursorController cursor;
        [SerializeField] Collider2D cursor_collider;
        [SerializeField] LayerMask clickable_layers = -1;

        readonly List<Collider2D> nearby = new();
        readonly List<ClickCandidate> candidates = new();
        ICursorClickTarget active_target;

        public void Configure(CursorController cursor, Collider2D cursor_collider)
        {
            this.cursor = cursor;
            this.cursor_collider = cursor_collider;
        }

        void Update()
        {
            var mouse = Mouse.current;
            if (mouse == null)
                return;

            if (mouse.leftButton.wasPressedThisFrame)
                Press();

            if (active_target != null)
                active_target.Hold(cursor, Time.deltaTime);

            if (mouse.leftButton.wasReleasedThisFrame && active_target != null)
            {
                active_target.Release(cursor);
                active_target = null;
            }
        }

        void Press()
        {
            var selected = FindTarget();
            if (selected == null)
                return;

            active_target = selected.Value.Target;
            active_target.Press(cursor, selected.Value.Collider);
            if (active_target.HoldsCursor)
                return;

            active_target.Release(cursor);
            active_target = null;
        }

        ClickCandidate? FindTarget()
        {
            nearby.Clear();
            candidates.Clear();
            var bounds = cursor_collider.bounds;
            var filter = new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = clickable_layers,
                useTriggers = true
            };
            Physics2D.OverlapBox(bounds.center, bounds.size + Vector3.one * 0.12f, 0, filter, nearby);

            foreach (var candidate_collider in nearby)
            {
                if (candidate_collider == cursor_collider)
                    continue;

                var target = FindClickTarget(candidate_collider);
                if (target == null)
                    continue;

                var separation = cursor_collider.Distance(candidate_collider);
                var touching = separation.isValid && !separation.isOverlapped && separation.distance <= 0.025f;
                var distance = Vector2.Distance(cursor.Position, candidate_collider.ClosestPoint(cursor.Position));
                candidates.Add(new ClickCandidate(target, candidate_collider, touching, distance));
            }

            return ClickSelectionPolicy.Select(candidates);
        }

        static ICursorClickTarget FindClickTarget(Collider2D candidate)
        {
            foreach (var behaviour in candidate.GetComponentsInParent<MonoBehaviour>(true))
                if (behaviour is ICursorClickTarget target)
                    return target;
            return null;
        }
    }
}
