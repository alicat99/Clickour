using UnityEngine;
using UnityEngine.EventSystems;

namespace Clickour.MapEditor
{
    public sealed class MapResizeHandle :
        MonoBehaviour,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler
    {
        [SerializeField] MapEditorElement element;
        [SerializeField] Vector2 corner;

        Vector2 start_pointer;
        Vector2 start_position;
        Vector2 start_size;

        public void Configure(MapEditorElement element, Vector2 corner)
        {
            this.element = element;
            this.corner = corner;
        }

        public void OnBeginDrag(PointerEventData event_data)
        {
            var controller = element.GetComponentInParent<MapEditorController>();
            controller.BeginElementMutation(element);
            element.Rect.SetParent(controller.MapRoot, true);
            element.Rect.anchoredPosition = controller.WorldToMapPosition(element.transform);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                element.Rect.parent as RectTransform,
                event_data.position,
                event_data.pressEventCamera,
                out start_pointer);
            start_position = element.Rect.anchoredPosition;
            start_size = element.Rect.sizeDelta;
            event_data.Use();
        }

        public void OnDrag(PointerEventData event_data)
        {
            var parent = element.Rect.parent as RectTransform;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parent, event_data.position, event_data.pressEventCamera, out var pointer);
            var world_delta = pointer - start_pointer;
            var local_delta = Quaternion.Inverse(element.Rect.localRotation) * world_delta;
            var requested = start_size + Vector2.Scale(local_delta, corner);
            var cell_size = element.GetComponentInParent<MapEditorController>().CellSize;
            var size = new Vector2(
                Mathf.Max(cell_size, requested.x),
                Mathf.Max(cell_size, requested.y));
            var center_delta = Vector2.Scale(size - start_size, corner) * 0.5f;
            element.Rect.sizeDelta = size;
            element.Rect.anchoredPosition = start_position +
                (Vector2)(element.Rect.localRotation * center_delta);
            element.RefreshLayout();
            event_data.Use();
        }

        public void OnEndDrag(PointerEventData event_data)
        {
            var controller = element.GetComponentInParent<MapEditorController>();
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            var shift = keyboard != null &&
                (keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed);
            controller.FinishElementMutation(element, !shift);
            event_data.Use();
        }
    }
}
