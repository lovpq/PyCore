using UnityEngine;
using Core;

/// <summary>
/// помощник для автоматической настройки объектов компьютера
/// 
/// что делает этот скрипт:
/// - Автоматически добавляет необходимые компоненты к объектам компьютеров
/// - Добавляет компоненты Interactable и ComputerInteraction
/// - Добавляет коллайдер для взаимодействия
/// - Устанавливает правильный слой (Layer) для взаимодействия
/// 
/// как использовать в unity:
/// 1. Добавьте этот скрипт на пустой GameObject в сцене
/// 2. Назначьте объекты ноутбука и монитора в Inspector
/// 3. Скрипт автоматически настроит их при запуске игры
/// 
/// как работает:
/// - При запуске вызывает SetupComputer для каждого компьютера
/// - SetupComputer проверяет и добавляет недостающие компоненты
/// </summary>
public class ComputerSetupHelper : MonoBehaviour
{
    [Header("Computer Objects")]
    // объект ноутбука в 3D сцене
    [SerializeField] private GameObject laptop;
    // объект монитора в 3D сцене
    [SerializeField] private GameObject monitor;

    /// <summary>
    /// вызывается Unity при запуске скрипта (один раз)
    /// настраивает оба компьютера автоматически
    /// </summary>
    private void Start()
    {
        // настраиваем ноутбук
        SetupComputer(laptop, "Laptop");
        // настраиваем монитор
        SetupComputer(monitor, "Monitor");
    }

    /// <summary>
    /// настраивает один объект компьютера, добавляя все необходимые компоненты
    /// </summary>
    /// <param name="computerObject">Объект компьютера для настройки</param>
    /// <param name="name">Название компьютера (для отладочных сообщений)</param>
    private void SetupComputer(GameObject computerObject, string name)
    {
        // если объект не назначен в Inspector, выводим предупреждение и выходим
        if (computerObject == null)
        {
            Debug.LogWarning($"{name} object is not assigned!");
            return;
        }

        // проверяем, есть ли компонент Interactable (позволяет взаимодействовать с объектом)
        if (computerObject.GetComponent<Interactable>() == null)
        {
            // если нет, добавляем компонент Interactable
            Interactable interactable = computerObject.AddComponent<Interactable>();
            Debug.Log($"Added Interactable component to {name}");
        }

        // проверяем, есть ли компонент ComputerInteraction (логика взаимодействия с компьютером)
        if (computerObject.GetComponent<ComputerInteraction>() == null)
        {
            // если нет, добавляем компонент ComputerInteraction
            ComputerInteraction computerInteraction = computerObject.AddComponent<ComputerInteraction>();
            Debug.Log($"Added ComputerInteraction component to {name}");
        }

        // получаем компонент BoxCollider (нужен для физики и raycast)
        BoxCollider collider = computerObject.GetComponent<BoxCollider>();
        // если коллайдера нет
        if (collider == null)
        {
            // добавляем BoxCollider
            collider = computerObject.AddComponent<BoxCollider>();
            Debug.Log($"Added BoxCollider to {name}");
        }

        // проверяем, установлен ли правильный слой "Interactable"
        if (computerObject.layer != LayerMask.NameToLayer("Interactable"))
        {
            // получаем номер слоя "Interactable"
            int interactableLayer = LayerMask.NameToLayer("Interactable");
            // если слой существует в проекте (-1 означает, что слой не найден)
            if (interactableLayer != -1)
            {
                // устанавливаем объекту слой "Interactable"
                computerObject.layer = interactableLayer;
                Debug.Log($"Set {name} layer to Interactable");
            }
        }

        // выводим сообщение об успешной настройке
        Debug.Log($"{name} setup complete!");
    }
}
