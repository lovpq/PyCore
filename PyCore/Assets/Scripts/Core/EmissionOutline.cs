using UnityEngine;

namespace Core
{
    /// <summary>
    /// создает эффект подсветки объекта через emission (свечение материала)
    /// 
    /// что делает этот скрипт:
    /// - Включает emission (свечение) на материале объекта
    /// - Создает пульсирующий эффект свечения
    ///- Работает со всеми материалами объекта (поддерживает несколько)
    /// - Переключается между обычными и светящимися материалами
    /// 
    /// как использовать в unity:
    /// 1. Добавьте этот скрипт на объект с Renderer
    /// 2. Настройте цвет и интенсивность подсветки
    /// 3. Материал объекта должен поддерживать emission (URP Lit, Standard и т.д.)
    /// 4. Вызывайте SetOutlineEnabled(true/false) для включения/выключения
    /// 
    /// как работает:
    /// - Создает копии материалов с включенным emission
    /// - Update() создает пульсацию через Mathf.Sin
    /// - SetOutlineEnabled() переключает между обычными и светящимися материалами
    /// </summary>
    public class EmissionOutline : MonoBehaviour
    {
        [Header("Outline Settings")]
        // цвет подсветки (например: белый, желтый, cyan)
        [SerializeField] private Color highlightColor = Color.white;
        // интенсивность свечения (яркость)
        [SerializeField] private float highlightIntensity = 2f;
        // скорость пульсации (как быстро мигает)
        [SerializeField] private float pulseSpeed = 2f;

        // массив всех рендеров объекта и его детей
        private Renderer[] renderers;
        // двумерный массив оригинальных материалов для каждого рендера
        // [номер_рендера][номер_материала]
        private Material[][] originalMaterials;
        // двумерный массив светящихся материалов
        private Material[][] highlightMaterials;
        // флаг: включена ли сейчас подсветка
        private bool isHighlighted = false;
        // текущая интенсивность (меняется для пульсации)
        private float currentIntensity;

        /// <summary>
        /// вызывается Unity при создании объекта
        /// находит все рендеры и создает копии материалов
        /// </summary>
        private void Awake()
        {
            // получаем все Renderer компоненты (включая детей)
            // GetComponentsInChildren ищет на объекте и всех дочерних объектах
            renderers = GetComponentsInChildren<Renderer>();
            // настраиваем материалы
            SetupMaterials();
        }

        /// <summary>
        /// создает копии материалов с включенным emission
        /// нужно для переключения между обычными и светящимися материалами
        /// </summary>
        private void SetupMaterials()
        {
            // создаем массивы для хранения материалов
            // размер = количество рендеров
            originalMaterials = new Material[renderers.Length][];
            highlightMaterials = new Material[renderers.Length][];

            // проходим по каждому рендеру
            for (int i = 0; i < renderers.Length; i++)
            {
                // сохраняем оригинальные материалы рендера
                // materials возвращает массив материалов (объект может иметь несколько)
                originalMaterials[i] = renderers[i].materials;
                // создаем массив для светящихся копий
                highlightMaterials[i] = new Material[originalMaterials[i].Length];

                // проходим по каждому материалу рендера
                for (int j = 0; j < originalMaterials[i].Length; j++)
                {
                    // создаем копию оригинального материала
                    highlightMaterials[i][j] = new Material(originalMaterials[i][j]);
                    // включаем emission keyword (делает материал светящимся)
                    // Keyword - это флаг в shader, который активирует определенную функцию
                    highlightMaterials[i][j].EnableKeyword("_EMISSION");
                }
            }
        }

        /// <summary>
        /// вызывается Unity каждый кадр
        /// создает эффект пульсации подсветки
        /// </summary>
        private void Update()
        {
            // если подсветка включена
            if (isHighlighted)
            {
                // создаем пульсирующую интенсивность
                // Mathf.Sin создает волну от -1 до 1
                // * 0.5f делает пульсацию более мягкой
                currentIntensity = highlightIntensity + Mathf.Sin(Time.time * pulseSpeed) * 0.5f;
                // обновляем emission цвет всех материалов
                UpdateEmission();
            }
        }

        /// <summary>
        /// обновляет emission цвет всех светящихся материалов
        /// </summary>
        private void UpdateEmission()
        {
            // проходим по всем рендерам
            for (int i = 0; i < renderers.Length; i++)
            {
                // проходим по всем материалам каждого рендера
                for (int j = 0; j < highlightMaterials[i].Length; j++)
                {
                    // вычисляем цвет emission (цвет * интенсивность = яркость)
                    Color emissionColor = highlightColor * currentIntensity;
                    // устанавливаем emission цвет материала
                    // "_EmissionColor" - стандартное имя свойства для emission
                    highlightMaterials[i][j].SetColor("_EmissionColor", emissionColor);
                }
            }
        }

        /// <summary>
        /// включает или выключает подсветку
        /// </summary>
        /// <param name="enabled">true = включить подсветку, false = выключить</param>
        public void SetOutlineEnabled(bool enabled)
        {
            // если состояние уже такое же, ничего не делаем
            if (isHighlighted == enabled) return;

            // сохраняем новое состояние
            isHighlighted = enabled;

            // проходим по всем рендерам
            for (int i = 0; i < renderers.Length; i++)
            {
                // если нужно включить подсветку
                if (enabled)
                {
                    // заменяем материалы на светящиеся
                    renderers[i].materials = highlightMaterials[i];
                }
                else
                {
                    // возвращаем оригинальные материалы
                    renderers[i].materials = originalMaterials[i];
                }
            }
        }

        /// <summary>
        /// вызывается Unity при отключении компонента
        /// выключает подсветку
        /// </summary>
        private void OnDisable()
        {
            SetOutlineEnabled(false);
        }

        /// <summary>
        /// вызывается Unity при уничтожении объекта
        /// освобождает память от созданных материалов
        /// </summary>
        private void OnDestroy()
        {
            // если есть светящиеся материалы
            if (highlightMaterials != null)
            {
                // проходим по всем рендерам
                foreach (var materials in highlightMaterials)
                {
                    // проходим по всем материалам
                    foreach (var mat in materials)
                    {
                        // важно: уничтожаем созданные материалы для предотвращения утечки памяти
                        // new Material() создает материал в памяти, который нужно удалить
                        if (mat != null) Destroy(mat);
                    }
                }
            }
        }
    }
}
