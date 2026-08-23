using Clickour.Balance;
using UnityEngine;

namespace Clickour.Core
{
    public sealed class CursorController : MonoBehaviour
    {
        enum HoldMode
        {
            Free,
            Fixed,
            SlowFall
        }

        [SerializeField] Rigidbody2D body;
        [SerializeField] CollisionExitGate collision_gate;

        HoldMode hold_mode;
        float default_gravity_scale;
        float fixed_x;

        public Rigidbody2D Body => body;
        public Vector2 Position => body.position;

        public void Configure(Rigidbody2D body, CollisionExitGate collision_gate)
        {
            this.body = body;
            this.collision_gate = collision_gate;
        }

        void Awake()
        {
            default_gravity_scale = BalanceDatabase.GetFloat("cursor_gravity_scale");
            body.gravityScale = default_gravity_scale;
            Cursor.visible = false;
        }

        void FixedUpdate()
        {
            if (hold_mode == HoldMode.Fixed)
                return;

            if (hold_mode == HoldMode.SlowFall)
            {
                body.position = new Vector2(fixed_x, body.position.y);
                body.linearVelocity = new Vector2(0, body.linearVelocity.y);
                return;
            }

            var max_speed = BalanceDatabase.GetFloat("cursor_max_horizontal_speed");
            var target_speed = DirectionalInput.Read().x * max_speed;
            var speed = Mathf.MoveTowards(body.linearVelocity.x, target_speed,
                BalanceDatabase.GetFloat("cursor_horizontal_acceleration") * Time.fixedDeltaTime);
            body.linearVelocity = new Vector2(speed, body.linearVelocity.y);
        }

        public void BeginFixedHold(Vector2 world_position)
        {
            hold_mode = HoldMode.Fixed;
            body.bodyType = RigidbodyType2D.Kinematic;
            body.linearVelocity = Vector2.zero;
            body.position = world_position;
        }

        public void MoveFixedHold(Vector2 world_position)
        {
            body.position = world_position;
        }

        public void BeginSlowFall(float world_x, float gravity_scale)
        {
            hold_mode = HoldMode.SlowFall;
            fixed_x = world_x;
            body.gravityScale = gravity_scale;
            body.linearVelocity = new Vector2(0, body.linearVelocity.y);
        }

        public void ReleaseWithVelocity(Vector2 velocity)
        {
            hold_mode = HoldMode.Free;
            body.bodyType = RigidbodyType2D.Dynamic;
            body.gravityScale = default_gravity_scale;
            body.linearVelocity = velocity;
        }

        public void IgnoreUntilExited(Collider2D[] solids, Collider2D exit_zone) =>
            collision_gate.IgnoreUntilExited(solids, exit_zone);

        public float JumpVelocity(float height_in_cells)
        {
            var height = height_in_cells * BalanceDatabase.GetFloat("grid_cell_size");
            return Mathf.Sqrt(2 * Mathf.Abs(Physics2D.gravity.y) * default_gravity_scale * height);
        }
    }
}
