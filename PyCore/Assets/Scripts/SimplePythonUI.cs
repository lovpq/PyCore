using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using EasyPeasyFirstPersonController;

/// <summary>
/// UI система для решения задач на Python.
/// Показывает задачи с разбивкой по локациям (5 задач на локацию).
/// Можно просматривать задачи прошлых локаций.
/// Проверяет энергию перед запуском кода.
///
/// В Unity:
/// 1. Повесить на Canvas GameObject
/// 2. Назначить все UI элементы в Inspector
/// 3. locationTabsContainer — Horizontal Layout Group для кнопок-вкладок
/// 4. locationTabPrefab — префаб кнопки вкладки (с TMP_Text)
/// </summary>
public class SimplePythonUI : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject taskListPanel;
    [SerializeField] private GameObject taskSolverPanel;

    [Header("Location Tabs")]
    [SerializeField] private Transform locationTabsContainer;  // контейнер для вкладок
    [SerializeField] private GameObject locationTabPrefab;      // префаб кнопки-вкладки
    [SerializeField] private TMP_Text locationInfoText;         // "Локация: Подвал (3/5)"

    [Header("Task List")]
    [SerializeField] private Transform tasksContainer;
    [SerializeField] private GameObject taskItemPrefab;
    [SerializeField] private Button closeListButton;

    [Header("Task Solver")]
    [SerializeField] private TMP_Text taskTitleText;
    [SerializeField] private TMP_Text taskDescriptionText;
    [SerializeField] private TMP_Text taskExampleText;
    [SerializeField] private TMP_Text expectedOutputText;
    [SerializeField] private TMP_InputField codeInputField;
    [SerializeField] private TMP_Text outputText;
    [SerializeField] private Button runCodeButton;
    [SerializeField] private Button backToListButton;
    [SerializeField] private Button clearOutputButton;
    [SerializeField] private ScrollRect outputScrollRect;

    [Header("Colors")]
    [SerializeField] private Color successColor = Color.green;
    [SerializeField] private Color errorColor = Color.red;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color activeTabColor = new Color(0.3f, 0.6f, 1f);
    [SerializeField] private Color inactiveTabColor = new Color(0.3f, 0.3f, 0.3f);

    private PythonExecutor pythonExecutor;
    private int currentTaskIndex = -1;
    private int viewingLocationIndex = 0; // какую локацию смотрим сейчас
    private string lastOutput = "";
    private FirstPersonController playerController;
    private UnityEngine.InputSystem.PlayerInput playerInput;
    private const string INDENT = "    ";

    private void Awake()
    {
        pythonExecutor = gameObject.AddComponent<PythonExecutor>();
        pythonExecutor.OnOutputReceived += OnPythonOutput;
        pythonExecutor.OnErrorReceived += OnPythonError;
        pythonExecutor.OnExecutionCompleted += OnPythonCompleted;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerController = player.GetComponent<FirstPersonController>();
            playerInput = player.GetComponent<UnityEngine.InputSystem.PlayerInput>();
        }

        if (closeListButton != null) closeListButton.onClick.AddListener(CloseTaskSystem);
        if (runCodeButton != null) runCodeButton.onClick.AddListener(RunCode);
        if (backToListButton != null) backToListButton.onClick.AddListener(BackToTaskList);
        if (clearOutputButton != null) clearOutputButton.onClick.AddListener(ClearOutput);

        if (taskListPanel != null) taskListPanel.SetActive(false);
        if (taskSolverPanel != null) taskSolverPanel.SetActive(false);

        if (codeInputField != null)
        {
            codeInputField.lineType = TMP_InputField.LineType.MultiLineNewline;
            var nav = codeInputField.navigation;
            nav.mode = Navigation.Mode.None;
            codeInputField.navigation = nav;
        }
    }

    private void Update()
    {
        var keyboard = UnityEngine.InputSystem.Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.escapeKey.wasPressedThisFrame)
        {
            if (taskSolverPanel != null && taskSolverPanel.activeSelf)
                BackToTaskList();
            else if (taskListPanel != null && taskListPanel.activeSelf)
                CloseTaskSystem();
        }

        if (codeInputField != null && codeInputField.isFocused)
            HandleCodeEditorInput(keyboard);
    }

    private void OnDestroy()
    {
        if (pythonExecutor != null)
        {
            pythonExecutor.OnOutputReceived -= OnPythonOutput;
            pythonExecutor.OnErrorReceived -= OnPythonError;
            pythonExecutor.OnExecutionCompleted -= OnPythonCompleted;
        }
    }

    public bool IsPanelOpen
    {
        get
        {
            return (taskListPanel != null && taskListPanel.activeSelf) ||
                   (taskSolverPanel != null && taskSolverPanel.activeSelf);
        }
    }

    // === ОТКРЫТИЕ / ЗАКРЫТИЕ ===

    public void OpenTaskSystem()
    {
        Debug.Log("SimplePythonUI: OpenTaskSystem called!");
        if (taskListPanel == null) return;

        taskListPanel.SetActive(true);
        if (taskSolverPanel != null) taskSolverPanel.SetActive(false);

        // Показываем текущую локацию
        viewingLocationIndex = Core.LocationManager.Instance != null
            ? Core.LocationManager.Instance.GetCurrentLocationIndex() : 0;

        RefreshLocationTabs();
        RefreshTaskList();
        EnableUIMode();
        Debug.Log("SimplePythonUI: Task system opened, UI mode enabled");
    }

    private void CloseTaskSystem()
    {
        Debug.Log("SimplePythonUI: CloseTaskSystem called!");
        SaveCurrentCode();
        currentTaskIndex = -1;

        if (taskListPanel != null) taskListPanel.SetActive(false);
        if (taskSolverPanel != null) taskSolverPanel.SetActive(false);

        DisableUIMode();
        Debug.Log("SimplePythonUI: Task system closed, UI mode disabled");
    }

    private void EnableUIMode()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (playerController != null)
        {
            playerController.SetLookControl(false);
            playerController.SetMoveControl(false);
        }
    }

    private void DisableUIMode()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (playerController != null)
        {
            playerController.SetLookControl(true);
            playerController.SetMoveControl(true);
        }
    }

    // === ВКЛАДКИ ЛОКАЦИЙ ===

    private void RefreshLocationTabs()
    {
        if (locationTabsContainer == null || Core.TaskManager.Instance == null) return;

        // Удаляем старые вкладки
        foreach (Transform child in locationTabsContainer)
            Destroy(child.gameObject);

        int currentLoc = Core.LocationManager.Instance != null
            ? Core.LocationManager.Instance.GetCurrentLocationIndex() : 0;

        for (int loc = 0; loc <= currentLoc; loc++)
        {
            int locIndex = loc; // захват для лямбды

            // Если нет префаба, создаём простую кнопку
            GameObject tabObj;
            if (locationTabPrefab != null)
            {
                tabObj = Instantiate(locationTabPrefab, locationTabsContainer);
            }
            else
            {
                tabObj = new GameObject($"Tab_{loc}");
                tabObj.transform.SetParent(locationTabsContainer, false);
            }

            string locName = Core.TaskManager.Instance.GetLocationName(loc);
            int completed = Core.TaskManager.Instance.GetCompletedTasksForLocation(loc);
            string tabText = $"{locName} ({completed}/{Core.TaskManager.TASKS_PER_LOCATION})";

            TMP_Text text = tabObj.GetComponentInChildren<TMP_Text>();
            if (text != null) text.text = tabText;

            Button btn = tabObj.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => OnLocationTabClicked(locIndex));

                // Подсветка активной вкладки
                var colors = btn.colors;
                colors.normalColor = (loc == viewingLocationIndex) ? activeTabColor : inactiveTabColor;
                btn.colors = colors;
            }
        }
    }

    private void OnLocationTabClicked(int locationIndex)
    {
        viewingLocationIndex = locationIndex;
        RefreshLocationTabs();
        RefreshTaskList();
    }

    // === СПИСОК ЗАДАЧ ===

    private void RefreshTaskList()
    {
        if (tasksContainer == null || Core.TaskManager.Instance == null) return;

        foreach (Transform child in tasksContainer)
            Destroy(child.gameObject);

        var tasks = Core.TaskManager.Instance.GetTasksForLocation(viewingLocationIndex);
        int baseIndex = viewingLocationIndex * Core.TaskManager.TASKS_PER_LOCATION;

        // Инфо о локации
        if (locationInfoText != null)
        {
            string locName = Core.TaskManager.Instance.GetLocationName(viewingLocationIndex);
            int completed = Core.TaskManager.Instance.GetCompletedTasksForLocation(viewingLocationIndex);
            locationInfoText.text = $"{locName} — {completed}/{Core.TaskManager.TASKS_PER_LOCATION} задач";
        }

        for (int i = 0; i < tasks.Count; i++)
            CreateTaskItem(tasks[i], baseIndex + i);
    }

    private void CreateTaskItem(Core.PythonTask task, int globalIndex)
    {
        if (taskItemPrefab == null) return;

        GameObject taskItem = Instantiate(taskItemPrefab, tasksContainer);

        TMP_Text titleText = taskItem.transform.Find("TaskTitle")?.GetComponent<TMP_Text>();
        TMP_Text statusText = taskItem.transform.Find("Status")?.GetComponent<TMP_Text>();
        Button button = taskItem.GetComponent<Button>();

        if (titleText != null) titleText.text = task.taskTitle;

        if (statusText != null)
        {
            statusText.text = task.isCompleted ? "✓ Выполнено" : "○ Не выполнено";
            statusText.color = task.isCompleted ? successColor : Color.yellow;
        }

        int idx = globalIndex;
        if (button != null)
            button.onClick.AddListener(() => OpenTask(idx));
    }

    // === РЕДАКТОР ЗАДАЧИ ===

    private void OpenTask(int taskIndex)
    {
        if (Core.TaskManager.Instance == null) return;

        var tasks = Core.TaskManager.Instance.GetTasks();
        if (taskIndex < 0 || taskIndex >= tasks.Count) return;

        currentTaskIndex = taskIndex;
        Core.PythonTask task = tasks[taskIndex];

        if (taskListPanel != null) taskListPanel.SetActive(false);
        if (taskSolverPanel != null) taskSolverPanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (taskTitleText != null) taskTitleText.text = task.taskTitle;
        if (taskDescriptionText != null) taskDescriptionText.text = task.taskDescription;
        if (taskExampleText != null) taskExampleText.text = $"Пример:\n{task.taskExample}";
        if (expectedOutputText != null) expectedOutputText.text = $"Ожидаемый вывод:\n{task.expectedOutput}";

        if (codeInputField != null)
        {
            codeInputField.text = task.savedCode;
            codeInputField.ActivateInputField();
        }

        ClearOutput();
    }

    private void BackToTaskList()
    {
        SaveCurrentCode();

        if (taskSolverPanel != null) taskSolverPanel.SetActive(false);
        if (taskListPanel != null) taskListPanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        RefreshLocationTabs();
        RefreshTaskList();
        currentTaskIndex = -1;
    }

    private void SaveCurrentCode()
    {
        if (currentTaskIndex >= 0 && Core.TaskManager.Instance != null && codeInputField != null)
            Core.TaskManager.Instance.SaveTaskCode(currentTaskIndex, codeInputField.text);
    }

    // === ЗАПУСК КОДА ===

    private void RunCode()
    {
        if (codeInputField == null || string.IsNullOrWhiteSpace(codeInputField.text))
        {
            AppendOutput("Введите код для выполнения!", errorColor);
            return;
        }

        // Проверка голода
        if (Core.NeedsManager.Instance != null && !Core.NeedsManager.Instance.CanWork())
        {
            AppendOutput("Ты слишком голоден! Купи еду в магазине.", errorColor);
            return;
        }

        SaveCurrentCode();

        ClearOutput();
        AppendOutput("Выполнение кода...", normalColor);
        AppendOutput("========================================", normalColor);

        lastOutput = "";
        pythonExecutor.ExecutePythonCode(codeInputField.text);
    }

    // === ОБРАБОТКА РЕЗУЛЬТАТОВ ===

    private void OnPythonOutput(string output)
    {
        lastOutput = output;
        AppendOutput(output, normalColor);
    }

    private void OnPythonError(string error)
    {
        lastOutput = "";
        AppendOutput($"ОШИБКА:\n{error}", errorColor);
    }

    private void OnPythonCompleted(int exitCode)
    {
        AppendOutput("========================================", normalColor);

        if (exitCode == 0)
        {
            AppendOutput("Код выполнен успешно!", successColor);
            CheckSolutionAutomatically();
        }
        else
        {
            AppendOutput($"Код завершился с ошибкой (код: {exitCode})", errorColor);
        }

        ScrollToBottom();
    }

    private void CheckSolutionAutomatically()
    {
        if (currentTaskIndex < 0 || Core.TaskManager.Instance == null) return;

        var tasks = Core.TaskManager.Instance.GetTasks();
        if (currentTaskIndex >= tasks.Count) return;

        Core.PythonTask task = tasks[currentTaskIndex];
        if (task.isCompleted || string.IsNullOrEmpty(lastOutput)) return;

        if (task.CheckOutput(lastOutput))
        {
            AppendOutput("", normalColor);
            AppendOutput("========================================", successColor);
            AppendOutput("✓✓✓ ПРАВИЛЬНО! ЗАДАЧА ВЫПОЛНЕНА! ✓✓✓", successColor);
            AppendOutput($"Награда: +{task.rewardXP} XP, +{task.rewardMoney} монет", successColor);
            AppendOutput("========================================", successColor);

            Core.TaskManager.Instance.CompleteTask(currentTaskIndex);
        }
        else
        {
            AppendOutput("", normalColor);
            AppendOutput("========================================", errorColor);
            AppendOutput("✗ Результат не совпадает с ожидаемым выводом.", errorColor);
            AppendOutput("Код может быть любым — главное чтобы вывод совпадал!", errorColor);
            AppendOutput("========================================", errorColor);
        }
    }

    // === РЕДАКТОР КОДА (Tab + автоиндент) ===

    private void HandleCodeEditorInput(UnityEngine.InputSystem.Keyboard keyboard)
    {
        if (keyboard.tabKey.wasPressedThisFrame)
        {
            InsertTextAtCaret(INDENT);
            codeInputField.ActivateInputField();
            codeInputField.Select();
        }

        if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame)
            HandleAutoIndent();
    }

    private void InsertTextAtCaret(string textToInsert)
    {
        if (codeInputField == null) return;
        string text = codeInputField.text;
        int pos = codeInputField.caretPosition;
        codeInputField.text = text.Insert(pos, textToInsert);
        codeInputField.caretPosition = pos + textToInsert.Length;
        codeInputField.selectionAnchorPosition = codeInputField.caretPosition;
        codeInputField.selectionFocusPosition = codeInputField.caretPosition;
    }

    private void HandleAutoIndent()
    {
        if (codeInputField == null) return;

        string text = codeInputField.text;
        int pos = codeInputField.caretPosition;

        int lineStart = text.LastIndexOf('\n', Mathf.Max(0, pos - 1));
        lineStart = (lineStart < 0) ? 0 : lineStart + 1;

        string currentLine = text.Substring(lineStart, pos - lineStart);

        string indent = "";
        foreach (char c in currentLine)
        {
            if (c == ' ') indent += ' ';
            else break;
        }

        if (currentLine.TrimEnd().EndsWith(":"))
            indent += INDENT;

        if (indent.Length > 0)
            StartCoroutine(InsertIndentNextFrame(indent));
    }

    private IEnumerator InsertIndentNextFrame(string indent)
    {
        yield return null;
        InsertTextAtCaret(indent);
    }

    // === ВЫВОД ===

    private void ClearOutput()
    {
        if (outputText != null) outputText.text = "";
        lastOutput = "";
    }

    private void AppendOutput(string text, Color color)
    {
        if (outputText != null)
        {
            string hex = ColorUtility.ToHtmlStringRGBA(color);
            outputText.text += $"<color=#{hex}>{text}</color>\n";
        }
    }

    private void ScrollToBottom()
    {
        if (outputScrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            outputScrollRect.verticalNormalizedPosition = 0f;
        }
    }
}
