using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace Core
{
    /// <summary>
    /// Управляет локациями и телепортом.
    /// После 5 задач показывает модальное окно "Телепортироваться" (без отмены).
    /// После всех 15 задач — экран победы.
    ///
    /// В Unity:
    /// 1. Создать GameObject "LocationManager"
    /// 2. Назначить 3 локации (stock1/2/3), спаун точки, игрока
    /// 3. Создать teleportPanel с текстом и кнопкой "Телепортироваться"
    /// 4. Создать victoryPanel с текстом
    /// </summary>
    public class LocationManager : MonoBehaviour
    {
        public static LocationManager Instance { get; private set; }

        [Header("Locations")]
        [SerializeField] private GameObject stock1;
        [SerializeField] private GameObject stock2;
        [SerializeField] private GameObject stock3;

        [Header("Player Spawn Points")]
        [SerializeField] private Transform stock1SpawnPoint;
        [SerializeField] private Transform stock2SpawnPoint;
        [SerializeField] private Transform stock3SpawnPoint;

        [Header("Player Reference")]
        [SerializeField] private GameObject player;

        [Header("Teleport Modal UI")]
        [SerializeField] private GameObject teleportPanel;       // модальное окно
        [SerializeField] private TMP_Text teleportText;           // текст сообщения
        [SerializeField] private Button teleportButton;           // кнопка "Телепортироваться" (ONLY)

        [Header("Victory UI")]
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private TMP_Text victoryText;

        private int currentLocationIndex = 0;
        private int pendingLocationIndex = -1; // куда телепортироваться

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }
        }

        private void Start()
        {
            if (teleportPanel != null) teleportPanel.SetActive(false);
            if (victoryPanel != null) victoryPanel.SetActive(false);

            if (teleportButton != null)
                teleportButton.onClick.AddListener(OnTeleportButtonClicked);

            // Загружаем сохранённую локацию
            int savedLoc = SaveManager.Instance != null ? SaveManager.Instance.GetSavedLocation() : 0;
            InitializeLocations();
            if (savedLoc > 0) MoveToLocation(savedLoc);
        }

        private void InitializeLocations()
        {
            if (stock1 != null) stock1.SetActive(true);
            if (stock2 != null) stock2.SetActive(false);
            if (stock3 != null) stock3.SetActive(false);

            currentLocationIndex = 0;

            if (player != null && stock1SpawnPoint != null)
                TeleportPlayer(stock1SpawnPoint);
        }

        /// <summary>Вызывается из TaskManager при завершении задачи</summary>
        public void CheckLocationProgress()
        {
            if (TaskManager.Instance == null) return;

            // Проверяем, завершены ли все 5 задач текущей локации
            if (TaskManager.Instance.IsLocationCompleted(currentLocationIndex))
            {
                int nextLocation = currentLocationIndex + 1;

                if (nextLocation >= TaskManager.TOTAL_LOCATIONS)
                {
                    // Все 3 локации пройдены!
                    ShowVictory();
                }
                else
                {
                    // Показываем модальное окно телепорта
                    ShowTeleportModal(nextLocation);
                }
            }
        }

        // === МОДАЛЬНОЕ ОКНО ТЕЛЕПОРТА ===

        private void ShowTeleportModal(int nextLocationIndex)
        {
            pendingLocationIndex = nextLocationIndex;

            string currentName = TaskManager.Instance.GetLocationName(currentLocationIndex);
            string nextName = TaskManager.Instance.GetLocationName(nextLocationIndex);

            if (teleportPanel != null) teleportPanel.SetActive(true);
            if (teleportText != null)
            {
                teleportText.text = $"Поздравляем!\n\n" +
                    $"Ты прошёл локацию \"{currentName}\"!\n" +
                    $"Все {TaskManager.TASKS_PER_LOCATION} задач выполнены.\n\n" +
                    $"Следующая локация: \"{nextName}\"";
            }

            // Блокируем игру — отменить нельзя!
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            Debug.Log($"LocationManager: Показано окно телепорта в {nextName}");
        }

        private void OnTeleportButtonClicked()
        {
            if (pendingLocationIndex < 0) return;

            // Скрываем модальное окно
            if (teleportPanel != null) teleportPanel.SetActive(false);

            // Переходим на новую локацию
            MoveToLocation(pendingLocationIndex);
            pendingLocationIndex = -1;

            // Возобновляем игру
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // === ЛОКАЦИИ ===

        private void MoveToLocation(int locationIndex)
        {
            currentLocationIndex = locationIndex;

            // Выключаем все
            if (stock1 != null) stock1.SetActive(false);
            if (stock2 != null) stock2.SetActive(false);
            if (stock3 != null) stock3.SetActive(false);

            // Включаем нужную
            switch (locationIndex)
            {
                case 0:
                    if (stock1 != null) stock1.SetActive(true);
                    TeleportPlayer(stock1SpawnPoint);
                    break;
                case 1:
                    if (stock2 != null) stock2.SetActive(true);
                    TeleportPlayer(stock2SpawnPoint);
                    break;
                case 2:
                    if (stock3 != null) stock3.SetActive(true);
                    TeleportPlayer(stock3SpawnPoint);
                    break;
            }

            Debug.Log($"LocationManager: Перешли на локацию {TaskManager.Instance?.GetLocationName(locationIndex)}");
        }

        private void TeleportPlayer(Transform destination)
        {
            if (player == null || destination == null) return;

            CharacterController controller = player.GetComponent<CharacterController>();
            Rigidbody rb = player.GetComponent<Rigidbody>();

            if (controller != null) controller.enabled = false;
            if (rb != null) { rb.linearVelocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }

            player.transform.position = destination.position;
            player.transform.rotation = destination.rotation;

            Physics.SyncTransforms();

            if (controller != null) controller.enabled = true;
        }

        // === ПОБЕДА ===

        private void ShowVictory()
        {
            Debug.Log("Поздравляем! Все задачи выполнены!");

            if (victoryPanel != null) victoryPanel.SetActive(true);
            if (victoryText != null)
            {
                int totalTasks = TaskManager.TASKS_PER_LOCATION * TaskManager.TOTAL_LOCATIONS;
                victoryText.text = $"Поздравляем!\n\n" +
                    $"Ты прошёл игру!\n" +
                    $"Все {totalTasks} задач выполнены!\n\n" +
                    $"Ты прошёл путь от подвала до офиса.\n" +
                    $"Теперь ты Python-разработчик!";
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0f;
        }

        public void RestartGame()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void ExitToMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("StartMenu");
        }

        public int GetCurrentLocationIndex() => currentLocationIndex;
    }
}
