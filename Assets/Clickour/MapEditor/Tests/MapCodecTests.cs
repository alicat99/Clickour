using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Clickour.MapEditor.Tests
{
    public sealed class MapCodecTests
    {
        [Test]
        public void RoundTripPreservesElementLayoutAndOwnership()
        {
            var source = new MapDocument { width = 16, height = 9 };
            source.elements.Add(new MapElementData
            {
                id = "switch-1",
                kind = MapElementKind.Switch,
                position = new Vector2(3, 4),
                size = new Vector2(2, 1),
                rotation = 90,
                owner_list_id = "list-1",
                owner_row = 2
            });

            var decoded = MapCodec.Decode(MapCodec.Encode(source));
            var element = decoded.elements[0];

            Assert.That(decoded.width, Is.EqualTo(16));
            Assert.That(decoded.height, Is.EqualTo(9));
            Assert.That(element.id, Is.EqualTo("switch-1"));
            Assert.That(element.kind, Is.EqualTo(MapElementKind.Switch));
            Assert.That(element.position, Is.EqualTo(new Vector2(3, 4)));
            Assert.That(element.size, Is.EqualTo(new Vector2(2, 1)));
            Assert.That(element.rotation, Is.EqualTo(90));
            Assert.That(element.owner_list_id, Is.EqualTo("list-1"));
            Assert.That(element.owner_row, Is.EqualTo(2));
        }

        [Test]
        public void ExactRowBoundsCountAsContained()
        {
            var list_object = new GameObject("List", typeof(RectTransform), typeof(MapListRows));
            var element_object = new GameObject("Element", typeof(RectTransform));
            try
            {
                var list_rect = list_object.GetComponent<RectTransform>();
                list_rect.sizeDelta = new Vector2(360, 216);
                var rows = new List<RectTransform>();
                for (var i = 0; i < 3; i++)
                {
                    var row = new GameObject($"Row {i + 1}", typeof(RectTransform))
                        .GetComponent<RectTransform>();
                    row.SetParent(list_rect, false);
                    rows.Add(row);
                }

                var list_rows = list_object.GetComponent<MapListRows>();
                list_rows.Configure(rows);
                LayoutRebuilder.ForceRebuildLayoutImmediate(list_rect);

                var element = element_object.GetComponent<RectTransform>();
                element.SetParent(list_rect, false);
                element.sizeDelta = new Vector2(360, 72);
                element.anchoredPosition = new Vector2(0, 72);

                Assert.That(list_rows.TryFindContainingRow(element, out _, out var row_index), Is.True);
                Assert.That(row_index, Is.EqualTo(0));
            }
            finally
            {
                Object.DestroyImmediate(element_object);
                Object.DestroyImmediate(list_object);
            }
        }
    }
}
