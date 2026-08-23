using System;
using System.Collections.Generic;
using Clickour.Balance;
using Clickour.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Clickour.MapEditor
{
    public sealed class MapEditorController : MonoBehaviour
    {
        [SerializeField] RectTransform map_root;
        [SerializeField] TMP_Text status;
        [SerializeField] Toggle snap_toggle;
        [SerializeField] float cell_size = 72;
        [SerializeField] int width = 12;
        [SerializeField] int height = 7;
        [SerializeField] List<MapEditorElement> element_prefabs = new();

        readonly Stack<string> undo_history = new();
        readonly Stack<string> redo_history = new();
        readonly Dictionary<MapElementKind, MapEditorElement> prefabs = new();

        MapEditorElement selected;
        bool initialized;
        bool mutation_open;

        public RectTransform MapRoot => map_root;
        public float CellSize => cell_size;
        public bool SnapEnabled => snap_toggle == null || snap_toggle.isOn;

        public void Configure(
            RectTransform map_root,
            TMP_Text status,
            Toggle snap_toggle,
            float cell_size,
            int width,
            int height,
            List<MapEditorElement> element_prefabs)
        {
            this.map_root = map_root;
            this.status = status;
            this.snap_toggle = snap_toggle;
            this.cell_size = cell_size;
            this.width = width;
            this.height = height;
            this.element_prefabs = element_prefabs;
        }

        void Update()
        {
            if (!initialized)
            {
                if (!BalanceDatabase.IsLoaded)
                    return;
                Initialize();
            }

            HandleShortcuts();
        }

        public void DropFromPalette(MapElementKind kind, Vector2 screen_position, Camera event_camera)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    map_root, screen_position, event_camera, out var map_position) ||
                !map_root.rect.Contains(map_position))
            {
                SetStatus("Drop inside the map canvas");
                return;
            }

            BeginMutation();
            var element = CreateElement(kind, Guid.NewGuid().ToString("N"));
            element.Rect.anchoredPosition = map_position;
            SnapAndClamp(element, !IsShiftPressed());
            Select(element);
            CommitMutation($"{kind} placed");
        }

        public void Select(MapEditorElement element)
        {
            if (selected == element)
                return;
            if (selected != null)
                selected.SetSelected(false);

            selected = element;
            if (selected != null)
            {
                selected.SetSelected(true);
                SetStatus($"{selected.Kind} selected · drag, corner resize, R rotate");
            }
        }

        public void BeginElementMutation(MapEditorElement element)
        {
            BeginMutation();
            DetachFromList(element);
            Select(element);
        }

        public void FinishElementMutation(MapEditorElement element, bool allow_snap)
        {
            SnapAndClamp(element, allow_snap);
            EvaluateListOwnership(element);
            CommitMutation($"{element.Kind} updated");
        }

        public void CancelSelection()
        {
            if (selected != null)
                selected.SetSelected(false);
            selected = null;
            SetStatus("Selection cleared");
        }

        public Vector2 WorldToMapPosition(Transform target) =>
            map_root.InverseTransformPoint(target.position);

        #region Initialization

        void Initialize()
        {
            foreach (var prefab in element_prefabs)
                prefabs.Add(prefab.Kind, prefab);

            LoadDocument(MapCodec.Decode(BalanceDatabase.GetEncodedMap()));
            initialized = true;
            SetStatus("Drag a component from the right palette");
        }

        void LoadDocument(MapDocument document)
        {
            ClearElements();
            width = document.width;
            height = document.height;

            var elements_by_id = new Dictionary<string, MapEditorElement>();
            foreach (var data in document.elements)
            {
                var element = CreateElement(data.kind, data.id);
                element.Rect.anchoredPosition = data.position * cell_size;
                element.Rect.sizeDelta = data.size * cell_size;
                element.Rect.localEulerAngles = new Vector3(0, 0, data.rotation);
                element.RefreshLayout();
                elements_by_id.Add(data.id, element);
            }

            for (var i = 0; i < document.elements.Count; i++)
            {
                var data = document.elements[i];
                if (string.IsNullOrEmpty(data.owner_list_id) ||
                    !elements_by_id.TryGetValue(data.owner_list_id, out var list))
                    continue;

                var rows = list.GetComponent<MapListRows>();
                if (rows == null || !rows.TryGetRow(data.owner_row, out var row))
                    continue;

                elements_by_id[data.id].SetListOwner(list.Id, data.owner_row, row);
            }

            NormalizeLayerOrder();
        }

        MapEditorElement CreateElement(MapElementKind kind, string id)
        {
            var element = Instantiate(prefabs[kind], map_root);
            element.gameObject.SetActive(true);
            element.Initialize(this, id);
            element.RefreshLayout();
            return element;
        }

        void ClearElements()
        {
            selected = null;
            foreach (var element in map_root.GetComponentsInChildren<MapEditorElement>(true))
                if (element != null)
                {
                    element.transform.SetParent(null, true);
                    Destroy(element.gameObject);
                }
        }

        #endregion

        #region Shortcuts

        void HandleShortcuts()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
                return;

            var control = keyboard.leftCtrlKey.isPressed || keyboard.rightCtrlKey.isPressed;
            if (control && keyboard.zKey.wasPressedThisFrame)
            {
                Undo();
                return;
            }
            if (control && keyboard.yKey.wasPressedThisFrame)
            {
                Redo();
                return;
            }
            if (control && keyboard.dKey.wasPressedThisFrame && selected != null)
            {
                DuplicateSelected();
                return;
            }
            if (keyboard.escapeKey.wasPressedThisFrame)
            {
                CancelSelection();
                return;
            }
            if (selected == null)
                return;

            if (keyboard.deleteKey.wasPressedThisFrame || keyboard.backspaceKey.wasPressedThisFrame)
            {
                DeleteSelected();
                return;
            }
            if (keyboard.rKey.wasPressedThisFrame)
            {
                RotateSelected();
                return;
            }

            var x = DirectionalInput.ComposeAxis(
                keyboard.leftArrowKey.wasPressedThisFrame || keyboard.aKey.wasPressedThisFrame,
                keyboard.rightArrowKey.wasPressedThisFrame || keyboard.dKey.wasPressedThisFrame);
            var y = DirectionalInput.ComposeAxis(
                keyboard.downArrowKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame,
                keyboard.upArrowKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame);
            if (x != 0 || y != 0)
                NudgeSelected(new Vector2(x, y), IsShiftPressed());
        }

        void RotateSelected()
        {
            BeginMutation();
            DetachFromList(selected);
            var angle = Mathf.RoundToInt(selected.Rect.localEulerAngles.z / 90) * 90 + 90;
            selected.Rect.localEulerAngles = new Vector3(0, 0, angle % 360);
            SnapAndClamp(selected, true);
            selected.RefreshLayout();
            EvaluateListOwnership(selected);
            CommitMutation($"{selected.Kind} rotated 90°");
        }

        void NudgeSelected(Vector2 direction, bool fine)
        {
            BeginMutation();
            DetachFromList(selected);
            selected.Rect.anchoredPosition += direction * (cell_size * (fine ? 0.25f : 1));
            SnapAndClamp(selected, !fine);
            EvaluateListOwnership(selected);
            CommitMutation($"{selected.Kind} moved");
        }

        void DuplicateSelected()
        {
            BeginMutation();
            var source = selected;
            var duplicate = CreateElement(source.Kind, Guid.NewGuid().ToString("N"));
            duplicate.Rect.anchoredPosition = WorldToMapPosition(source.transform) + Vector2.one * cell_size;
            duplicate.Rect.sizeDelta = source.Rect.sizeDelta;
            duplicate.Rect.localEulerAngles = source.transform.eulerAngles;
            duplicate.RefreshLayout();
            SnapAndClamp(duplicate, true);
            Select(duplicate);
            EvaluateListOwnership(duplicate);
            CommitMutation($"{duplicate.Kind} duplicated");
        }

        void DeleteSelected()
        {
            BeginMutation();
            var label = selected.Kind.ToString();
            var removed = selected;
            if (removed.Kind == MapElementKind.List)
            {
                foreach (var child in removed.GetComponentsInChildren<MapEditorElement>(true))
                    if (child != removed)
                        child.ClearListOwner(map_root);
            }

            removed.transform.SetParent(null, true);
            Destroy(removed.gameObject);
            selected = null;
            CommitMutation($"{label} deleted · Ctrl+Z to restore");
        }

        #endregion

        #region History

        void BeginMutation()
        {
            if (mutation_open)
                return;

            undo_history.Push(MapCodec.Encode(CaptureDocument()));
            if (undo_history.Count > 50)
            {
                var retained = undo_history.ToArray();
                undo_history.Clear();
                for (var i = 48; i >= 0; i--)
                    undo_history.Push(retained[i]);
            }
            redo_history.Clear();
            mutation_open = true;
        }

        void CommitMutation(string message)
        {
            mutation_open = false;
            NormalizeLayerOrder();
            var encoded = MapCodec.Encode(CaptureDocument());
            BalanceDatabase.SetEncodedMap(encoded);
            var saved = BalanceDatabase.Save();
            SetStatus($"{(saved ? "Saved" : "Session")} · {message}");
        }

        void Undo()
        {
            if (undo_history.Count == 0)
            {
                SetStatus("Nothing to undo");
                return;
            }

            redo_history.Push(MapCodec.Encode(CaptureDocument()));
            var encoded = undo_history.Pop();
            LoadDocument(MapCodec.Decode(encoded));
            BalanceDatabase.SetEncodedMap(encoded);
            BalanceDatabase.Save();
            SetStatus("Undo");
        }

        void Redo()
        {
            if (redo_history.Count == 0)
            {
                SetStatus("Nothing to redo");
                return;
            }

            undo_history.Push(MapCodec.Encode(CaptureDocument()));
            var encoded = redo_history.Pop();
            LoadDocument(MapCodec.Decode(encoded));
            BalanceDatabase.SetEncodedMap(encoded);
            BalanceDatabase.Save();
            SetStatus("Redo");
        }

        MapDocument CaptureDocument()
        {
            var document = new MapDocument { width = width, height = height };
            foreach (var element in map_root.GetComponentsInChildren<MapEditorElement>(true))
            {
                if (element == null)
                    continue;

                var position = WorldToMapPosition(element.transform) / cell_size;
                var rotation = Mathf.RoundToInt(
                    Mathf.Repeat(element.transform.eulerAngles.z - map_root.eulerAngles.z, 360));
                document.elements.Add(new MapElementData
                {
                    id = element.Id,
                    kind = element.Kind,
                    position = position,
                    size = element.Rect.sizeDelta / cell_size,
                    rotation = rotation,
                    owner_list_id = element.OwnerListId,
                    owner_row = element.OwnerRow
                });
            }
            return document;
        }

        #endregion

        #region Layout

        void SnapAndClamp(MapEditorElement element, bool allow_snap)
        {
            if (allow_snap && SnapEnabled)
            {
                var size = element.Rect.sizeDelta;
                size.x = Mathf.Max(cell_size, Mathf.Round(size.x / cell_size) * cell_size);
                size.y = Mathf.Max(cell_size, Mathf.Round(size.y / cell_size) * cell_size);
                element.Rect.sizeDelta = size;

                var map_rect = map_root.rect;
                var position = element.Rect.anchoredPosition;
                position.x = Mathf.Round((position.x - map_rect.xMin - size.x * 0.5f) / cell_size) *
                    cell_size + map_rect.xMin + size.x * 0.5f;
                position.y = Mathf.Round((position.y - map_rect.yMin - size.y * 0.5f) / cell_size) *
                    cell_size + map_rect.yMin + size.y * 0.5f;
                element.Rect.anchoredPosition = position;
            }

            var rotation = Mathf.RoundToInt(element.Rect.localEulerAngles.z / 90) % 2;
            var extents = rotation == 0
                ? element.Rect.sizeDelta * 0.5f
                : new Vector2(element.Rect.sizeDelta.y, element.Rect.sizeDelta.x) * 0.5f;
            var clamped = element.Rect.anchoredPosition;
            clamped.x = Mathf.Clamp(clamped.x, map_root.rect.xMin + extents.x, map_root.rect.xMax - extents.x);
            clamped.y = Mathf.Clamp(clamped.y, map_root.rect.yMin + extents.y, map_root.rect.yMax - extents.y);
            element.Rect.anchoredPosition = clamped;
            element.RefreshLayout();
        }

        void DetachFromList(MapEditorElement element)
        {
            if (element.OwnerRow < 0)
                return;
            element.ClearListOwner(map_root);
        }

        void EvaluateListOwnership(MapEditorElement element)
        {
            if (element.Kind == MapElementKind.List)
                return;

            foreach (var list in map_root.GetComponentsInChildren<MapListRows>(true))
            {
                if (!list.TryFindContainingRow(element.Rect, out var row, out var row_index))
                    continue;

                var owner = list.GetComponent<MapEditorElement>();
                element.SetListOwner(owner.Id, row_index, row);
                SetStatus($"{element.Kind} belongs to list row {row_index + 1}");
                return;
            }
        }

        void NormalizeLayerOrder()
        {
            var top_level = new List<MapEditorElement>();
            foreach (var element in map_root.GetComponentsInChildren<MapEditorElement>(true))
                if (element != null && element.transform.parent == map_root)
                    top_level.Add(element);

            foreach (var element in top_level)
                if (element.Kind == MapElementKind.List)
                    element.transform.SetAsFirstSibling();
            foreach (var element in top_level)
                if (element.Kind != MapElementKind.List)
                    element.transform.SetAsLastSibling();
        }

        static bool IsShiftPressed()
        {
            var keyboard = Keyboard.current;
            return keyboard != null && (keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed);
        }

        void SetStatus(string message)
        {
            if (status != null)
                status.text = message;
        }

        #endregion
    }
}
