using UnityEngine;
using UnityEngine.InputSystem;

namespace Clickour.Core
{
    public static class DirectionalInput
    {
        public static Vector2 Read()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
                return Vector2.zero;

            return new Vector2(
                ComposeAxis(
                    keyboard.leftArrowKey.isPressed || keyboard.aKey.isPressed,
                    keyboard.rightArrowKey.isPressed || keyboard.dKey.isPressed),
                ComposeAxis(
                    keyboard.downArrowKey.isPressed || keyboard.sKey.isPressed,
                    keyboard.upArrowKey.isPressed || keyboard.wKey.isPressed));
        }

        public static float ComposeAxis(bool negative, bool positive) =>
            (positive ? 1f : 0f) - (negative ? 1f : 0f);
    }
}
