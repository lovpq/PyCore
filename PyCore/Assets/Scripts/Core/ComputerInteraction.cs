using UnityEngine;

namespace Core
{
    /// <summary>
    /// обрабатывает взаимодействие с компьютером
    /// 
    /// что делает этот скрипт:
    /// - Подписывается на событие взаимодействия компонента Interactable
    /// - Открывает панель с задачами на Python при клике на компьютер
    /// - Связывает объект компьютера с SimplePythonUI
    /// 
    /// как использовать в unity:
    /// 1. Добавьте этот скрипт на объект компьютера (вместе с Interactable)
    /// 2. Скрипт автоматически настроится при запуске
    /// 3. При клике E на компьютер откроется панель задач
    /// 
    /// как работает:
    /// - При запуске находит компонент Interactable на том же объекте
    /// - Подписывается на событие OnInteract
    /// - Когда игрок взаимодействует с компьютером, вызывается OpenTaskWindow()
    /// - OpenTaskWindow() находит SimplePythonUI и открывает его
    /// </summary>
    public class ComputerInteraction : MonoBehaviour
    {
        /// <summary>
        /// вызывается Unity при запуске скрипта
        /// подписывается на событие взаимодействия
        /// </summary>
        private void Start()
        {
            // получаем компонент Interactable на этом же объекте
            Interactable interactable = GetComponent<Interactable>();
            // если компонент найден
            if (interactable != null)
            {
                // подписываемся на событие OnInteract
                // AddListener добавляет функцию OpenTaskWindow к списку вызываемых функций
                // когда игрок нажмет E рядом с компьютером, вызовется OpenTaskWindow
                interactable.OnInteract.AddListener(OpenTaskWindow);
            }
        }

        /// <summary>
        /// вызывается при взаимодействии с компьютером (нажатие E)
        /// открывает панель задач на Python
        /// </summary>
        private void OpenTaskWindow()
        {
            // выводим сообщение в консоль для отладки
            Debug.Log("ComputerInteraction: OpenTaskWindow called!");
            
            // ищем SimplePythonUI в сцене
            // FindFirstObjectByType находит первый объект указанного типа
            SimplePythonUI pythonUI = FindFirstObjectByType<SimplePythonUI>();
            // если UI панель найдена
            if (pythonUI != null)
            {
                // выводим подтверждение в консоль
                Debug.Log("ComputerInteraction: SimplePythonUI found, opening task system...");
                // открываем систему задач (панель с заданиями на Python)
                pythonUI.OpenTaskSystem();
            }
            else
            {
                // если UI панель не найдена, выводим предупреждение
                Debug.LogWarning("ComputerInteraction: SimplePythonUI not found in scene!");
            }
        }
    }
}
