using UnityEngine;
using UnityEngine.InputSystem;

namespace NeonSurvivor
{
    /// <summary>Leitura direta de teclado + gamepad (controle Xbox) pelo Input System.</summary>
    public static class GameInput
    {
        static int stickNavFrame = -1;
        static int stickNavValue;
        static bool stickHeld;

        /// <summary>Usado pelo AutoTest para dirigir o jogador.</summary>
        public static Vector2? BotMove;

        public static Vector2 Move()
        {
            if (BotMove.HasValue) return BotMove.Value;
            var v = Vector2.zero;
            var k = Keyboard.current;
            if (k != null)
            {
                if (k.aKey.isPressed || k.leftArrowKey.isPressed) v.x -= 1;
                if (k.dKey.isPressed || k.rightArrowKey.isPressed) v.x += 1;
                if (k.sKey.isPressed || k.downArrowKey.isPressed) v.y -= 1;
                if (k.wKey.isPressed || k.upArrowKey.isPressed) v.y += 1;
            }
            var g = Gamepad.current;
            if (g != null)
            {
                var stick = g.leftStick.ReadValue();
                if (stick.sqrMagnitude > 0.04f) v += stick;
                v += g.dpad.ReadValue();
            }
            return Vector2.ClampMagnitude(v, 1f);
        }

        public static bool DashPressed()
        {
            var k = Keyboard.current;
            var g = Gamepad.current;
            return (k != null && (k.spaceKey.wasPressedThisFrame || k.leftShiftKey.wasPressedThisFrame))
                || (g != null && (g.buttonSouth.wasPressedThisFrame || g.rightTrigger.wasPressedThisFrame
                                  || g.rightShoulder.wasPressedThisFrame));
        }

        public static bool ConfirmPressed()
        {
            var k = Keyboard.current;
            var g = Gamepad.current;
            return (k != null && (k.enterKey.wasPressedThisFrame || k.numpadEnterKey.wasPressedThisFrame
                                  || k.spaceKey.wasPressedThisFrame))
                || (g != null && (g.buttonSouth.wasPressedThisFrame || g.startButton.wasPressedThisFrame));
        }

        /// <summary>-1 / +1 quando o jogador navega no menu (setas, A/D, D-pad, analógico).</summary>
        public static int NavPressed()
        {
            int nav = 0;
            var k = Keyboard.current;
            if (k != null)
            {
                if (k.leftArrowKey.wasPressedThisFrame || k.aKey.wasPressedThisFrame || k.upArrowKey.wasPressedThisFrame || k.wKey.wasPressedThisFrame) nav = -1;
                if (k.rightArrowKey.wasPressedThisFrame || k.dKey.wasPressedThisFrame || k.downArrowKey.wasPressedThisFrame || k.sKey.wasPressedThisFrame) nav = 1;
            }
            var g = Gamepad.current;
            if (g != null)
            {
                if (g.dpad.left.wasPressedThisFrame || g.dpad.up.wasPressedThisFrame) nav = -1;
                if (g.dpad.right.wasPressedThisFrame || g.dpad.down.wasPressedThisFrame) nav = 1;
                nav = nav != 0 ? nav : StickNav(g.leftStick.ReadValue().x);
            }
            return nav;
        }

        static int StickNav(float x)
        {
            if (stickNavFrame == Time.frameCount) return stickNavValue;
            stickNavFrame = Time.frameCount;
            stickNavValue = 0;
            if (Mathf.Abs(x) < 0.3f) stickHeld = false;
            else if (!stickHeld && Mathf.Abs(x) > 0.6f)
            {
                stickHeld = true;
                stickNavValue = x > 0 ? 1 : -1;
            }
            return stickNavValue;
        }

        /// <summary>0..2 se o jogador apertou 1, 2 ou 3; senão -1.</summary>
        public static int NumberPressed()
        {
            var k = Keyboard.current;
            if (k == null) return -1;
            if (k.digit1Key.wasPressedThisFrame || k.numpad1Key.wasPressedThisFrame) return 0;
            if (k.digit2Key.wasPressedThisFrame || k.numpad2Key.wasPressedThisFrame) return 1;
            if (k.digit3Key.wasPressedThisFrame || k.numpad3Key.wasPressedThisFrame) return 2;
            return -1;
        }
    }
}
