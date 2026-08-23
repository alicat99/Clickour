using System;
using UnityEngine;

namespace Clickour.MapEditor
{
    [Serializable]
    public sealed class MapElementData
    {
        public string id;
        public MapElementKind kind;
        public Vector2 position;
        public Vector2 size;
        public int rotation;
        public string owner_list_id;
        public int owner_row = -1;
    }
}
