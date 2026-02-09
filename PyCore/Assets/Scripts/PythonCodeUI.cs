using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// простой UI для написания и выполнения Python кода
/// 
/// что делает этот скрипт:
/// - Предоставляет интерфейс для написания Python кода
/// - Выполняет код и показывает результат
/// - Отображает ошибки красным цветом
/// - Поддерживает очистку вывода
/// 
/// как использовать в unity:
/// 1. Добавьте этот скрипт на GameObject с Canvas
/// 2. Назначьте все UI элементы в Inspector
/// 3. Вызовите OpenPythonPanel() для открытия панели
/// 
/// как работает:
/// - Использует PythonExecutor для запуска Python кода
/// - Подписывается на события вывода и ошибок
/// - Управляет курсором и видимостью панели
/// </summary>
public class PythonCodeUI : MonoBehaviour
{
    [Header("UI References")]
    // главная панель с интерфейсом Python
    [SerializeField] private GameObject pythonPanel;
    // поле ввода для написания кода
    [SerializeField] private TMP_InputField codeInputField;
    // текст для отображения вывода программы
    [SerializeField] private TMP_Text outputText;
    // кнопка "Выполнить код"
    [SerializeField] private Button executeButton;
    // кнопка "Очистить вывод"
    [SerializeField] private Button clearButton;
    // кнопка "Закрыть панель"
    [SerializeField] private Button closeButton;
    // ScrollRect для прокрутки вывода (если текста много)
    [SerializeField] private ScrollRect outputScrollRect;

    [Header("Settings")]
    // цвет для обычного вывода (белый)
    [SerializeField] private Color normalOutputColor = Color.white;
    // цвет для ошибок (красный)
    [SerializeField] private Color errorOutputColor = Color.red;
    // цвет для успешного выполнения (зеленый)
    [SerializeField] private Color successOutputColor = Color.green;

    // компонент для выполнения Python кода (приватный)
    private PythonExecutor pythonExecutor;

    /// <summary>
    /// вызывается Unity при создании объекта (перед Start)
    /// настраивает все компоненты и подписывается на события
    /// </summary>
    private void Awake()
    {
        // добавляем компонент PythonExecutor к этому объекту
        pythonExecutor = gameObject.AddComponent<PythonExecutor>();

        // подписываемся на события PythonExecutor
        // += означает "добавить обработчик события"
        // когда Python выдаст результат, вызовется OnPythonOutput
        pythonExecutor.OnOutputReceived += OnPythonOutput;
        // когда Python выдаст ошибку, вызовется OnPythonError
        pythonExecutor.OnErrorReceived += OnPythonError;
        // когда Python завершит выполнение, вызовется OnPythonCompleted
        pythonExecutor.OnExecutionCompleted += OnPythonCompleted;

        // настраиваем кнопку выполнения
        if (executeButton != null)
        {
            // когда кнопку нажмут, вызовется функция ExecuteCode
            executeButton.onClick.AddListener(ExecuteCode);
        }

        // настраиваем кнопку очистки
        if (clearButton != null)
        {
            // когда кнопку нажмут, вызовется функция ClearOutput
            clearButton.onClick.AddListener(ClearOutput);
        }

        // настраиваем кнопку закрытия
        if (closeButton != null)
        {
            // когда кнопку нажмут, вызовется функция ClosePythonPanel
            closeButton.onClick.AddListener(ClosePythonPanel);
        }

        // скрываем панель при запуске игры
        if (pythonPanel != null)
        {
            pythonPanel.SetActive(false);
        }
    }

    /// <summary>
    /// вызывается Unity при уничтожении объекта
    /// отписывается от всех событий и удаляет обработчики кнопок
    /// </summary>
    private void OnDestroy()
    {
        // отписываемся от событий PythonExecutor
        if (pythonExecutor != null)
        {
            // -= означает "удалить обработчик события"
            pythonExecutor.OnOutputReceived -= OnPythonOutput;
            pythonExecutor.OnErrorReceived -= OnPythonError;
            pythonExecutor.OnExecutionCompleted -= OnPythonCompleted;
        }

        // удаляем обработчики кнопок
        if (executeButton != null)
        {
            executeButton.onClick.RemoveListener(ExecuteCode);
        }

        if (clearButton != null)
        {
            clearButton.onClick.RemoveListener(ClearOutput);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(ClosePythonPanel);
        }
    }

