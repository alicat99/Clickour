using System;
using System.Text;

namespace Clickour.GameFlow
{
    public static class MapCodec
    {
        public static string Encode(int width, int height, byte[] cells)
        {
            var values = new StringBuilder(cells.Length);
            foreach (var cell in cells)
                values.Append((char)('0' + cell));
            var raw = $"{width}x{height}|{values}";
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));
        }

        public static byte[] Decode(string encoded, out int width, out int height)
        {
            var raw = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
            var sections = raw.Split('|');
            var size = sections[0].Split('x');
            width = int.Parse(size[0]);
            height = int.Parse(size[1]);
            var cells = new byte[sections[1].Length];
            for (var i = 0; i < cells.Length; i++)
                cells[i] = (byte)(sections[1][i] - '0');
            return cells;
        }
    }
}
