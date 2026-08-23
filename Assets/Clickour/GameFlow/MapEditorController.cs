using System.Collections.Generic;
using Clickour.Balance;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Clickour.GameFlow
{
    public sealed class MapEditorController : MonoBehaviour
    {
        static readonly Color[] CELL_COLORS =
        {
            new(0.93f, 0.95f, 0.97f),
            new(0.16f, 0.52f, 0.92f),
            new(0.26f, 0.28f, 0.31f),
            new(0.4f, 0.42f, 0.46f),
            new(0.55f, 0.57f, 0.61f),
            new(0.7f, 0.72f, 0.76f)
        };

        static readonly string[] CELL_NAMES =
        {
            "Empty", "Platform", "Button", "Switch", "Slider", "List"
        };

        [SerializeField] int width = 12;
        [SerializeField] int height = 7;
        [SerializeField] List<Image> cell_images = new();
        [SerializeField] TMP_Text status;

        byte[] cells;

        public void Configure(int width, int height, List<Image> cell_images, TMP_Text status)
        {
            this.width = width;
            this.height = height;
            this.cell_images = cell_images;
            this.status = status;
        }

        void OnEnable()
        {
            cells = MapCodec.Decode(BalanceDatabase.GetEncodedMap(), out var saved_width, out var saved_height);
            if (saved_width != width || saved_height != height)
                throw new UnityException("Map YAML size does not match the MapEditor Scene grid.");

            for (var i = 0; i < cell_images.Count; i++)
            {
                var index = i;
                cell_images[i].GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => Cycle(index));
                ApplyCell(i);
            }
        }

        void Cycle(int index)
        {
            cells[index] = (byte)((cells[index] + 1) % CELL_COLORS.Length);
            ApplyCell(index);
            BalanceDatabase.SetEncodedMap(MapCodec.Encode(width, height, cells));
            status.text = BalanceDatabase.Save()
                ? $"Saved: {CELL_NAMES[cells[index]]}"
                : $"Session: {CELL_NAMES[cells[index]]}";
        }

        void ApplyCell(int index) => cell_images[index].color = CELL_COLORS[cells[index]];
    }
}
