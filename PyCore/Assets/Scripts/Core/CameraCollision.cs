using UnityEngine;

namespace Core
{
    /// <summary>
    /// предотвращает проход камеры сквозь стены и объекты
    /// 
    /// что делает этот скрипт:
    /// - Проверяет, нет ли препятствий между игроком и камерой
    /// - Автоматически приближает камеру к игроку при обнаружении стены
    /// - Плавно возвращает камеру обратно, когда препятствие исчезает
    /// - Использует SphereCast для более точного обнаружения коллизий
    /// 
    /// как использовать в unity:
    /// 1. Добавьте этот скрипт на камеру (Camera)
    /// 2. Камера должна быть дочерним объектом игрока
    /// 3. Настройте слои коллизии в Inspector (какие объекты блокируют камеру)
    /// 4. Настройте минимальное и максимальное расстояние
    /// 
    /// как работает:
    /// - SphereCast отправляет сферу от игрока к камере
    /// - Если сфера задевает объект, камера приближается
    /// - Используется Lerp для плавного движения камеры
    /// </summary>
    public class CameraCollision : MonoBehaviour
    {
        [Header("Collision Settings")]
        // минимальное расстояние между игроком и камерой (чтобы камера не была слишком близко)
        [SerializeField] private float minDistance = 0.5f;
        // максимальное расстояние между игроком и камерой (обычное расстояние)
        [SerializeField] private float maxDistance = 2f;
        // скорость плавного движения камеры (чем больше, тем быстрее)
        [SerializeField] private float smoothSpeed = 10f;
        // радиус сферы для SphereCast (размер "пробника")
        [SerializeField] private float sphereCastRadius = 0.3f;
        // слои, которые могут блокировать камеру (стены, объекты и т.д.)
        [SerializeField] private LayerMask collisionLayers = -1;

        // ссылка на родительский объект (обычно это игрок)
        private Transform parentTransform;
        // оригинальная локальная позиция камеры (относительно игрока)
        private Vector3 originalLocalPosition;
        // текущее расстояние камеры от игрока
        private float currentDistance;

        /// <summary>
        /// вызывается Unity при запуске скрипта
        /// сохраняет начальную позицию камеры
        /// </summary>
        private void Start()
        {
            // получаем ссылку на родительский объект (игрока)
            parentTransform = transform.parent;
            // сохраняем оригинальную локальную позицию камеры
            // localPosition - позиция относительно родителя (не мировая)
            originalLocalPosition = transform.localPosition;
            // вычисляем начальное расстояние (magnitude - длина вектора)
            currentDistance = originalLocalPosition.magnitude;
        }

        /// <summary>
        /// вызывается Unity после Update (каждый кадр)
        /// LateUpdate используется для камеры, чтобы она двигалась после игрока
        /// </summary>
        private void LateUpdate()
        {
            // если родительский объект не существует, выходим
            if (parentTransform == null) return;

            // вычисляем желаемую позицию камеры в мировом пространстве
            // TransformDirection переводит локальное направление в мировое
            Vector3 desiredPosition = parentTransform.position + parentTransform.TransformDirection(originalLocalPosition);
            // направление от игрока к камере
            Vector3 direction = desiredPosition - parentTransform.position;
            // расстояние от игрока до желаемой позиции камеры
            float distance = direction.magnitude;

            // RaycastHit хранит информацию о столкновении
            RaycastHit hit;
            // SphereCast отправляет сферу от игрока в направлении камеры
            // если сфера задела что-то из collisionLayers
            if (Physics.SphereCast(parentTransform.position, sphereCastRadius, direction.normalized, out hit, distance, collisionLayers))
            {
                // есть препятствие! Приближаем камеру
                // Mathf.Clamp ограничивает значение между minDistance и maxDistance
                // вычитаем sphereCastRadius, чтобы камера не была внутри стены
                currentDistance = Mathf.Clamp(hit.distance - sphereCastRadius, minDistance, maxDistance);
            }
            else
            {
                // препятствий нет, используем полное расстояние
                currentDistance = distance;
            }

            // плавно переходим к новому расстоянию (Lerp = Linear Interpolation)
            // Time.deltaTime * smoothSpeed определяет скорость перехода
            currentDistance = Mathf.Lerp(transform.localPosition.magnitude, currentDistance, Time.deltaTime * smoothSpeed);

            // вычисляем новую локальную позицию камеры
            // normalized делает вектор единичной длины, затем умножаем на расстояние
            Vector3 newLocalPosition = originalLocalPosition.normalized * currentDistance;
            // применяем новую позицию
            transform.localPosition = newLocalPosition;
        }

        /// <summary>
        /// рисует визуальные подсказки в редакторе Unity
        /// показывает сферы для отладки
        /// </summary>
        private void OnDrawGizmos()
        {
            // если родительский объект не существует, выходим
            if (parentTransform == null) return;

            // устанавливаем цвет Gizmo (синий)
            Gizmos.color = Color.blue;
            // направление от игрока к камере
            Vector3 direction = transform.position - parentTransform.position;
            // рисуем сферу на минимальном расстоянии (показывает, насколько близко может быть камера)
            Gizmos.DrawWireSphere(parentTransform.position + direction.normalized * minDistance, sphereCastRadius);
            // рисуем сферу в текущей позиции камеры
            Gizmos.DrawWireSphere(transform.position, sphereCastRadius);
        }
    }
}
