using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Clickour.MapEditor
{
    public sealed class MapEditorElement :
        MonoBehaviour,
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
        string id;
        string owner_list_id;
        int owner_row = -1;

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

        public void OnBeginDrag(PointerEventData event_data)
        {
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
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                controller.MapRoot, event_data.position, event_data.pressEventCamera, out var pointer);
            rect.anchoredPosition = drag_start_position + pointer - drag_start_pointer;
            event_data.Use();
        }

        public void OnEndDrag(PointerEventData event_data)
        {
            var allow_snap = !IsShiftPressed();
            controller.FinishElementMutation(this, allow_snap);
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

        static bool IsShiftPressed()
        {
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            return keyboard != null && (keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed);
        }
    }
}
