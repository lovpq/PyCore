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
        private SimplePythonUI pythonUI;
        private Interactable interactable;

        private void Start()
        {
            pythonUI = FindFirstObjectByType<SimplePythonUI>();
            if (pythonUI == null)
                Debug.LogWarning("ComputerInteraction: SimplePythonUI not found in scene!");

            interactable = GetComponent<Interactable>();
            if (interactable != null)
                interactable.OnInteract.AddListener(OpenTaskWindow);
            else
                Debug.LogWarning($"ComputerInteraction: Interactable not found on {gameObject.name}!");
        }

        private void OnDestroy()
        {
            if (interactable != null)
                interactable.OnInteract.RemoveListener(OpenTaskWindow);
        }

        /// <summary>Открывает панель задач. Вызывается через OnInteract.</summary>
        private void OpenTaskWindow()
        {
            if (pythonUI == null)
                pythonUI = FindFirstObjectByType<SimplePythonUI>();

            if (pythonUI != null)
                pythonUI.OpenTaskSystem();
            else
                Debug.LogWarning("ComputerInteraction: SimplePythonUI not found in scene!");
        }
    }
}
