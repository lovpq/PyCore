using UnityEngine;
using UnityEngine.InputSystem;

namespace Core
{
    /// <summary>
    /// помощник для управления Input System
    /// 
    /// что делает этот скрипт:
    /// - Включает/выключает управление игроком
    /// - Управляет курсором (блокировка/разблокировка, видимость)
    /// - Опционально останавливает время при открытии UI
    /// - Помогает переключаться между режимами игры и UI
    /// 
    /// как использовать в unity:
    /// 1. Добавьте этот скрипт на объект игрока или в сцену
    /// 2. Назначьте PlayerInput компонент в Inspector
    /// 3. Вызывайте EnableGameplayInput() когда игрок должен управлять персонажем
    /// 4. Вызывайте DisableGameplayInput() когда открыто меню или UI
    /// 
    /// как работает:
    /// - PlayerInput.ActivateInput() включает ввод (движение, прыжки и т.д.)
    /// - Cursor.lockState управляет блокировкой курсора
    /// - Time.timeScale = 0 останавливает игру (если pauseOnTaskWindow = true)
    /// </summary>
    public class InputSystemHelper : MonoBehaviour
    {
        [Header("Input Settings")]
        // ссылка на компонент PlayerInput (управляет вводом игрока)
        [SerializeField] private PlayerInput playerInput;
        // нужно ли ставить игру на паузу при открытии окна задач
        [SerializeField] private bool pauseOnTaskWindow = true;

        /// <summary>
        /// вызывается Unity при запуске скрипта
        /// находит PlayerInput если не назначен
        /// </summary>
        private void Start()
        {
            // если PlayerInput не назначен в Inspector
            if (playerInput == null)
            {
                // пытаемся найти его на этом же объекте
                playerInput = GetComponent<PlayerInput>();
            }

           // если все еще не найден
            if (playerInput == null)
            {
                // выводим предупреждение
                Debug.LogWarning("PlayerInput component not found. Input System may not work correctly.");
            }
        }

        /// <summary>
        /// включает управление игроком (игровой режим)
        /// вызывается при закрытии меню или UI панелей
        /// </summary>
        public void EnableGameplayInput()
        {
            // если PlayerInput существует
            if (playerInput != null)
            {
                // активируем ввод (игрок может двигаться, прыгать и т.д.)
                playerInput.ActivateInput();
            }
            
            // блокируем курсор в центре экрана (для FPS камеры)
            // Locked = курсор невидим и закреплен в центре
            Cursor.lockState = CursorLockMode.Locked;
            // делаем курсор невидимым
            Cursor.visible = false;
            // восстанавливаем нормальную скорость времени (игра не на паузе)
            Time.timeScale = 1f;
        }

        /// <summary>
        /// отключает управление игроком (режим UI)
        /// вызывается при открытии меню или окна задач
        /// </summary>
        public void DisableGameplayInput()
        {
            // если PlayerInput существует И нужна пауза
            if (playerInput != null && pauseOnTaskWindow)
            {
                // деактивируем ввод (игрок не может двигаться)
                playerInput.DeactivateInput();
            }
            
            // разблокируем курсор (можно двигать мышью)
            // None = курсор свободно двигается
            Cursor.lockState = CursorLockMode.None;
            // делаем курсор видимым
            Cursor.visible = true;
            
            // если нужна пауза при открытии окон
            if (pauseOnTaskWindow)
            {
                // останавливаем время (Time.deltaTime станет 0)
                // физика и большинство Update() остановятся
                Time.timeScale = 0f;
            }
        }
    }
}
