using UnityEngine;
using UnityEngine.InputSystem;
using EasyPeasyFirstPersonController;

namespace Core
{
    /// <summary>
    /// Централизованное управление вводом, курсором и Time.timeScale.
    /// Синглтон. Все системы (NeedsManager, LocationManager, SimplePythonUI)
    /// переключают режим только через этот класс.
    ///
    /// EnableGameplayInput()   — игровой режим: курсор скрыт, время идёт.
    /// EnableUIMode()          — UI без паузы: курсор видим, время идёт (магазин, сон).
    /// EnableUIModeWithPause() — UI с паузой: курсор видим, Time.timeScale = 0 (модалки).
    ///
    /// ВАЖНО: НЕ используем DeactivateInput() — он отключает InputSystemUIInputModule
    /// и ломает клики кнопок. Вместо этого отключаем FPS контроллер напрямую.
    /// </summary>
    public class InputSystemHelper : MonoBehaviour
    {
        public static InputSystemHelper Instance { get; private set; }

        [SerializeField] private PlayerInput playerInput;

        private FirstPersonController fpsController;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            if (playerInput == null)
                playerInput = GetComponent<PlayerInput>();
        }

        private void Start()
        {
            fpsController = FindFirstObjectByType<FirstPersonController>();

            if (playerInput == null)
                Debug.LogWarning("InputSystemHelper: PlayerInput не назначен и не найден на объекте.");

            if (fpsController == null)
                Debug.LogWarning("InputSystemHelper: FirstPersonController не найден в сцене.");
        }

        /// <summary>Игровой режим: курсор заблокирован, время идёт, ввод активен.</summary>
        public void EnableGameplayInput()
        {
            SetFPSControls(true);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1f;
        }

        /// <summary>UI-режим без паузы времени (магазин, панель сна, инфо).</summary>
        public void EnableUIMode()
        {
            // Не вызываем DeactivateInput() — это убивает обработку кликов мышью в UI.
            // Вместо этого просто отключаем FPS контроллер (взгляд + движение).
            SetFPSControls(false);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        /// <summary>UI-режим с паузой времени (модальные окна: телепорт, победа).</summary>
        public void EnableUIModeWithPause()
        {
            EnableUIMode();
            Time.timeScale = 0f;
        }

        /// <summary>Устаревший псевдоним — оставлен для совместимости.</summary>
        public void DisableGameplayInput() => EnableUIMode();

        private void SetFPSControls(bool enabled)
        {
            if (fpsController == null) return;
            fpsController.SetLookControl(enabled);
            fpsController.SetMoveControl(enabled);
        }
    }
}
