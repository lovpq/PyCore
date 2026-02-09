using UnityEngine;

namespace Core
{
    /// <summary>
    /// Сохранение / загрузка игрового прогресса через PlayerPrefs.
    /// Сохраняет: деньги, уровень, опыт, голод, локацию, выполненные задачи, код игрока.
    ///
    /// В Unity: создать GameObject "SaveManager", повесить этот скрипт.
    /// Автосохранение каждые 30 секунд + при выходе из игры.
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        private const string KEY_PREFIX = "pycore_";
        private const float AUTOSAVE_INTERVAL = 30f;
        private float autosaveTimer = 0f;

        private void Awake()
        {
            if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
            else { Destroy(gameObject); }
        }

        private void Update()
        {
            autosaveTimer += Time.deltaTime;
            if (autosaveTimer >= AUTOSAVE_INTERVAL)
            {
                autosaveTimer = 0f;
                SaveGame();
            }
        }

        // Сохраняем при сворачивании и выходе
        private void OnApplicationPause(bool pause) { if (pause) SaveGame(); }
        private void OnApplicationQuit() { SaveGame(); }
        private void OnDestroy() { if (Instance == this) SaveGame(); }

        // === СОХРАНЕНИЕ ===

        public void SaveGame()
        {
            // PlayerData
            PlayerData data = GameManager.Instance?.playerData;
            if (data != null)
            {
                PlayerPrefs.SetInt(KEY_PREFIX + "level", data.level);
                PlayerPrefs.SetInt(KEY_PREFIX + "experience", data.experience);
                PlayerPrefs.SetInt(KEY_PREFIX + "money", data.money);
                PlayerPrefs.SetFloat(KEY_PREFIX + "hunger", data.hunger);
                PlayerPrefs.SetFloat(KEY_PREFIX + "health", data.health);
            }

            // Локация
            if (LocationManager.Instance != null)
                PlayerPrefs.SetInt(KEY_PREFIX + "location", LocationManager.Instance.GetCurrentLocationIndex());

            // Задачи
            if (TaskManager.Instance != null)
            {
                var tasks = TaskManager.Instance.GetTasks();
                for (int i = 0; i < tasks.Count; i++)
                {
                    PlayerPrefs.SetInt(KEY_PREFIX + "task_done_" + i, tasks[i].isCompleted ? 1 : 0);
                    if (!string.IsNullOrEmpty(tasks[i].savedCode))
                        PlayerPrefs.SetString(KEY_PREFIX + "task_code_" + i, tasks[i].savedCode);
                }
            }

            PlayerPrefs.SetInt(KEY_PREFIX + "has_save", 1);
            PlayerPrefs.Save();
            Debug.Log("SaveManager: Игра сохранена!");
        }

        // === ЗАГРУЗКА ===

        public bool HasSave()
        {
            return PlayerPrefs.GetInt(KEY_PREFIX + "has_save", 0) == 1;
        }

        public void LoadGame()
        {
            if (!HasSave()) return;

            // PlayerData
            PlayerData data = GameManager.Instance?.playerData;
            if (data != null)
            {
                data.level = PlayerPrefs.GetInt(KEY_PREFIX + "level", 1);
                data.experience = PlayerPrefs.GetInt(KEY_PREFIX + "experience", 0);
                data.money = PlayerPrefs.GetInt(KEY_PREFIX + "money", 0);
                data.hunger = PlayerPrefs.GetFloat(KEY_PREFIX + "hunger", 50f);
                data.health = PlayerPrefs.GetFloat(KEY_PREFIX + "health", 100f);
            }

            // Задачи
            if (TaskManager.Instance != null)
            {
                var tasks = TaskManager.Instance.GetTasks();
                for (int i = 0; i < tasks.Count; i++)
                {
                    tasks[i].isCompleted = PlayerPrefs.GetInt(KEY_PREFIX + "task_done_" + i, 0) == 1;
                    tasks[i].savedCode = PlayerPrefs.GetString(KEY_PREFIX + "task_code_" + i, "");
                }
            }

            Debug.Log("SaveManager: Игра загружена!");
        }

        /// <summary>Загружает сохранённый номер локации</summary>
        public int GetSavedLocation()
        {
            return PlayerPrefs.GetInt(KEY_PREFIX + "location", 0);
        }

        public void DeleteSave()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.Log("SaveManager: Сохранение удалено");
        }
    }
}
