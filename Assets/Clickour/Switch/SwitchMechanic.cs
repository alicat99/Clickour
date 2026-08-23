using Clickour.Balance;
using Clickour.Core;
using DG.Tweening;
using UnityEngine;

namespace Clickour.Switch
{
    public sealed class SwitchMechanic : MonoBehaviour, ICursorClickTarget
    {
        static readonly Color DARK_GRAY = new Color32(66, 70, 77, 255);
        static readonly Color BLUE = new Color32(11, 87, 208, 255);
        static readonly Color PALE_BLUE = new Color32(211, 227, 253, 255);

        [SerializeField] SwitchKind kind;
        [SerializeField] Transform handle;
        [SerializeField] SpriteRenderer track_renderer;
        [SerializeField] Sprite checked_track;
        [SerializeField] BoxCollider2D solid_collider;
        [SerializeField] BoxCollider2D exit_zone;
        [SerializeField] float handle_distance = 0.48f;
        [SerializeField] bool active_on_positive = true;

        bool handle_positive;
        Sprite unchecked_track;

        public bool HoldsCursor => true;

        void Awake()
        {
            unchecked_track = track_renderer.sprite;
            handle.GetComponent<SpriteRenderer>().sortingOrder = track_renderer.sortingOrder + 100;
            ApplyState(false);
        }

        public void Configure(
            SwitchKind kind,
            Transform handle,
            SpriteRenderer track_renderer,
            Sprite checked_track,
            BoxCollider2D solid_collider,
            BoxCollider2D exit_zone,
            bool active_on_positive = true)
        {
            this.kind = kind;
            this.handle = handle;
            this.track_renderer = track_renderer;
            this.checked_track = checked_track;
            unchecked_track = track_renderer.sprite;
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
            var active = handle_positive == active_on_positive;
            var target_distance = active ? Mathf.Min(handle_distance, 0.4375f) : handle_distance;
            var target = new Vector3(handle_positive ? target_distance : -target_distance, 0, -0.1f);
            var target_scale = Vector3.one * (active ? 1.2f : 0.8f);
            if (animated)
            {
                handle.DOLocalMove(target, BalanceDatabase.GetFloat("switch_toggle_duration"));
                handle.DOScale(target_scale, BalanceDatabase.GetFloat("switch_toggle_duration"));
            }
            else
            {
                handle.localPosition = target;
                handle.localScale = target_scale;
            }

            if (kind == SwitchKind.Gray)
            {
                track_renderer.sprite = checked_track;
                track_renderer.color = DARK_GRAY;
                solid_collider.enabled = false;
                return;
            }

            track_renderer.sprite = active ? checked_track : unchecked_track;
            track_renderer.color = active ? BLUE : PALE_BLUE;
            solid_collider.enabled = active;
        }
    }
}
