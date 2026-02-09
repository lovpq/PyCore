using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using TMPro;
using System;
using Keyboard = UnityEngine.InputSystem.Keyboard;
using Mouse = UnityEngine.InputSystem.Mouse;

namespace Core
{
    /// <summary>
    /// Управляет потребностями игрока.
    /// 3 шкалы: опыт, баланс, голод (насыщение).
    /// Голод падает со временем. При 0 = смерть (потеря прогресса).
    /// Сон каждые 10 минут реального времени.
    /// Магазин с 6 товарами еды.
    ///
    /// В Unity: создать GameObject "NeedsManager", повесить скрипт,
    /// назначить UI элементы.
    /// </summary>
    public class NeedsManager : MonoBehaviour
    {
        public static NeedsManager Instance { get; private set; }
        
        /// <summary>Проверяет, открыта ли хотя бы одна UI панель</summary>
        public bool IsAnyPanelOpen()
        {
            return (infoPanel != null && infoPanel.activeSelf) ||
                   (shopPanel != null && shopPanel.activeSelf) ||
                   (sleepPanel != null && sleepPanel.activeSelf) ||
                   (warningPanel != null && warningPanel.activeSelf);
        }

        // === HUD (всегда видны) ===
        [Header("HUD - 3 шкалы")]
        [SerializeField] private Slider expBar;         // шкала опыта
        [SerializeField] private Slider moneyBar;       // шкала баланса
        [SerializeField] private Slider hungerBar;      // шкала голода (насыщения)
        [SerializeField] private TMP_Text expText;
        [SerializeField] private TMP_Text moneyText;
        [SerializeField] private TMP_Text hungerText;

        // === КНОПКИ HUD ===
        [Header("HUD Buttons")]
        [SerializeField] private Button infoButton;     // кнопка "Информация"
        [SerializeField] private Button shopButton;     // кнопка "Магазин"

        // === ПАНЕЛЬ ИНФОРМАЦИИ ===
        [Header("Info Panel")]
        [SerializeField] private GameObject infoPanel;
        [SerializeField] private Button closeInfoButton;

        // === МАГАЗИН (6 товаров) ===
        [Header("Shop Panel")]
        [SerializeField] private GameObject shopPanel;
        [SerializeField] private TMP_Text shopBalanceText;
        [SerializeField] private Button closeShopButton;

        // 6 кнопок "Купить" для 6 товаров
        [Header("Shop Items (6)")]
        [SerializeField] private Button[] shopBuyButtons = new Button[6];

        // === СОН ===
        [Header("Sleep")]
        [SerializeField] private GameObject sleepPanel;
        [SerializeField] private TMP_Text sleepTimerText;    // "Можно поспать через 5:30"
        [SerializeField] private Button sleepButton;
        [SerializeField] private Button closeSleepButton;

        // === ПРЕДУПРЕЖДЕНИЕ ===
        [Header("Warning")]
        [SerializeField] private GameObject warningPanel;
        [SerializeField] private TMP_Text warningText;

        // === НАСТРОЙКИ ===
        [Header("Settings")]
        [SerializeField] private float hungerDrainPerMinute = 3f;
        [SerializeField] private float sleepCooldownMinutes = 10f; // реальных минут

        // 6 товаров: [цена, очки насыщения]
        private readonly int[,] shopItems = {
            { 15, 10 },   // Хлеб
            { 25, 20 },   // Лапша
            { 40, 30 },   // Бутерброд
            { 60, 45 },   // Суп
            { 80, 60 },   // Пицца
            { 120, 90 },  // Обед
        };

