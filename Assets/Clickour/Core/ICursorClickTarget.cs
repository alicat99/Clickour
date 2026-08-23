using UnityEngine;

namespace Clickour.Core
{
    public interface ICursorClickTarget
    {
        bool HoldsCursor { get; }
        void Press(CursorController cursor, Collider2D clicked_collider);
        void Hold(CursorController cursor, float delta_time);
        void Release(CursorController cursor);
    }
}
