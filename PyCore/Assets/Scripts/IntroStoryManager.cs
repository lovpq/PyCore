using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// управляет вступительной историей игры (интро)
/// 
/// что делает этот скрипт:
/// - Показывает текст истории с эффектом печатной машинки (по одной букве)
/// - Делает плавное появление и исчезновение текста (fade in/out)
/// - Автоматически переходит к следующей сцене после окончания истории
/// - Позволяет пропустить интро нажатием клавиши
/// 
/// как использовать в unity:
/// 1. Добавьте этот скрипт на GameObject с Canvas в сцене Intro
/// 2. Назначьте Text элемент для отображения истории
/// 3. Назначьте CanvasGroup для эффектов затухания
/// 4. Укажите название следующей сцены (например, "Basement")
/// 
/// как работает:
/// - При запуске начинает воспроизведение истории
/// - Использует Coroutines для постепенного показа текста
/// - Можно пропустить нажатием Space, Enter или клика мышью
/// </summary>
public class IntroStoryManager : MonoBehaviour
{
    [Header("UI References")]
    // текстовый элемент для отображения истории
    public Text storyText;
    // CanvasGroup для управления прозрачностью всего экрана (fade effects)
    public CanvasGroup canvasGroup;
    
    [Header("Settings")]
    // скорость печати текста (время между буквами в секундах)
    public float textSpeed = 0.05f;
    // задержка между строками текста в секундах
    public float delayBetweenLines = 1.5f;
    // длительность плавного появления экрана в секундах
    public float fadeInDuration = 1f;
    // длительность плавного исчезновения экрана в секундах
    public float fadeOutDuration = 1f;
    
    [Header("Scene to Load")]
    // название следующей сцены для загрузки (например, "Basement")
    public string nextSceneName = "Basement";

    // массив строк истории (каждая строка - отдельный текст)
    private string[] storyLines = new string[]
    {
        "Ты просыпаешься в холодном подвале...",
        "",
        "Вокруг темно, сыро и пусто.",
        "",
        "У тебя нет денег, нет работы.\n\nТы едва сводишь концы с концами.\n\nНо ты решаешь изменить свою жизнь.",
        "",
        "Программирование — это твой шанс.\n\nТы включаешь старый компьютер...",
        "",
        "И начинаешь свой путь к успеху."
    };

    /// <summary>
    /// вызывается Unity при запуске скрипта (один раз)
    /// устанавливает начальную прозрачность и запускает интро
    /// </summary>
    void Start()
    {
        // если CanvasGroup назначен, делаем экран полностью прозрачным (невидимым)
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
            
        // запускаем корутину (асинхронную функцию) для воспроизведения интро
        StartCoroutine(PlayIntro());
    }

    /// <summary>
    /// основная корутина для воспроизведения всей истории
    /// IEnumerator позволяет делать паузы в выполнении кода
    /// </summary>
    IEnumerator PlayIntro()
    {
        // ждем завершения плавного появления экрана
        yield return FadeIn();
        
        // проходим по каждой строке истории
        foreach (string line in storyLines)
        {
            // если строка пустая (для создания пауз в истории)
            if (string.IsNullOrEmpty(line))
            {
                // делаем короткую паузу (половину обычной задержки)
                yield return new WaitForSeconds(delayBetweenLines * 0.5f);
            }
            else
            {
                // показываем текст с эффектом печатной машинки
                yield return ShowText(line);
                // ждем перед следующей строкой
                yield return new WaitForSeconds(delayBetweenLines);
            }
        }
        
        // после показа всех строк, ждем еще 2 секунды
        yield return new WaitForSeconds(2f);
        // плавно скрываем экран
        yield return FadeOut();
        
        // загружаем следующую сцену
        LoadNextScene();
    }

    /// <summary>
    /// корутина для плавного появления экрана (fade in)
    /// постепенно увеличивает прозрачность от 0 до 1
    /// </summary>
    IEnumerator FadeIn()
    {
        // если CanvasGroup не назначен, выходим из функции
        if (canvasGroup == null) yield break;
        
        // счетчик прошедшего времени
        float elapsed = 0f;
        // пока не прошло fadeInDuration секунд
        while (elapsed < fadeInDuration)
        {
            // увеличиваем счетчик на время, прошедшее с прошлого кадра
            elapsed += Time.deltaTime;
            // устанавливаем прозрачность пропорционально прошедшему времени (от 0 до 1)
            canvasGroup.alpha = elapsed / fadeInDuration;
            // ждем следующего кадра
            yield return null;
        }
        // гарантируем, что в конце прозрачность точно равна 1 (полностью видимый)
        canvasGroup.alpha = 1f;
    }

    /// <summary>
    /// корутина для плавного исчезновения экрана (fade out)
    /// постепенно уменьшает прозрачность от 1 до 0
    /// </summary>
    IEnumerator FadeOut()
    {
        // если CanvasGroup не назначен, выходим из функции
        if (canvasGroup == null) yield break;
        
        // счетчик прошедшего времени
        float elapsed = 0f;
        // пока не прошло fadeOutDuration секунд
        while (elapsed < fadeOutDuration)
        {
            // увеличиваем счетчик
            elapsed += Time.deltaTime;
            // устанавливаем прозрачность (от 1 до 0)
            canvasGroup.alpha = 1f - (elapsed / fadeOutDuration);
            // ждем следующего кадра
            yield return null;
        }
        // гарантируем, что в конце прозрачность точно равна 0 (полностью прозрачный)
        canvasGroup.alpha = 0f;
    }

    /// <summary>
    /// корутина для показа текста с эффектом печатной машинки
    /// показывает текст по одной букве за раз
    /// </summary>
    /// <param name="text">Текст для отображения</param>
    IEnumerator ShowText(string text)
    {
        // если Text не назначен, выходим из функции
        if (storyText == null) yield break;
        
        // очищаем текущий текст
        storyText.text = "";
        
        // проходим по каждой букве в тексте
        foreach (char letter in text)
        {
            // добавляем букву к отображаемому тексту
            storyText.text += letter;
            // ждем textSpeed секунд перед следующей буквой (эффект печати)
            yield return new WaitForSeconds(textSpeed);
        }
    }

    /// <summary>
    /// загружает следующую сцену
    /// </summary>
    void LoadNextScene()
    {
        // загружаем сцену по имени (например, "Basement")
        SceneManager.LoadScene(nextSceneName);
    }

    /// <summary>
    /// вызывается Unity каждый кадр
    /// проверяет, нажал ли игрок клавишу для пропуска интро
    /// поддерживает оба Input System: Legacy и новый (com.unity.inputsystem)
    /// </summary>
    void Update()
    {
        if (ShouldSkip())
        {
            StopAllCoroutines();
            LoadNextScene();
        }
    }

    /// <summary>
    /// возвращает true если игрок нажал Space, Enter или левую кнопку мыши
    /// работает с Legacy Input Manager и с новым Input System одновременно
    /// </summary>
    private bool ShouldSkip()
    {
#if ENABLE_INPUT_SYSTEM
        var keyboard = Keyboard.current;
        var mouse = Mouse.current;

        if (keyboard != null && (keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame))
            return true;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            return true;
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0))
            return true;
#endif

        return false;
    }
}
