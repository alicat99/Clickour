using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Clickour.MapEditor
{
    public sealed class MapPaletteItem :
        MonoBehaviour,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler
    {
        [SerializeField] MapEditorController controller;
        [SerializeField] MapElementKind kind;
        [SerializeField] Image preview;

        RectTransform ghost;

        public void Configure(MapEditorController controller, MapElementKind kind, Image preview)
        {
            this.controller = controller;
            this.kind = kind;
            this.preview = preview;
        }

        public void OnBeginDrag(PointerEventData event_data)
        {
            var ghost_object = new GameObject("Drag Preview", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            ghost = ghost_object.GetComponent<RectTransform>();
            ghost.SetParent(GetComponentInParent<Canvas>().transform, false);
            ghost.sizeDelta = preview.rectTransform.rect.size;
            var image = ghost_object.GetComponent<Image>();
            image.sprite = preview.sprite;
            image.type = preview.type;
            image.color = new Color(preview.color.r, preview.color.g, preview.color.b, 0.72f);
            ghost_object.GetComponent<CanvasGroup>().blocksRaycasts = false;
            ghost.position = event_data.position;
        }

        public void OnDrag(PointerEventData event_data)
        {
            ghost.position = event_data.position;
        }

        public void OnEndDrag(PointerEventData event_data)
        {
            controller.DropFromPalette(kind, event_data.position, event_data.pressEventCamera);
            Destroy(ghost.gameObject);
            ghost = null;
        }
    }
}
