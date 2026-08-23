using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Clickour.MapEditor
{
    public sealed class MapEditorElement :
        MonoBehaviour,
        IPointerDownHandler,
        IPointerUpHandler,
        IPointerClickHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler
    {
        [SerializeField] MapElementKind kind;
        [SerializeField] RectTransform rect;
        [SerializeField] Outline selection_outline;
        [SerializeField] GameObject resize_handles;

        MapEditorController controller;
        Vector2 drag_start_pointer;
        Vector2 drag_start_position;
        Vector2 resize_corner;
        Vector2 resize_start_size;
        string id;
        string owner_list_id;
        int owner_row = -1;
        bool resizing;

        public MapElementKind Kind => kind;
        public RectTransform Rect => rect;
        public string Id => id;
        public string OwnerListId => owner_list_id;
        public int OwnerRow => owner_row;

        public void Configure(
            MapElementKind kind,
            RectTransform rect,
            Outline selection_outline,
            GameObject resize_handles)
        {
            this.kind = kind;
            this.rect = rect;
            this.selection_outline = selection_outline;
            this.resize_handles = resize_handles;
        }

        public void Initialize(MapEditorController controller, string id)
        {
            this.controller = controller;
            this.id = id;
            owner_list_id = string.Empty;
            owner_row = -1;
            SetSelected(false);
        }

        public void OnPointerClick(PointerEventData event_data)
        {
            event_data.Use();
            controller.Select(this);
        }

        public void OnPointerDown(PointerEventData event_data)
        {
            if (TryGetResizeCorner(event_data, out var corner))
                BeginResize(event_data, corner);
        }

        public void OnPointerUp(PointerEventData event_data)
        {
            if (resizing)
                EndResize(event_data);
        }

        public void OnBeginDrag(PointerEventData event_data)
        {
            if (resizing)
                return;

            if (TryGetResizeCorner(event_data, out var corner))
            {
                BeginResize(event_data, corner);
                return;
            }

            controller.BeginElementMutation(this);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                controller.MapRoot, event_data.position, event_data.pressEventCamera, out drag_start_pointer);
            drag_start_position = controller.WorldToMapPosition(transform);
            rect.SetParent(controller.MapRoot, true);
            rect.anchoredPosition = drag_start_position;
            event_data.Use();
        }

        public void OnDrag(PointerEventData event_data)
        {
            if (resizing)
            {
                Resize(event_data);
                return;
            }

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                controller.MapRoot, event_data.position, event_data.pressEventCamera, out var pointer);
            rect.anchoredPosition = drag_start_position + pointer - drag_start_pointer;
            event_data.Use();
        }

        public void OnEndDrag(PointerEventData event_data)
        {
            if (resizing)
            {
                EndResize(event_data);
                return;
            }

            var allow_snap = !IsShiftPressed();
            controller.FinishElementMutation(this, allow_snap);
            event_data.Use();
        }

        public void BeginResize(PointerEventData event_data, Vector2 corner)
        {
            resizing = true;
            resize_corner = corner;
            controller.BeginElementMutation(this);
            rect.SetParent(controller.MapRoot, true);
            rect.anchoredPosition = controller.WorldToMapPosition(transform);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                controller.MapRoot,
                event_data.position,
                event_data.pressEventCamera,
                out drag_start_pointer);
            drag_start_position = rect.anchoredPosition;
            resize_start_size = rect.sizeDelta;
            event_data.Use();
        }

        public void Resize(PointerEventData event_data)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                controller.MapRoot,
                event_data.position,
                event_data.pressEventCamera,
                out var pointer);
            var world_delta = pointer - drag_start_pointer;
            var local_delta = Quaternion.Inverse(rect.localRotation) * world_delta;
            var requested = resize_start_size + Vector2.Scale(local_delta, resize_corner);
            var size = new Vector2(
                Mathf.Max(controller.CellSize, requested.x),
                Mathf.Max(controller.CellSize, requested.y));
            var center_delta = Vector2.Scale(size - resize_start_size, resize_corner) * 0.5f;
            rect.sizeDelta = size;
            rect.anchoredPosition = drag_start_position + (Vector2)(rect.localRotation * center_delta);
            RefreshLayout();
            event_data.Use();
        }

        public void EndResize(PointerEventData event_data)
        {
            if (!resizing)
                return;

            resizing = false;
            controller.FinishElementMutation(this, !IsShiftPressed());
            event_data.Use();
        }

        public void SetSelected(bool value)
        {
            if (selection_outline != null)
                selection_outline.enabled = value;
            if (resize_handles != null)
                resize_handles.SetActive(value);
        }

        public void SetListOwner(string list_id, int row_index, RectTransform row)
        {
            owner_list_id = list_id;
            owner_row = row_index;
            rect.SetParent(row, true);
            rect.SetAsLastSibling();
        }

        public void ClearListOwner(RectTransform map_root)
        {
            owner_list_id = string.Empty;
            owner_row = -1;
            rect.SetParent(map_root, true);
        }

        public void RefreshLayout()
        {
            var rows = GetComponent<MapListRows>();
            if (rows != null)
                rows.RefreshRows();
        }

        bool TryGetResizeCorner(PointerEventData event_data, out Vector2 corner)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rect, event_data.position, event_data.pressEventCamera, out var local);
            const float handle_radius = 28;
            var half_size = rect.rect.size * 0.5f;
            var near_x = Mathf.Abs(Mathf.Abs(local.x) - half_size.x) <= handle_radius;
            var near_y = Mathf.Abs(Mathf.Abs(local.y) - half_size.y) <= handle_radius;
            corner = new Vector2(Mathf.Sign(local.x), Mathf.Sign(local.y));
            return near_x && near_y;
        }

        static bool IsShiftPressed()
        {
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            return keyboard != null && (keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed);
        }
    }
}
