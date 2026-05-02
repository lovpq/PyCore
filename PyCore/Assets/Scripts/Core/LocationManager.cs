using System.Collections;
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
        [SerializeField] private Button victoryRestartButton;   // кнопка "Играть снова" на экране победы
        [SerializeField] private Button victoryMenuButton;       // кнопка "В меню" на экране победы

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

            // Кнопки победного экрана
            if (victoryRestartButton != null)
                victoryRestartButton.onClick.AddListener(RestartGame);
            if (victoryMenuButton != null)
                victoryMenuButton.onClick.AddListener(ExitToMainMenu);

            // Загружаем сохранение — делаем здесь, чтобы порядок был гарантирован
            if (SaveManager.Instance != null && SaveManager.Instance.HasSave())
                SaveManager.Instance.LoadGame();

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
                StartCoroutine(TeleportNextFrame(stock1SpawnPoint));
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
            InputSystemHelper.Instance?.EnableUIModeWithPause();

            Debug.Log($"LocationManager: Показано окно телепорта в {nextName}");
        }

        private void OnTeleportButtonClicked()
        {
            if (pendingLocationIndex < 0) return;

            if (teleportPanel != null) teleportPanel.SetActive(false);

            // EnableGameplayInput ПЕРЕД MoveToLocation — восстанавливаем timeScale=1,
            // чтобы WaitForFixedUpdate/yield return null в корутине телепорта не завис
            InputSystemHelper.Instance?.EnableGameplayInput();

            MoveToLocation(pendingLocationIndex);
            pendingLocationIndex = -1;
        }

        // === ЛОКАЦИИ ===

        private void MoveToLocation(int locationIndex)
        {
            currentLocationIndex = locationIndex;

            // Выключаем все
            if (stock1 != null) stock1.SetActive(false);
            if (stock2 != null) stock2.SetActive(false);
            if (stock3 != null) stock3.SetActive(false);

            // Включаем нужную и телепортируем через корутину,
            // чтобы физика успела зарегистрировать новые коллайдеры
            switch (locationIndex)
            {
                case 0:
                    if (stock1 != null) stock1.SetActive(true);
                    StartCoroutine(TeleportNextFrame(stock1SpawnPoint));
                    break;
                case 1:
                    if (stock2 != null) stock2.SetActive(true);
                    StartCoroutine(TeleportNextFrame(stock2SpawnPoint));
                    break;
                case 2:
                    if (stock3 != null) stock3.SetActive(true);
                    StartCoroutine(TeleportNextFrame(stock3SpawnPoint));
                    break;
            }

            Debug.Log($"LocationManager: Перешли на локацию {TaskManager.Instance?.GetLocationName(locationIndex)}");
        }

        /// <summary>
        /// Ждёт физический кадр, чтобы коллайдеры включённой локации зарегистрировались,
        /// и только потом телепортирует игрока. WaitForFixedUpdate не зависит от timeScale.
        /// </summary>
        private IEnumerator TeleportNextFrame(Transform destination)
        {
            yield return new WaitForFixedUpdate();
            Physics.SyncTransforms();
            TeleportPlayer(destination);
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

            InputSystemHelper.Instance?.EnableUIModeWithPause();
        }

        public void RestartGame()
        {
            Time.timeScale = 1f;
            if (GameManager.Instance != null)
                GameManager.Instance.StartNewGame();
            if (SaveManager.Instance != null)
                SaveManager.Instance.DeleteSave();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void ExitToMainMenu()
        {
            Time.timeScale = 1f;
            if (SaveManager.Instance != null)
                SaveManager.Instance.SaveGame();
            SceneManager.LoadScene("StartMenu");
        }

        public int GetCurrentLocationIndex() => currentLocationIndex;
    }
}
