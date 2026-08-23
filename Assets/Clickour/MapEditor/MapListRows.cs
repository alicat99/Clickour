using System.Collections.Generic;
using UnityEngine;

namespace Clickour.MapEditor
{
    public sealed class MapListRows : MonoBehaviour
    {
        [SerializeField] List<RectTransform> rows = new();

        public void Configure(List<RectTransform> rows)
        {
            this.rows = rows;
            RefreshRows();
        }

        public void RefreshRows()
        {
            for (var i = 0; i < rows.Count; i++)
            {
                var bottom = 1f - (i + 1f) / rows.Count;
                var top = 1f - i / (float)rows.Count;
                rows[i].anchorMin = new Vector2(0, bottom);
                rows[i].anchorMax = new Vector2(1, top);
                rows[i].offsetMin = Vector2.zero;
                rows[i].offsetMax = Vector2.zero;
            }
        }

        public bool TryGetRow(int index, out RectTransform row)
        {
            if (index >= 0 && index < rows.Count)
            {
                row = rows[index];
                return true;
            }

            row = null;
            return false;
        }

        public bool TryFindContainingRow(
            RectTransform element,
            out RectTransform containing_row,
            out int row_index)
        {
            var corners = new Vector3[4];
            element.GetWorldCorners(corners);
            for (var i = 0; i < rows.Count; i++)
            {
                var contains_all = true;
                foreach (var corner in corners)
                {
                    var local = rows[i].InverseTransformPoint(corner);
                    if (Contains(rows[i].rect, local))
                        continue;
                    contains_all = false;
                    break;
                }

                if (!contains_all)
                    continue;
                containing_row = rows[i];
                row_index = i;
                return true;
            }

            containing_row = null;
            row_index = -1;
            return false;
        }

        static bool Contains(Rect rect, Vector2 point)
        {
            const float tolerance = 0.25f;
            return point.x >= rect.xMin - tolerance &&
                point.x <= rect.xMax + tolerance &&
                point.y >= rect.yMin - tolerance &&
                point.y <= rect.yMax + tolerance;
        }
    }
}