        private PlayerData data;
        private float hungerTimer = 0f;
        private float lastSleepTime = -999f; // время последнего сна
        private float warningTimer = 0f;
        private EasyPeasyFirstPersonController.FirstPersonController fpsController;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }
        }

        private void Start()
        {
            Debug.Log("NeedsManager: Start called");
            data = GameManager.Instance != null ? GameManager.Instance.playerData : new PlayerData();
            lastSleepTime = Time.time;
            
            // Найти FirstPersonController в сцене
            fpsController = FindFirstObjectByType<EasyPeasyFirstPersonController.FirstPersonController>();
            Debug.Log($"NeedsManager: FPS Controller found: {fpsController != null}");

            // Кнопки HUD
            if (infoButton != null)
            {
                Debug.Log("NeedsManager: Subscribing to Info button");
                infoButton.onClick.AddListener(OpenInfoPanel);
            }
            else
            {
                Debug.LogWarning("NeedsManager: Info button is NULL!");
            }
            
            if (shopButton != null)
            {
                Debug.Log("NeedsManager: Subscribing to Shop button");
                shopButton.onClick.AddListener(OpenShopPanel);
            }
            else
            {
                Debug.LogWarning("NeedsManager: Shop button is NULL!");
            }

            // Панель инфо
            if (closeInfoButton != null) closeInfoButton.onClick.AddListener(CloseInfoPanel);

            // Магазин
            if (closeShopButton != null) closeShopButton.onClick.AddListener(CloseShopPanel);
            for (int i = 0; i < shopBuyButtons.Length && i < 6; i++)
            {
                int idx = i;
                if (shopBuyButtons[i] != null)
                    shopBuyButtons[i].onClick.AddListener(() => BuyFood(idx));
            }

            // Сон
            if (sleepButton != null) sleepButton.onClick.AddListener(DoSleep);
            if (closeSleepButton != null) closeSleepButton.onClick.AddListener(CloseSleepPanel);

            // Скрываем
            if (infoPanel != null) infoPanel.SetActive(false);
            if (shopPanel != null) shopPanel.SetActive(false);
            if (sleepPanel != null) sleepPanel.SetActive(false);
            if (warningPanel != null) warningPanel.SetActive(false);

            // Загружаем сохранение
            if (SaveManager.Instance != null && SaveManager.Instance.HasSave())
                SaveManager.Instance.LoadGame();
        }

        private void Update()
        {
            if (data == null) return;

            // ТЕСТ: проверяем клики мыши
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                Debug.Log($"NeedsManager: Mouse click! Cursor state = {Cursor.lockState}, visible = {Cursor.visible}");
            }

            // Голод падает со временем
            hungerTimer += Time.deltaTime;
            if (hungerTimer >= 60f)
            {
                hungerTimer = 0f;
                data.hunger = Mathf.Max(0f, data.hunger - hungerDrainPerMinute);
            }

            // Голод = 0 → смерть (game over)
            if (data.hunger <= 0f)
            {
                data.hunger = 0f;
                OnPlayerDeath();
            }

            // Предупреждение при низком голоде
            if (data.hunger <= 15f && data.hunger > 0f)
            {
                ShowWarning("⚠ Ты голодаешь! Купи еду в магазине!");
            }

            // Таймер предупреждения
            if (warningTimer > 0f)
            {
                warningTimer -= Time.deltaTime;
                if (warningTimer <= 0f && warningPanel != null)
                    warningPanel.SetActive(false);
            }

            UpdateHUD();
            UpdateSleepTimer();
        }

        // === HUD ===

        private void UpdateHUD()
        {
            // Опыт: текущий/нужный
            if (expBar != null)
            {
                int needed = data.GetExperienceToNextLevel();
                expBar.value = needed > 0 ? (float)data.experience / needed : 1f;
            }
            if (expText != null)
                expText.text = $"Ур. {data.level} ({data.experience}/{data.GetExperienceToNextLevel()})";

            // Баланс
            if (moneyBar != null) moneyBar.value = Mathf.Clamp01(data.money / 500f);
            if (moneyText != null) moneyText.text = $"${data.money}";

            // Голод
            if (hungerBar != null) hungerBar.value = data.hunger / 100f;
            if (hungerText != null) hungerText.text = $"{Mathf.RoundToInt(data.hunger)}%";
        }

        // === СОН ===

        private float GetSleepCooldownRemaining()
        {
            float elapsed = Time.time - lastSleepTime;
            float cooldown = sleepCooldownMinutes * 60f;
            return Mathf.Max(0f, cooldown - elapsed);
        }

        private bool CanSleep() => GetSleepCooldownRemaining() <= 0f;

        private void UpdateSleepTimer()
        {
            if (sleepTimerText == null || sleepPanel == null || !sleepPanel.activeSelf) return;

            float remaining = GetSleepCooldownRemaining();
            if (remaining > 0f)
            {
                int min = Mathf.FloorToInt(remaining / 60f);
                int sec = Mathf.FloorToInt(remaining % 60f);
                sleepTimerText.text = $"Можно поспать через {min}:{sec:D2}";
                if (sleepButton != null) sleepButton.interactable = false;
            }
            else
            {
                sleepTimerText.text = "Можно поспать!";
                if (sleepButton != null) sleepButton.interactable = true;
            }
        }

        /// <summary>Открывает панель сна (вызывается из Interactable кровати)</summary>
        public void OpenSleepPanel()
        {
            if (sleepPanel != null) sleepPanel.SetActive(true);
            DisablePlayerControls();
        }

        private void DoSleep()
        {
            if (!CanSleep()) return;

            // Восстанавливаем голод на 25 (бонус за сон)
            data.hunger = Mathf.Min(100f, data.hunger + 25f);
            lastSleepTime = Time.time;

            Debug.Log("NeedsManager: Поспал! Голод +25");
            CloseSleepPanel();
        }

        private void CloseSleepPanel()
        {
            if (sleepPanel != null) sleepPanel.SetActive(false);
            EnablePlayerControls();
        }

        // === МАГАЗИН ===

        /// <summary>Открывает магазин (вызывается из Interactable / HUD кнопки)</summary>
        public void OpenShopPanel()
        {
            Debug.Log("NeedsManager: OpenShopPanel called!");
            if (shopPanel != null) shopPanel.SetActive(true);
            UpdateShopUI();
            DisablePlayerControls();
        }

        private void BuyFood(int itemIndex)
        {
            if (itemIndex < 0 || itemIndex >= 6) return;

            int cost = shopItems[itemIndex, 0];
            int saturation = shopItems[itemIndex, 1];

            if (data.money < cost)
            {
                ShowWarning($"Не хватает! Нужно ${cost}");
                return;
            }

            data.money -= cost;
            data.hunger = Mathf.Min(100f, data.hunger + saturation);
            Debug.Log($"NeedsManager: Купил еду #{itemIndex + 1} за ${cost}, насыщение +{saturation}");
            UpdateShopUI();
        }

        private void UpdateShopUI()
        {
            if (shopBalanceText != null) shopBalanceText.text = $"Баланс: ${data.money}";
        }

        private void CloseShopPanel()
        {
            if (shopPanel != null) shopPanel.SetActive(false);
            EnablePlayerControls();
        }

        // === ИНФОРМАЦИЯ ===

        private void EnablePlayerControls()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            if (fpsController != null)
            {
                fpsController.SetLookControl(true);
                fpsController.SetMoveControl(true);
            }
        }

        private void DisablePlayerControls()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (fpsController != null)
            {
                fpsController.SetLookControl(false);
                fpsController.SetMoveControl(false);
            }
        }

        private void OpenInfoPanel()
        {
            Debug.Log("NeedsManager: OpenInfoPanel called!");
            if (infoPanel != null) infoPanel.SetActive(true);
            DisablePlayerControls();
        }

        private void CloseInfoPanel()
        {
            Debug.Log("NeedsManager: CloseInfoPanel called!");
            if (infoPanel != null) infoPanel.SetActive(false);
            EnablePlayerControls();
        }

        // === СМЕРТЬ ===

        private void OnPlayerDeath()
        {
            ShowWarning("Ты умер от голода! Прогресс потерян.");

            // Сбрасываем голод, отнимаем деньги
            data.hunger = 50f;
            data.money = Mathf.Max(0, data.money - 50);

            Debug.Log("NeedsManager: Игрок умер от голода!");
        }

        // === УТИЛИТЫ ===

        private void ShowWarning(string msg)
        {
            if (warningPanel != null) warningPanel.SetActive(true);
            if (warningText != null) warningText.text = msg;
            warningTimer = 3f;
        }

        /// <summary>Можно ли работать (не мёртв)</summary>
        public bool CanWork() => data != null && data.hunger > 0f;
    }
}
