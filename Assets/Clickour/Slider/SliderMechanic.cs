using Clickour.Balance;
using Clickour.Core;
using UnityEngine;

namespace Clickour.Slider
{
    public sealed class SliderMechanic : MonoBehaviour, ICursorClickTarget
    {
        static readonly Color DARK_GRAY = new Color32(66, 70, 77, 255);
        static readonly Color BLUE = new Color32(11, 87, 208, 255);
        static readonly Color PALE_BLUE = new Color32(211, 227, 253, 255);

        [SerializeField] SliderKind kind;
        [SerializeField] Transform handle;
        [SerializeField] SpriteRenderer track_renderer;
        [SerializeField] SpriteRenderer fill_renderer;
        [SerializeField] BoxCollider2D solid_collider;
        [SerializeField] BoxCollider2D exit_zone;
        [SerializeField] float travel = 3;

        float handle_speed;
        Vector2 cursor_offset;

        public bool HoldsCursor => true;

        void Awake()
        {
            fill_renderer.sortingOrder = track_renderer.sortingOrder + 50;
            handle.GetComponent<SpriteRenderer>().sortingOrder = track_renderer.sortingOrder + 100;
            ApplyVisuals();
        }

        public void Configure(
            SliderKind kind,
            Transform handle,
            SpriteRenderer track_renderer,
            SpriteRenderer fill_renderer,
            BoxCollider2D solid_collider,
            BoxCollider2D exit_zone,
            float travel)
        {
            this.kind = kind;
            this.handle = handle;
            this.track_renderer = track_renderer;
            this.fill_renderer = fill_renderer;
            this.solid_collider = solid_collider;
            this.exit_zone = exit_zone;
            this.travel = travel;
            handle.localPosition = new Vector3(-travel * 0.5f, 0, -0.2f);
            ApplyVisuals();
        }

        public void Press(CursorController cursor, Collider2D clicked_collider)
        {
            cursor_offset = cursor.Position - (Vector2)handle.position;
            cursor.BeginFixedHold(cursor.Position);
            if (kind == SliderKind.Blue)
                cursor.IgnoreUntilExited(new Collider2D[] { solid_collider }, exit_zone);
        }

        public void Hold(CursorController cursor, float delta_time)
        {
            var input = Vector2.Dot(DirectionalInput.Read(), (Vector2)transform.right.normalized);

            var acceleration = BalanceDatabase.GetFloat("slider_acceleration");
            var max_speed = BalanceDatabase.GetFloat("slider_max_speed");
            handle_speed = Mathf.MoveTowards(handle_speed, input * max_speed, acceleration * delta_time);
            var position = Mathf.Clamp(handle.localPosition.x + handle_speed * delta_time, -travel * 0.5f, travel * 0.5f);
            if (Mathf.Abs(position) >= travel * 0.5f && Mathf.Sign(handle_speed) == Mathf.Sign(position))
                handle_speed = 0;

            handle.localPosition = new Vector3(position, 0, -0.2f);
            cursor.MoveFixedHold((Vector2)handle.position + cursor_offset);
            ApplyVisuals();
        }

        public void Release(CursorController cursor)
        {
            var axis = (Vector2)transform.right.normalized;
            var velocity = axis * handle_speed;
            if (Mathf.Abs(axis.x) > 0.5f)
            {
                var height = BalanceDatabase.GetFloat("button_jump_height_cells");
                velocity += Vector2.up * cursor.JumpVelocity(height);
            }
            cursor.ReleaseWithVelocity(velocity);
        }

        void ApplyVisuals()
        {
            if (kind == SliderKind.Gray)
            {
                track_renderer.color = DARK_GRAY;
                fill_renderer.enabled = false;
                solid_collider.enabled = false;
                return;
            }

            track_renderer.color = PALE_BLUE;
            fill_renderer.enabled = true;
            fill_renderer.color = BLUE;
            var filled_length = handle.localPosition.x + travel * 0.5f;
            fill_renderer.transform.localPosition = new Vector3(
                -travel * 0.5f + filled_length * 0.5f,
                0,
                -0.1f);
            fill_renderer.transform.localScale = new Vector3(filled_length, 0.72f, 1);
            solid_collider.offset = new Vector2(-travel * 0.5f + filled_length * 0.5f, 0);
            solid_collider.size = new Vector2(Mathf.Max(filled_length, 0.01f), 0.72f);
            solid_collider.enabled = filled_length > 0.02f;
        }
    }
}
