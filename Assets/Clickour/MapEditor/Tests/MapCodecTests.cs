using NUnit.Framework;
using UnityEngine;

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
    }
}
