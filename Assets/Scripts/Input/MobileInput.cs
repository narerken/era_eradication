using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Emberfall.Input
{

    public static class MobileInput
    {
        public static float Horizontal;
        public static bool JumpHeld;
        public static bool JumpPressedThisFrame;

        private static bool _touchJumpHeld;
        private static bool _kbJumpHeld;

        private static float _touchHorizontal;
        private static float _kbHorizontal;

        public static void SetMove(float x)
        {
            _touchHorizontal = Mathf.Clamp(x, -1f, 1f);
            RecomputeHorizontal();
        }

        private static void RecomputeHorizontal()
        {
            Horizontal = Mathf.Abs(_kbHorizontal) > 0.01f ? _kbHorizontal : _touchHorizontal;
        }

        public static void SetJumpHeld(bool held)
        {
            _touchJumpHeld = held;
            RecomputeJumpHeld();
        }

        public static void PulseJump()
        {
            SetJumpHeld(true);
            SetJumpHeld(false);
        }

        public static void ConsumeJumpPressed()
        {
            JumpPressedThisFrame = false;
        }

        public static void EndFrame()
        {
#if ENABLE_INPUT_SYSTEM

            var kb = Keyboard.current;
            if (kb != null)
            {
                float kx = 0f;
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) kx -= 1f;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) kx += 1f;

                _kbHorizontal = kx;
                RecomputeHorizontal();

                _kbJumpHeld = kb.spaceKey.isPressed || kb.upArrowKey.isPressed || kb.wKey.isPressed;
                RecomputeJumpHeld();
            }
#endif
        }

        private static void RecomputeJumpHeld()
        {
            bool wasHeld = JumpHeld;
            JumpHeld = _touchJumpHeld || _kbJumpHeld;
            if (JumpHeld && !wasHeld) JumpPressedThisFrame = true;
        }
    }
}
