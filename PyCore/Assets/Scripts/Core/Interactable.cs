using UnityEngine;
using UnityEngine.Events;

namespace Core
{
    /// <summary>
    /// базовый компонент для всех взаимодействуемых объектов в игре
    /// 
    /// что делает этот скрипт:
    /// - Определяет, находится ли игрок рядом с объектом
    /// - Включает/выключает обводку объекта (outline)
    /// - Показывает UI подсказку "Нажмите E"
    /// - Вызывает события при взаимодействии
    /// 
    /// как использовать в unity:
    /// 1. Добавьте этот скрипт на объект, с которым можно взаимодействовать
    /// 2. Настройте расстояние взаимодействия в Inspector
    /// 3. Добавьте действия в событие OnInteract (что произойдет при нажатии E)
    /// 4. Опционально: добавьте SimpleOutline для визуального эффекта
    /// 
    /// как работает:
    /// - Каждый кадр проверяет расстояние до игрока
    /// - Если игрок близко, включает обводку и показывает подсказку
    /// - При вызове Interact() выполняет назначенные действия
    /// </summary>
    public class Interactable : MonoBehaviour
    {
        [Header("Interaction Settings")]
        // максимальное расстояние, на котором можно взаимодействовать с объектом
        [SerializeField] private float interactionDistance = 3f;
        // UnityEvent позволяет назначить действия через Inspector
        // например: открыть дверь, включить свет, открыть меню
        [SerializeField] private UnityEvent onInteract;

        // флаг: находится ли игрок рядом (в зоне взаимодействия)
        private bool isPlayerNearby = false;
        // ссылка на Transform игрока
        private Transform playerTransform;
        // компонент для визуальной обводки объекта
        private SimpleOutline outlineEffect;

        // публичное свойство для чтения флага isPlayerNearby из других скриптов
        public bool IsPlayerNearby => isPlayerNearby;
        // публичное свойство для доступа к событию onInteract
        public UnityEvent OnInteract => onInteract;

        /// <summary>
        /// вызывается Unity при запуске скрипта
        /// находит компонент обводки
        /// </summary>
        private void Start()
        {
            // пытаемся найти компонент SimpleOutline на этом объекте
            outlineEffect = GetComponent<SimpleOutline>();
            // если компонент не найден
            if (outlineEffect == null)
            {
                // выводим предупреждение (это не критично, обводка опциональна)
                Debug.LogWarning($"SimpleOutline component not found on {gameObject.name}. Add SimpleOutline for visual feedback.");
            }
        }

        /// <summary>
        /// вызывается Unity каждый кадр
        /// проверяет расстояние до игрока и управляет визуальными эффектами
        /// </summary>
        private const float FIND_PLAYER_INTERVAL = 0.5f;
        private float nextFindPlayerTime = 0f;

        private void Update()
        {
            if (playerTransform == null)
            {
                if (Time.time < nextFindPlayerTime) return;
                nextFindPlayerTime = Time.time + FIND_PLAYER_INTERVAL;

                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                    playerTransform = player.transform;
                return;
            }

            float distance = Vector3.Distance(transform.position, playerTransform.position);
            bool wasNearby = isPlayerNearby;
            isPlayerNearby = distance <= interactionDistance;

            if (isPlayerNearby != wasNearby && outlineEffect != null)
                outlineEffect.SetOutlineEnabled(isPlayerNearby);
        }

        /// <summary>
        /// вызывается при взаимодействии (нажатие клавиши E игроком)
        /// выполняет все действия, назначенные в onInteract
        /// </summary>
        public void Interact()
        {
            // проверяем, что игрок действительно рядом (дополнительная защита)
            if (isPlayerNearby)
            {
                // вызываем событие onInteract
                // ?. означает "вызвать, только если onInteract не null"
                // Invoke() вызывает все функции, подписанные на это событие
                onInteract?.Invoke();
            }
        }

        /// <summary>
        /// рисует визуальную подсказку в редакторе Unity при выборе объекта
        /// показывает сферу радиуса взаимодействия
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            // устанавливаем цвет Gizmo (желтый)
            Gizmos.color = Color.yellow;
            // рисуем сферу-каркас вокруг объекта
            // показывает зону, в которой игрок может взаимодействовать с объектом
            Gizmos.DrawWireSphere(transform.position, interactionDistance);
        }
    }
}
