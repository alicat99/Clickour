using Clickour.Balance;
using Clickour.Core;
using UnityEngine;

namespace Clickour.Button
{
    public sealed class ButtonMechanic : MonoBehaviour, ICursorClickTarget
    {
        public bool HoldsCursor => false;

        public void Press(CursorController cursor, Collider2D clicked_collider)
        {
        }

        public void Hold(CursorController cursor, float delta_time)
        {
        }

        public void Release(CursorController cursor)
        {
            var height = BalanceDatabase.GetFloat("button_jump_height_cells");
            cursor.ReleaseWithVelocity(Vector2.up * cursor.JumpVelocity(height));
        }
    }
}
