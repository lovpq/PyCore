using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Core
{
    public class TaskUIManager : MonoBehaviour
    {
        public static TaskUIManager Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private GameObject taskWindow;
        [SerializeField] private Transform taskListContainer;
        [SerializeField] private GameObject taskItemPrefab;
        [SerializeField] private Button closeButton;

        [Header("Task Detail Panel")]
        [SerializeField] private GameObject taskDetailPanel;
        [SerializeField] private TextMeshProUGUI taskTitleText;
        [SerializeField] private TextMeshProUGUI taskDescriptionText;
        [SerializeField] private TextMeshProUGUI taskCodeText;
        [SerializeField] private TextMeshProUGUI taskRewardText;
        [SerializeField] private Button completeButton;
        [SerializeField] private Button backButton;

        private List<GameObject> taskItemInstances = new List<GameObject>();
        private int selectedTaskIndex = -1;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(CloseTaskWindow);
            }

            if (completeButton != null)
            {
                completeButton.onClick.AddListener(CompleteCurrentTask);
            }

            if (backButton != null)
            {
                backButton.onClick.AddListener(BackToTaskList);
            }

            if (taskWindow != null)
            {
                taskWindow.SetActive(false);
            }

            if (taskDetailPanel != null)
            {
                taskDetailPanel.SetActive(false);
            }
        }

        public void OpenTaskWindow()
        {
            if (taskWindow != null)
            {
                taskWindow.SetActive(true);
                RefreshTaskList();

                // Делегируем управление курсором/паузой в InputSystemHelper
                InputSystemHelper inputHelper = FindObjectOfType<InputSystemHelper>();
                if (inputHelper != null)
                {
                    inputHelper.DisableGameplayInput();
                }
                else
                {
                    // Фоллбэк если InputSystemHelper нет в сцене
                    Time.timeScale = 0f;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
            }
        }

        public void CloseTaskWindow()
        {
            if (taskDetailPanel != null)
            {
                taskDetailPanel.SetActive(false);
            }

            if (taskWindow != null)
            {
                taskWindow.SetActive(false);

                InputSystemHelper inputHelper = FindObjectOfType<InputSystemHelper>();
                if (inputHelper != null)
                {
                    inputHelper.EnableGameplayInput();
                }
                else
                {
                    Time.timeScale = 1f;
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }
        }

        private void RefreshTaskList()
        {
            foreach (var item in taskItemInstances)
            {
                Destroy(item);
            }
            taskItemInstances.Clear();

            if (TaskManager.Instance == null) return;

            List<PythonTask> tasks = TaskManager.Instance.GetTasks();

            for (int i = 0; i < tasks.Count; i++)
            {
                int taskIndex = i;
                GameObject taskItem = Instantiate(taskItemPrefab, taskListContainer);
                taskItemInstances.Add(taskItem);

                TextMeshProUGUI titleText = taskItem.transform.Find("TaskTitle")?.GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI statusText = taskItem.transform.Find("Status")?.GetComponent<TextMeshProUGUI>();
                Button selectButton = taskItem.GetComponent<Button>();

                if (titleText != null)
                {
                    titleText.text = tasks[i].taskTitle;
                }

                if (statusText != null)
                {
                    statusText.text = tasks[i].isCompleted ? "✓ Выполнено" : "○ Не выполнено";
                    statusText.color = tasks[i].isCompleted ? Color.green : Color.yellow;
                }

                if (selectButton != null)
                {
                    selectButton.onClick.AddListener(() => ShowTaskDetail(taskIndex));
                }
            }
        }

        private void ShowTaskDetail(int taskIndex)
        {
            if (TaskManager.Instance == null) return;

            List<PythonTask> tasks = TaskManager.Instance.GetTasks();
            if (taskIndex < 0 || taskIndex >= tasks.Count) return;

            selectedTaskIndex = taskIndex;
            PythonTask task = tasks[taskIndex];

            if (taskDetailPanel != null)
            {
                taskDetailPanel.SetActive(true);
            }

            if (taskTitleText != null)
            {
                taskTitleText.text = task.taskTitle;
            }

            if (taskDescriptionText != null)
            {
                taskDescriptionText.text = task.taskDescription;
            }

            if (taskCodeText != null)
            {
                taskCodeText.text = task.isCompleted ? task.expectedCode : "Решите задачу, чтобы увидеть код";
            }

            if (taskRewardText != null)
            {
                taskRewardText.text = $"Награда: {task.rewardXP} XP, {task.rewardMoney} монет";
            }

            if (completeButton != null)
            {
                completeButton.gameObject.SetActive(!task.isCompleted);
            }
        }

        private void CompleteCurrentTask()
        {
            if (selectedTaskIndex >= 0 && TaskManager.Instance != null)
            {
                TaskManager.Instance.CompleteTask(selectedTaskIndex);
                RefreshTaskList();
                ShowTaskDetail(selectedTaskIndex);
            }
        }

        private void BackToTaskList()
        {
            if (taskDetailPanel != null)
            {
                taskDetailPanel.SetActive(false);
            }
            selectedTaskIndex = -1;
        }
    }
}
