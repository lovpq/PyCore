using UnityEngine;
using UnityEngine.UI;

namespace Core
{
    /// <summary>
    /// управляет UI подсказками для взаимодействия с объектами
    /// 
    /// что делает этот скрипт:
    /// - Показывает подсказку "Нажмите E" над взаимодействуемым объектом
    /// - Следит за объектом на экране (UI движется вместе с объектом)
    /// - Меняет изображение клавиши E при нажатии
    /// - Скрывает подсказку, когда игрок отходит от объекта
    /// 
    /// как использовать в unity:
    /// 1. Создайте GameObject с этим скриптом
    /// 2. Назначьте UI элементы в Inspector (подсказка, иконка клавиши)
    /// 3. Назначьте спрайты клавиши E (нормальное и нажатое состояние)
    /// 4. Скрипт автоматически работает с Interactable.cs
    /// 
    /// как работает:
    /// - Singleton pattern - только один экземпляр в игре
    /// - Interactable вызывает SetInteractionPrompt()
    /// - Update() обновляет позицию UI над объектом
    /// - WorldToScreenPoint конвертирует 3D позицию в 2D экрана
    /// </summary>
    public class InteractionUIManager : MonoBehaviour
    {
        // === SINGLETON ===
        // Static экземпляр для доступа из Interactable
        public static InteractionUIManager Instance { get; private set; }

        [Header("UI References")]
        // GameObject с подсказкой (обычно Canvas с текстом "Нажмите E")
        [SerializeField] private GameObject interactionPrompt;
        // изображение клавиши E (меняется при нажатии)
        [SerializeField] private Image keyIcon;
        // Canvas в мировом пространстве (над объектом)
        [SerializeField] private Canvas worldSpaceCanvas;

        [Header("Key Images")]
        // спрайт клавиши E в нормальном состоянии (не нажата)
        [SerializeField] private Sprite keyNormalSprite;
        // спрайт клавиши E в нажатом состоянии
        [SerializeField] private Sprite keyPressedSprite;

        // ссылка на текущий взаимодействуемый объект
        private Interactable currentInteractable;
        // ссылка на главную камеру (для конвертации координат)
        private Camera mainCamera;
        // RectTransform подсказки и корневого Canvas для правильного пересчёта координат
        private RectTransform promptRect;
        private RectTransform canvasRect;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
            {
                Destroy(gameObject);
                return;
            }

            mainCamera = Camera.main;

            if (interactionPrompt != null)
            {
                promptRect = interactionPrompt.GetComponent<RectTransform>();
                Canvas rootCanvas = interactionPrompt.GetComponentInParent<Canvas>();
                if (rootCanvas != null)
                    canvasRect = rootCanvas.GetComponent<RectTransform>();

                interactionPrompt.SetActive(false);
            }
        }

        private void Update()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;

            if (currentInteractable == null || interactionPrompt == null || mainCamera == null)
                return;

            Vector3 screenPos = mainCamera.WorldToScreenPoint(currentInteractable.transform.position);
            if (screenPos.z <= 0) return;

            if (promptRect != null && canvasRect != null)
            {
                // Смещение 100 пикселей вверх в screen space
                Vector2 offsetScreenPos = new Vector2(screenPos.x, screenPos.y + 100f);

                // Определяем камеру для overlay canvas
                Canvas rootCanvas = promptRect.GetComponentInParent<Canvas>();
                Camera uiCamera = (rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
                    ? rootCanvas.worldCamera
                    : null;

                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect, offsetScreenPos, uiCamera, out Vector2 localPoint))
                {
                    promptRect.localPosition = localPoint;
                }
            }
            else
            {
                // Фоллбэк для случаев без RectTransform
                interactionPrompt.transform.position = screenPos + new Vector3(0f, 100f, 0f);
            }
        }

        /// <summary>
        /// устанавливает текущий взаимодействуемый объект
        /// показывает или скрывает подсказку
        /// </summary>
        /// <param name="interactable">Объект для взаимодействия (или null для скрытия)</param>
        public void SetInteractionPrompt(Interactable interactable)
        {
            // сохраняем ссылку на текущий объект
            currentInteractable = interactable;

            // если UI подсказка существует
            if (interactionPrompt != null)
            {
                // включаем подсказку, если interactable не null
                // выключаем подсказку, если interactable == null
                // тернарный оператор: interactable != null ? true : false
                interactionPrompt.SetActive(interactable != null);
            }
        }

        /// <summary>
        /// меняет изображение клавиши E (нажата / не нажата)
        /// вызывается из PlayerInteraction
        /// </summary>
        /// <param name="pressed">true = клавиша нажата, false = клавиша отпущена</param>
        public void SetKeyPressed(bool pressed)
        {
            // если иконка клавиши существует
            if (keyIcon != null)
            {
                // меняем спрайт в зависимости от состояния
                // если нажата, используем keyPressedSprite, иначе keyNormalSprite
                keyIcon.sprite = pressed ? keyPressedSprite : keyNormalSprite;
            }
        }
    }
}
