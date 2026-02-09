using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Events;

/// <summary>
/// управляет заставкой (Splash Screen) при запуске игры
/// 
/// что делает этот скрипт:
/// - Показывает логотип "PyCore" с анимацией zoom
/// - Создает эффект приближения логотипа
/// - Плавно исчезает после анимации
/// - Вызывает событие после завершения (для показа главного меню)
/// 
/// как использовать в unity:
/// 1. Добавьте этот скрипт на GameObject с Canvas
/// 2. Назначьте CanvasGroup и Text элемент с логотипом
/// 3. Настройте параметры анимации в Inspector
/// 4. Подпишите MainMenuManager на событие onAnimationComplete
/// 
/// как работает:
/// - Использует Coroutine для анимации
/// - Lerp создает плавное изменение масштаба
/// - CanvasGroup.alpha создает эффект затухания
/// - UnityEvent уведомляет другие скрипты о завершении
/// </summary>
public class SplashScreen : MonoBehaviour
{
    [Header("UI References")]
    // CanvasGroup для управления прозрачностью всего экрана
    public CanvasGroup splashCanvasGroup;
    // текст с логотипом (будет анимироваться)
    public Text logoText;
    
    [Header("Animation Settings")]
    // длительность эффекта zoom (приближения) в секундах
    public float zoomDuration = 2f;
    // начальный масштаб логотипа (0.3 = 30% от нормального размера)
    public float zoomStartScale = 0.3f;
    // конечный масштаб логотипа (1.5 = 150% от нормального размера)
    public float zoomEndScale = 1.5f;
    // длительность исчезновения экрана в секундах
    public float fadeOutDuration = 0.5f;
    // задержка перед началом исчезновения
    public float delayBeforeFade = 0.5f;

    [Header("Events")]
    // событие, вызываемое после завершения анимации
    // MainMenuManager подписывается на это событие
    public UnityEvent onAnimationComplete;

    // константа с текстом логотипа
    private const string LOGO_TEXT = "PyCore";

    /// <summary>
    /// вызывается Unity при запуске скрипта
    /// устанавливает текст логотипа и запускает анимацию
    /// </summary>
    void Start()
    {
        // если текст логотипа назначен
        if (logoText != null)
        {
            // устанавливаем текст "PyCore"
            logoText.text = LOGO_TEXT;
        }
        
        // запускаем корутину с анимацией заставки
        StartCoroutine(PlaySplashAnimation());
    }

    /// <summary>
    /// корутина для воспроизведения анимации заставки
    /// выполняет zoom эффект, задержку и fade out
    /// </summary>
    IEnumerator PlaySplashAnimation()
    {
        // если текст логотипа существует
        if (logoText != null)
        {
            // получаем RectTransform для изменения масштаба
            // RectTransform используется для UI элементов
            RectTransform logoRect = logoText.rectTransform;
            // счетчик прошедшего времени
            float elapsedTime = 0f;

            // пока не прошло zoomDuration секунд
            while (elapsedTime < zoomDuration)
            {
                // увеличиваем счетчик на время, прошедшее с последнего кадра
                elapsedTime += Time.deltaTime;
                // вычисляем прогресс анимации (от 0 до 1)
                float progress = elapsedTime / zoomDuration;
                // Lerp создает плавный переход между начальным и конечным масштабом
                // progress определяет, насколько близко мы к концу
                float scale = Mathf.Lerp(zoomStartScale, zoomEndScale, progress);
                // применяем масштаб к логотипу
                // Vector3.one * scale создает (scale, scale, scale)
                logoRect.localScale = Vector3.one * scale;
                // ждем следующего кадра
                yield return null;
            }
        }

        // ждем delayBeforeFade секунд перед началом исчезновения
        yield return new WaitForSeconds(delayBeforeFade);

        // если CanvasGroup назначен
        if (splashCanvasGroup != null)
        {
            // счетчик прошедшего времени для fade out
            float elapsedTime = 0f;
            // пока не прошло fadeOutDuration секунд
            while (elapsedTime < fadeOutDuration)
            {
                // увеличиваем счетчик
                elapsedTime += Time.deltaTime;
                // вычисляем прозрачность (от 1 до 0)
                // 1f - (...) инвертирует значение, чтобы оно уменьшалось
                splashCanvasGroup.alpha = 1f - (elapsedTime / fadeOutDuration);
                // ждем следующего кадра
                yield return null;
            }
            // гарантируем полную прозрачность в конце
            splashCanvasGroup.alpha = 0f;
        }

        // вызываем событие завершения анимации
        // ?. означает "вызвать, только если onAnimationComplete не null"
        // подписанные функции (например, ShowMainMenu) будут вызваны
        onAnimationComplete?.Invoke();

        // отключаем GameObject заставки (он больше не нужен)
        gameObject.SetActive(false);
    }
}
