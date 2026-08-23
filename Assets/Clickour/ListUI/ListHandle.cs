using Clickour.Core;
using UnityEngine;

namespace Clickour.ListUI
{
    public sealed class ListHandle : MonoBehaviour, ICursorClickTarget
    {
        [SerializeField] ReorderableList owner;
        [SerializeField] Transform item;

        public bool HoldsCursor => true;

        public void Configure(ReorderableList owner, Transform item)
        {
            this.owner = owner;
            this.item = item;
        }

        public void Press(CursorController cursor, Collider2D clicked_collider) => owner.BeginDrag(item, cursor);

        public void Hold(CursorController cursor, float delta_time) => owner.Drag(delta_time);

        public void Release(CursorController cursor) => owner.EndDrag(cursor);
    }
}
