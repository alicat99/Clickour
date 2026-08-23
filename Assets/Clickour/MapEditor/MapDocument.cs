using System;
using System.Collections.Generic;

namespace Clickour.MapEditor
{
    [Serializable]
    public sealed class MapDocument
    {
        public int width = 12;
        public int height = 7;
        public List<MapElementData> elements = new();
    }
}
