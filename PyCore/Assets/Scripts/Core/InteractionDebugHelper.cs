using UnityEngine;

namespace Core
{
    /// <summary>
    /// инструмент отладки для системы взаимодействия
    /// 
    /// что делает этот скрипт:
    /// - Визуализирует зоны взаимодействия в редакторе (Gizmos)
    /// - Показывает расстояние между игроком и объектами
    /// - Предоставляет инструменты для диагностики проблем
    /// - Имеет Context Menu команды для быстрой проверки
    /// 
    /// как использовать в unity:
    /// 1. Добавьте этот скрипт на любой GameObject в сцене
    /// 2. Настройте флаги отладки в Inspector
    /// 3. Запустите игру и увидите визуальные подсказки в Scene View
    /// 4. ПКМ на скрипте -> Context Menu для диагностики
    /// 
    /// как работает:
    /// - OnDrawGizmos() рисует визуальные подсказки каждый кадр
    /// - #if UNITY_EDITOR компилируется только в редакторе
    /// - ContextMenu создает команды в правом клике на компоненте
    /// 
    /// важно:
    /// - Этот скрипт работает только в редакторе Unity (не в билде)
    /// - Используется только для отладки, можно удалить перед релизом
    /// </summary>
    public class InteractionDebugHelper : MonoBehaviour
    {
        [Header("Debug Settings")]
        // показывать ли радиус взаимодействия вокруг объектов
        [SerializeField] private bool showInteractionRadius = true;
        // показывать ли линию от игрока к объекту
        [SerializeField] private bool showPlayerToObjectLine = true;
        // показ вать ли raycast от камеры
        [SerializeField] private bool showRaycast = true;
        // показывать ли текстовые метки над объектами
        [SerializeField] private bool showLabels = true;

        [Header("Colors")]
        // цвет для объектов, к которым игрок близко (может взаимодействовать)
        [SerializeField] private Color nearbyColor = Color.green;
        // цвет для объектов, которые далеко от игрока
        [SerializeField] private Color farColor = Color.yellow;
        // цвет для raycast луча
        [SerializeField] private Color raycastColor = Color.red;
        // цвет для линии между игроком и объектом
        [SerializeField] private Color lineColor = Color.blue;

        // ссылка на Transform игрока
        private Transform playerTransform;
        // ссылка на камеру игрока
        private Camera playerCamera;

        /// <summary>
        /// вызывается Unity при запуске скрипта
        /// находит игрока и его камеру
        /// </summary>
        private void Start()
        {
            // ищем игрока по тегу
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                // сохраняем Transform игрока
                playerTransform = player.transform;
                // находим камеру в детях игрока
                playerCamera = player.GetComponentInChildren<Camera>();
            }
        }

        /// <summary>
        /// рисует отладочные визуализации в Scene View редактора
        /// вызывается Unity каждый кадр ТОЛЬКО в редакторе
        /// </summary>
        private void OnDrawGizmos()
        {
            // работает только когда игра запущена (в Play Mode)
            if (!Application.isPlaying) return;

            // находим все объекты Interactable в сцене
            Interactable[] interactables = FindObjectsOfType<Interactable>();

            // проходим по каждому взаимодействуемому объекту
            foreach (var interactable in interactables)
            {
                // проверка на null (если объект был удален)
                if (interactable == null) continue;

                // проверяем, рядом ли игрок с объектом
                bool isNearby = interactable.IsPlayerNearby;
                // выбираем цвет в зависимости от расстояния
                Color gizmoColor = isNearby ? nearbyColor : farColor;

                // если нужно показать радиус взаимодействия
                if (showInteractionRadius)
                {
                    // устанавливаем цвет Gizmo
                    Gizmos.color = gizmoColor;
                    // рисуем сферу-каркас вокруг объекта (радиус 3 метра)
                    Gizmos.DrawWireSphere(interactable.transform.position, 3f);
                }

                // если нужно показать линию от игрока к объекту
                if (showPlayerToObjectLine && playerTransform != null)
                {
                    // устанавливаем цвет линии
                    Gizmos.color = lineColor;
                    // рисуем линию от игрока к объекту
                    Gizmos.DrawLine(playerTransform.position, interactable.transform.position);

                    // вычисляем расстояние
                    float distance = Vector3.Distance(playerTransform.position, interactable.transform.position);
                    
                    // #if UNITY_EDITOR - код внутри компилируется ТОЛЬКО в редакторе
                    // в билде игры этого кода не будет
                    #if UNITY_EDITOR
                    // если нужно показывать текстовые метки
                    if (showLabels)
                    {
                        // вычисляем точку посередине между игроком и объектом
                        Vector3 midPoint = (playerTransform.position + interactable.transform.position) / 2f;
                        // рисуем текстовую метку с расстоянием
                        // F2 форматирует число с 2 знаками после запятой
                        UnityEditor.Handles.Label(midPoint, $"{distance:F2}m");
                    }
                    #endif
                }

                // показываем метку над объектом
                #if UNITY_EDITOR
                if (showLabels)
                {
                    // позиция метки чуть выше объекта
                    Vector3 labelPos = interactable.transform.position + Vector3.up * 0.5f;
                    // текст статуса (рядом или далеко)
                    string status = isNearby ? "NEARBY" : "FAR";
                    // рисуем метку с именем объекта и статусом
                    UnityEditor.Handles.Label(labelPos, $"{interactable.gameObject.name}\n{status}");
                }
                #endif
            }

           // показываем raycast от камеры
            if (showRaycast && playerCamera != null)
            {
                // устанавливаем цвет raycast
                Gizmos.color = raycastColor;
                // создаем луч от камеры вперед
                Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
                // рисуем луч длиной 3 метра
                Gizmos.DrawRay(ray.origin, ray.direction * 3f);
            }
        }

