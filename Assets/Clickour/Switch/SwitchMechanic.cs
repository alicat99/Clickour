using Clickour.Balance;
using Clickour.Core;
using DG.Tweening;
using UnityEngine;

namespace Clickour.Switch
{
    public sealed class SwitchMechanic : MonoBehaviour, ICursorClickTarget
    {
        static readonly Color DARK_GRAY = new(0.26f, 0.28f, 0.31f);
        static readonly Color BLUE = new(0.16f, 0.52f, 0.92f);
        static readonly Color PALE_BLUE = new(0.45f, 0.7f, 0.9f);

        [SerializeField] SwitchKind kind;
        [SerializeField] Transform handle;
        [SerializeField] SpriteRenderer track_renderer;
        [SerializeField] BoxCollider2D solid_collider;
        [SerializeField] BoxCollider2D exit_zone;
        [SerializeField] float handle_distance = 0.48f;
        [SerializeField] bool active_on_positive = true;

        bool handle_positive;

        public bool HoldsCursor => true;

        public void Configure(
            SwitchKind kind,
            Transform handle,
            SpriteRenderer track_renderer,
            BoxCollider2D solid_collider,
            BoxCollider2D exit_zone,
            bool active_on_positive = true)
        {
            this.kind = kind;
            this.handle = handle;
            this.track_renderer = track_renderer;
            this.solid_collider = solid_collider;
            this.exit_zone = exit_zone;
            this.active_on_positive = active_on_positive;
            ApplyState(false);
        }

        public void Press(CursorController cursor, Collider2D clicked_collider)
        {
            cursor.BeginFixedHold(cursor.Position);
            if (kind == SwitchKind.Blue)
                cursor.IgnoreUntilExited(new Collider2D[] { solid_collider }, exit_zone);
        }

        public void Hold(CursorController cursor, float delta_time)
        {
        }

        public void Release(CursorController cursor)
        {
            handle_positive = !handle_positive;
            ApplyState(true);

            var axis = (Vector2)transform.right.normalized;
            var direction = handle_positive ? axis : -axis;
            var one_cell_speed = cursor.JumpVelocity(
                BalanceDatabase.GetFloat("button_jump_height_cells"));
            var launch_speed = one_cell_speed * BalanceDatabase.GetFloat("switch_speed_multiplier");
            var is_horizontal = Mathf.Abs(axis.x) > 0.5f;
            var velocity = is_horizontal
                ? direction * launch_speed + Vector2.up * one_cell_speed
                : direction * launch_speed;
            cursor.ReleaseWithVelocity(velocity);
        }

        void ApplyState(bool animated)
        {
            var target = new Vector3(handle_positive ? handle_distance : -handle_distance, 0, 0);
            if (animated)
                handle.DOLocalMove(target, BalanceDatabase.GetFloat("switch_toggle_duration"));
            else
                handle.localPosition = target;

            if (kind == SwitchKind.Gray)
            {
                track_renderer.color = DARK_GRAY;
                solid_collider.enabled = false;
                return;
            }

            var active = handle_positive == active_on_positive;
            track_renderer.color = active ? BLUE : PALE_BLUE;
            solid_collider.enabled = active;
        }
    }
}
