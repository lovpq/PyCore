using UnityEngine;

namespace Core
{
    /// <summary>
    /// создает светящуюся обводку вокруг объекта
    /// 
    /// что делает этот скрипт:
    /// - Создает копию объекта немного большего размера
    /// - Применяет светящийся материал к копии
    /// - Создает пульсирующий эффект (brightness меняется)
    /// - Включает/выключает обводку по команде
    /// 
    /// как использовать в unity:
    /// 1. Добавьте этот скрипт на объект, который нужно обвести
    /// 2. Настройте цвет, интенсивность и скорость пульсации
    /// 3. Вызывайте SetOutlineEnabled(true/false) для включения обводки
    /// 4. Обычно используется вместе с Interactable.cs
    /// 
    /// как работает:
    /// - CreateOutline() дублирует mesh объекта и делает его чуть больше
    /// - Update() создает пульсацию через Mathf.Sin
    /// - Используется URP Unlit shader для свечения
    /// </summary>
    public class SimpleOutline : MonoBehaviour
    {
        [Header("Outline Settings")]
        // цвет обводки (например: белый, желтый, голубой)
        [SerializeField] private Color outlineColor = Color.white;
        // интенсивность свечения (яркость) обводки
        [SerializeField] private float outlineIntensity = 2f;
        // скорость пульсации обводки (как быстро она мерцает)
        [SerializeField] private float pulseSpeed = 2f;

        // объект, содержащий обводку (создается динамически)
        private GameObject outlineObject;
        // массив рендереров оригинального объекта (для копирования mesh)
        private Renderer[] originalRenderers;
        // материал для обводки (светящийся)
        private Material outlineMaterial;
        // текущая интенсивность (меняется для создания пульсации)
        private float currentIntensity;

        /// <summary>
        /// вызывается Unity при создании объекта
        /// находит все рендеры и создает материал обводки
        /// </summary>
        private void Awake()
        {
            // GetComponentsInChildren находит все рендеры на этом объекте и его детях
            originalRenderers = GetComponentsInChildren<Renderer>();
            // создаем новый материал с URP Unlit shader (простой светящийся shader)
            // Shader.Find ищет shader по имени
            outlineMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        }

        /// <summary>
        /// вызывается Unity каждый кадр
        /// создает пульсирующий эффект обводки
        /// </summary>
        private void Update()
        {
            // если обводка существует
            if (outlineObject != null)
            {
                // создаем пульсацию с помощью синусоиды
                // Mathf.Sin возвращает значение от -1 до 1
                // Time.time * pulseSpeed определяет скорость пульсации
                // * 0.5f делает пульсацию более мягкой
                currentIntensity = outlineIntensity + Mathf.Sin(Time.time * pulseSpeed) * 0.5f;
                
                // вычисляем цвет свечения (цвет * интенсивность)
                // умножение цвета на число делает его ярче
                Color emissionColor = outlineColor * currentIntensity;
                
                // устанавливаем цвет материала обводки
                // "_BaseColor" - стандартное имя свойства для URP Unlit shader
                outlineMaterial.SetColor("_BaseColor", emissionColor);
            }
        }

        /// <summary>
        /// включает или выключает обводку
        /// </summary>
        /// <param name="enabled">true = показать обводку, false = скрыть обводку</param>
        public void SetOutlineEnabled(bool enabled)
        {
            // если нужно включить обводку И её еще нет
            if (enabled && outlineObject == null)
            {
                // создаем обводку
                CreateOutline();
            }
            // если нужно выключить обводку И она существует
            else if (!enabled && outlineObject != null)
            {
                // уничтожаем объект обводки
                Destroy(outlineObject);
                // обнуляем ссылку
                outlineObject = null;
            }
        }

        /// <summary>
        /// создает обводку вокруг объекта
        /// копирует mesh объекта и делает его немного больше
        /// </summary>
        private void CreateOutline()
        {
            // создаем новый пустой GameObject для обводки
            outlineObject = new GameObject("Outline");
            // делаем его дочерним объектом текущего объекта
            outlineObject.transform.SetParent(transform);
            // устанавливаем локальную позицию в (0,0,0) относительно родителя
            outlineObject.transform.localPosition = Vector3.zero;
            // устанавливаем локальный поворот в нейтральное положение
            outlineObject.transform.localRotation = Quaternion.identity;
            // увеличиваем масштаб на 2% (1.02), чтобы обводка была немного больше оригинала
            outlineObject.transform.localScale = Vector3.one * 1.02f;

            // проходим по всем рендерам оригинального объекта
            foreach (var originalRenderer in originalRenderers)
            {
                // пытаемся получить MeshFilter (компонент, содержащий mesh)
                MeshFilter originalMeshFilter = originalRenderer.GetComponent<MeshFilter>();
                // если MeshFilter нет, пропускаем этот рендер
                if (originalMeshFilter == null) continue;

                // создаем новый GameObject для части обводки
                GameObject outlinePart = new GameObject(originalRenderer.name + "_Outline");
                // делаем его дочерним объектом outlineObject
                outlinePart.transform.SetParent(outlineObject.transform);
                // копируем позицию оригинального рендера
                outlinePart.transform.position = originalRenderer.transform.position;
                // копируем поворот оригинального рендера
                outlinePart.transform.rotation = originalRenderer.transform.rotation;
                // копируем масштаб оригинального рендера
                outlinePart.transform.localScale = originalRenderer.transform.localScale;

                // добавляем MeshFilter к части обводки
                MeshFilter meshFilter = outlinePart.AddComponent<MeshFilter>();
                // копируем mesh из оригинала
                // sharedMesh используется для экономии памяти (не копирует mesh, а использует тот же)
                meshFilter.mesh = originalMeshFilter.sharedMesh;

                // добавляем MeshRenderer для отображения mesh
                MeshRenderer meshRenderer = outlinePart.AddComponent<MeshRenderer>();
                // применяем светящийся материал обводки
                meshRenderer.material = outlineMaterial;
            }
        }

        /// <summary>
        /// вызывается Unity при отключении компонента
        /// отключает обводку
        /// </summary>
        private void OnDisable()
        {
            // выключаем обводку при отключении компонента
            SetOutlineEnabled(false);
        }

        /// <summary>
        /// вызывается Unity при уничтожении объекта
        /// очищает ресурсы (материал)
        /// </summary>
        private void OnDestroy()
        {
            // если материал обводки существует
            if (outlineMaterial != null)
            {
                // важно: уничтожаем материал, чтобы не было утечки памяти
                // материалы, созданные через new Material(), нужно удалять вручную
                Destroy(outlineMaterial);
            }
        }
    }
}
