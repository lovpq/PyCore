using UnityEngine;

/// <summary>
/// главный менеджер игры - управляет всей игрой
/// 
/// что делает этот скрипт:
/// - Хранит данные игрока (PlayerData)
/// - Реализует паттерн Singleton (существует в единственном экземпляре)
/// - Не уничтожается при смене сцен (DontDestroyOnLoad)
/// - Инициализирует игру при запуске
/// 
/// как использовать в unity:
/// 1. Создайте пустой GameObject и назовите его "GameManager"
/// 2. Добавьте этот скрипт на объект
/// 3. Скрипт автоматически настроится при запуске игры
/// 4. Доступ из других скриптов: GameManager.Instance
/// 
/// как работает:
/// - Singleton pattern гарантирует только один экземпляр в игре
/// - DontDestroyOnLoad сохраняет объект между сценами
/// - Все скрипты обращаются к данным игрока через этот менеджер
/// </summary>
public class GameManager : MonoBehaviour
{
    // === SINGLETON PATTERN ===
    // Static означает, что переменная принадлежит классу, а не объекту
    // Instance - единственный экземпляр GameManager во всей игре
    // { get; private set; } означает: можно читать откуда угодно, но изменять только внутри этого класса
    public static GameManager Instance { get; private set; }

    [Header("Player Data")]
    // данные игрока (уровень, опыт, деньги, характеристики)
    public PlayerData playerData;
    
    // свойство для доступа к данным игрока (альтернативный способ)
    // => означает "expression-bodied property" (короткая запись get)
    public PlayerData PlayerData => playerData;

    /// <summary>
    /// вызывается Unity при создании объекта (перед Start)
    /// настраивает Singleton и инициализирует игру
    /// </summary>
    void Awake()
    {
        // проверяем, является ли этот объект первым GameManager
        if (Instance == null)
        {
            // если это первый GameManager, сохраняем ссылку на него
            Instance = this;
            // DontDestroyOnLoad делает объект постоянным (не уничтожается при смене сцен)
            // это нужно, чтобы данные игрока сохранялись между сценами
            DontDestroyOnLoad(gameObject);
            // инициализируем игру (создаем PlayerData)
            InitializeGame();
        }
        else
        {
            // если GameManager уже существует (это дубликат)
            // уничтожаем этот объект, чтобы остался только один GameManager
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// инициализирует игру - создает данные игрока, если их нет
    /// </summary>
    void InitializeGame()
    {
        // проверяем, созданы ли данные игрока
        if (playerData == null)
        {
            // если данных нет, создаем новый экземпляр PlayerData
            // это вызовет конструктор PlayerData() с начальными значениями
            playerData = new PlayerData();
        }
    }

    /// <summary>
    /// начинает новую игру - сбрасывает все данные игрока
    /// вызывается при нажатии кнопки "Новая игра"
    /// </summary>
    public void StartNewGame()
    {
        // создаем новый экземпляр PlayerData (это сбросит все прогресс)
        playerData = new PlayerData();
    }
}
