using System;
using System.Text;
using UnityEngine;

namespace Clickour.MapEditor
{
    public static class MapCodec
    {
        public static string Encode(MapDocument document)
        {
            var json = JsonUtility.ToJson(document);
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
        }

        public static MapDocument Decode(string encoded)
        {
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
            return JsonUtility.FromJson<MapDocument>(json);
        }
    }
}
