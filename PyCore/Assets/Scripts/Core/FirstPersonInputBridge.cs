using UnityEngine;
using UnityEngine.InputSystem;
using EasyPeasyFirstPersonController;

namespace Core
{
    [RequireComponent(typeof(FirstPersonController))]
    public class FirstPersonInputBridge : MonoBehaviour
    {
        private FirstPersonController controller;
        private PlayerInput playerInput;
        private Vector2 moveInput;
        private Vector2 lookInput;
        private bool jumpInput;
        private bool sprintInput;
        private bool crouchInput;

        private void Awake()
        {
            controller = GetComponent<FirstPersonController>();
        }

        private void Update()
        {
            if (Keyboard.current == null || Mouse.current == null) return;

            moveInput.x = 0f;
            moveInput.y = 0f;

            if (Keyboard.current.wKey.isPressed) moveInput.y = 1f;
            if (Keyboard.current.sKey.isPressed) moveInput.y = -1f;
            if (Keyboard.current.aKey.isPressed) moveInput.x = -1f;
            if (Keyboard.current.dKey.isPressed) moveInput.x = 1f;

            lookInput = Mouse.current.delta.ReadValue();
            jumpInput = Keyboard.current.spaceKey.isPressed;
            sprintInput = Keyboard.current.leftShiftKey.isPressed;
            crouchInput = Keyboard.current.leftCtrlKey.isPressed;
        }

        public Vector2 GetMoveInput() => moveInput;
        public Vector2 GetLookInput() => lookInput;
        public bool IsJumpPressed() => jumpInput;
        public bool IsSprintPressed() => sprintInput;
        public bool IsCrouchPressed() => crouchInput;
        public bool WasJumpPressedThisFrame() => Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
        public bool WasCrouchPressedThisFrame() => Keyboard.current != null && Keyboard.current.leftCtrlKey.wasPressedThisFrame;
    }
}