        /// <summary>
        /// Context Menu - добавляет команду в меню ПКМ на компоненте
        /// находит и выводит информацию о всех интерактивных объектах
        /// </summary>
        [ContextMenu("Найти все интерактивные объекты")]
        private void FindAllInteractables()
        {
            // находим все Interactable в сцене
            Interactable[] interactables = FindObjectsOfType<Interactable>();
            // выводим заголовок
            Debug.Log($"=== НАЙДЕНО {interactables.Length} ИНТЕРАКТИВНЫХ ОБЪЕКТОВ ===\n");

            // проходим по каждому объекту
            for (int i = 0; i < interactables.Length; i++)
            {
                // выводим информацию об объекте
                Debug.Log($"{i + 1}. {interactables[i].gameObject.name}");
                Debug.Log($"   - Position: {interactables[i].transform.position}");
                // проверяем наличие компонентов
                Debug.Log($"   - Has Outline: {interactables[i].GetComponent<SimpleOutline>() != null}");
                Debug.Log($"   - Has Collider: {interactables[i].GetComponent<Collider>() != null}\n");
            }
        }

        /// <summary>
        /// Context Menu - проверяет правильность настройки игрока
        /// помогает найти проблемы, если взаимодействие не работает
        /// </summary>
        [ContextMenu("Проверить настройку персонажа")]
        private void CheckPlayerSetup()
        {
            // ищем игрока по тегу
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            
            Debug.Log("=== ПРОВЕРКА ПЕРСОНАЖА ===\n");

            // если игрок не найден
            if (player == null)
            {
                Debug.LogError("✗ Персонаж с тегом 'Player' не найден!");
                return;
            }

            Debug.Log($"✓ Персонаж найден: {player.name}\n");

            // проверяем наличие необходимых компонентов
            PlayerInteraction interaction = player.GetComponent<PlayerInteraction>();
            Debug.Log($"PlayerInteraction: {(interaction != null ? "✓" : "✗")}");

            Camera cam = player.GetComponentInChildren<Camera>();
            Debug.Log($"Camera: {(cam != null ? "✓" : "✗")}");

            var playerInput = player.GetComponent<UnityEngine.InputSystem.PlayerInput>();
            Debug.Log($"PlayerInput: {(playerInput != null ? "✓" : "✗")}");
        }

        /// <summary>
        /// Context Menu - проверяет наличие UI менеджеров в сцене
        /// </summary>
        [ContextMenu("Проверить UI менеджеры")]
        private void CheckUIManagers()
        {
            Debug.Log("=== ПРОВЕРКА UI МЕНЕДЖЕРОВ ===\n");

            // проверяем наличие менеджеров
            InteractionUIManager interactionUI = FindObjectOfType<InteractionUIManager>();
            Debug.Log($"InteractionUIManager: {(interactionUI != null ? "✓" : "✗")}");

            TaskUIManager taskUI = FindObjectOfType<TaskUIManager>();
            Debug.Log($"TaskUIManager: {(taskUI != null ? "✓" : "✗")}");

            TaskManager taskManager = FindObjectOfType<TaskManager>();
            Debug.Log($"TaskManager: {(taskManager != null ? "✓" : "✗")}");

            // если TaskManager найден, показываем количество задач
            if (taskManager != null)
            {
                Debug.Log($"Задач загружено: {taskManager.GetTasks().Count}");
            }
        }
    }
}