    /// <summary>
    /// открывает панель Python
    /// показывает курсор и разблокирует его
    /// </summary>
    public void OpenPythonPanel()
    {
        if (pythonPanel != null)
        {
            // делаем панель видимой
            pythonPanel.SetActive(true);
            // разблокируем курсор (чтобы можно было кликать по UI)
            Cursor.lockState = CursorLockMode.None;
            // делаем курсор видимым
            Cursor.visible = true;
        }
    }

    /// <summary>
    /// закрывает панель Python
    /// скрывает курсор и блокирует его в центре экрана
    /// </summary>
    public void ClosePythonPanel()
    {
        if (pythonPanel != null)
        {
            // скрываем панель
            pythonPanel.SetActive(false);
            // блокируем курсор в центре экрана (для FPS игры)
            Cursor.lockState = CursorLockMode.Locked;
            // делаем курсор невидимым
            Cursor.visible = false;
        }
    }

    /// <summary>
    /// выполняет код, написанный в поле ввода
    /// проверяет, что код не пустой, и запускает Python
    /// </summary>
    private void ExecuteCode()
    {
        // проверяем, что поле ввода существует и код не пустой
        if (codeInputField == null || string.IsNullOrWhiteSpace(codeInputField.text))
        {
            // если код пустой, выводим сообщение об ошибке
            AppendOutput("Введите код для выполнения!", errorOutputColor);
            return; // выходим из функции
        }

        // выводим сообщение о начале выполнения
        AppendOutput("Выполнение кода...", normalOutputColor);
        // выводим разделительную линию для красоты
        AppendOutput("----------------------------------------", normalOutputColor);

        // запускаем Python код через PythonExecutor
        pythonExecutor.ExecutePythonCode(codeInputField.text);
    }

    /// <summary>
    /// очищает весь текст вывода
    /// </summary>
    private void ClearOutput()
    {
        if (outputText != null)
        {
            // устанавливаем пустую строку в текст вывода
            outputText.text = "";
        }
    }

    /// <summary>
    /// вызывается, когда Python выдает обычный вывод (print)
    /// </summary>
    /// <param name="output">Текст, который вывел Python</param>
    private void OnPythonOutput(string output)
    {
        // добавляем вывод к тексту обычным цветом
        AppendOutput(output, normalOutputColor);
    }

    /// <summary>
    /// вызывается, когда Python выдает ошибку
    /// </summary>
    /// <param name="error">Текст ошибки</param>
    private void OnPythonError(string error)
    {
        // добавляем ошибку к тексту красным цветом
        AppendOutput($"ОШИБКА:\n{error}", errorOutputColor);
    }

    /// <summary>
    /// вызывается, когда Python завершает выполнение
    /// </summary>
    /// <param name="exitCode">Код завершения (0 = успех, другое = ошибка)</param>
    private void OnPythonCompleted(int exitCode)
    {
        // если exitCode == 0, значит код выполнился без ошибок
        if (exitCode == 0)
        {
            // выводим разделительную линию
            AppendOutput("----------------------------------------", normalOutputColor);
            // выводим сообщение об успешном выполнении зеленым цветом
            AppendOutput("Выполнено успешно!", successOutputColor);
        }
        else
        {
            // если exitCode != 0, значит произошла ошибка
            AppendOutput("----------------------------------------", normalOutputColor);
            // выводим сообщение об ошибке с кодом
            AppendOutput($"Завершено с кодом: {exitCode}", errorOutputColor);
        }

        // добавляем пустую строку для отступа
        AppendOutput("", normalOutputColor);
        // прокручиваем вывод вниз, чтобы видеть последнее сообщение
        ScrollToBottom();
    }

    /// <summary>
    /// добавляет текст к выводу с указанным цветом
    /// </summary>
    /// <param name="text">Текст для добавления</param>
    /// <param name="color">Цвет текста</param>
    private void AppendOutput(string text, Color color)
    {
        if (outputText != null)
        {
            // конвертируем цвет Unity в HTML hex формат (например, #FFFFFF для белого)
            string colorHex = ColorUtility.ToHtmlStringRGBA(color);
            // добавляем текст с HTML тегом color для окрашивания
            // <color=#FFFFFF>текст</color>
            outputText.text += $"<color=#{colorHex}>{text}</color>\n";
        }
    }

    /// <summary>
    /// прокручивает ScrollRect вниз, чтобы показать последний текст
    /// </summary>
    private void ScrollToBottom()
    {
        if (outputScrollRect != null)
        {
            // принудительно обновляем Canvas (чтобы правильно рассчитать размеры)
            Canvas.ForceUpdateCanvases();
            // устанавливаем позицию прокрутки в самый низ (0 = низ, 1 = верх)
            outputScrollRect.verticalNormalizedPosition = 0f;
        }
    }
}
