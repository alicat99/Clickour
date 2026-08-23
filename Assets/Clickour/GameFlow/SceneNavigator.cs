using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Clickour.GameFlow
{
    public sealed class SceneNavigator : MonoBehaviour
    {
        [SerializeField] bool debug_shortcut_enabled;

        void Update()
        {
            var keyboard = Keyboard.current;
            if (debug_shortcut_enabled && keyboard != null && keyboard.backquoteKey.wasPressedThisFrame)
                OpenDebug();
        }

        public void OpenGame() => SceneManager.LoadScene("Game");

        public void OpenDebug() => SceneManager.LoadScene("Debug");

        public void OpenMapEditor() => SceneManager.LoadScene("MapEditor");

        public void Configure(bool debug_shortcut_enabled) => this.debug_shortcut_enabled = debug_shortcut_enabled;
    }
}
