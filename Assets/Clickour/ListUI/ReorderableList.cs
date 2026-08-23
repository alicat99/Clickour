using System.Collections.Generic;
using Clickour.Balance;
using Clickour.Core;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Clickour.ListUI
{
    public sealed class ReorderableList : MonoBehaviour
    {
        [SerializeField] bool horizontal;
        [SerializeField] List<Transform> items = new();

        readonly List<Vector3> slots = new();
        Transform dragged_item;
        CursorController active_cursor;
        Vector2 item_offset;
        int dragged_index;

        public void Configure(bool horizontal, List<Transform> items)
        {
            this.horizontal = horizontal;
            this.items = items;
            CacheSlots();
        }

        public void BeginDrag(Transform item, CursorController cursor)
        {
            CacheSlots();
            dragged_item = item;
            active_cursor = cursor;
            dragged_index = items.IndexOf(item);
            item_offset = (Vector2)item.position - cursor.Position;

            if (horizontal)
                cursor.BeginFixedHold(cursor.Position);
            else
                cursor.BeginSlowFall(cursor.Position.x,
                    BalanceDatabase.GetFloat("list_vertical_hold_gravity_scale"));
        }

        public void Drag(float delta_time)
        {
            if (horizontal)
            {
                var input = 0f;
                var keyboard = Keyboard.current;
                if (keyboard != null)
                {
                    if (keyboard.rightArrowKey.isPressed)
                        input += 1;
                    if (keyboard.leftArrowKey.isPressed)
                        input -= 1;
                }
                var speed = BalanceDatabase.GetFloat("list_horizontal_hold_speed");
                active_cursor.MoveFixedHold(active_cursor.Position + Vector2.right * (input * speed * delta_time));
            }

            dragged_item.position = active_cursor.Position + item_offset;
            var next_index = FindNearestSlot(dragged_item.localPosition);
            if (next_index == dragged_index)
                return;

            var displaced = items[next_index];
            displaced.DOLocalMove(slots[dragged_index], 0.12f);
            items[dragged_index] = displaced;
            items[next_index] = dragged_item;
            dragged_index = next_index;
        }

        public void EndDrag(CursorController cursor)
        {
            dragged_item.DOLocalMove(slots[dragged_index], 0.12f);
            var velocity = horizontal ? Vector2.zero : new Vector2(0, cursor.Body.linearVelocity.y);
            cursor.ReleaseWithVelocity(velocity);
            dragged_item = null;
            active_cursor = null;
        }

        int FindNearestSlot(Vector3 local_position)
        {
            var coordinate = horizontal ? local_position.x : local_position.y;
            var nearest = 0;
            var nearest_distance = float.MaxValue;
            for (var i = 0; i < slots.Count; i++)
            {
                var slot_coordinate = horizontal ? slots[i].x : slots[i].y;
                var distance = Mathf.Abs(coordinate - slot_coordinate);
                if (distance >= nearest_distance)
                    continue;
                nearest = i;
                nearest_distance = distance;
            }
            return nearest;
        }

        void CacheSlots()
        {
            if (slots.Count == items.Count)
                return;
            slots.Clear();
            foreach (var item in items)
                slots.Add(item.localPosition);
        }
    }
}
