using UnityEngine;
using UnityEngine.EventSystems;

namespace Clickour.MapEditor
{
    public sealed class MapResizeHandle :
        MonoBehaviour,
        IPointerDownHandler,
        IPointerUpHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler
    {
        [SerializeField] MapEditorElement element;
        [SerializeField] Vector2 corner;

        bool resizing;

        public void Configure(MapEditorElement element, Vector2 corner)
        {
            this.element = element;
            this.corner = corner;
        }

        public void OnPointerDown(PointerEventData event_data)
        {
            resizing = true;
            element.BeginResize(event_data, corner);
        }

        public void OnPointerUp(PointerEventData event_data)
        {
            FinishResize(event_data);
        }

        public void OnBeginDrag(PointerEventData event_data)
        {
            if (!resizing)
            {
                resizing = true;
                element.BeginResize(event_data, corner);
            }
        }

        public void OnDrag(PointerEventData event_data)
        {
            element.Resize(event_data);
        }

        public void OnEndDrag(PointerEventData event_data)
        {
            FinishResize(event_data);
        }

        void FinishResize(PointerEventData event_data)
        {
            if (!resizing)
                return;

            resizing = false;
            element.EndResize(event_data);
        }
    }
}
