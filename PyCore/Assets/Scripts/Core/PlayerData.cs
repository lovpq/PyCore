using System;

/// <summary>
/// хранит все данные игрока (характеристики, прогресс, имущество)
/// 
/// что делает этот класс:
/// - Хранит уровень, опыт и деньги игрока
/// - Хранит характеристики: здоровье, голод, энергия
/// - Управляет навыками (например, pythonSkill)
/// - Отслеживает купленные предметы (компьютер, стол, кровать, лампа)
/// - Вычисляет систему уровней и опыта
/// 
/// как использовать:
/// - GameManager создает экземпляр этого класса
/// - Доступ к данным через GameManager.Instance.PlayerData
/// - Используйте функции AddExperience, AddMoney, SpendMoney для изменения данных
/// 
/// как работает:
/// - [Serializable] позволяет сохранять этот класс в файл
/// - Автоматически повышает уровень при накоплении достаточного опыта
/// </summary>
[Serializable] // атрибут делает класс сериализуемым (можно сохранить в JSON/файл)
public class PlayerData
{
    // === ПРОГРЕСС ИГРОКА ===
    // текущий уровень игрока (начинается с 1)
    public int level = 1;
    // текущий опыт игрока (обнуляется при повышении уровня)
    public int experience = 0;
    // текущее количество денег у игрока
    public int money = 0;
    
    // === ХАРАКТЕРИСТИКИ ===
    // здоровье игрока (0 до 100)
    public float health = 100f;
    // уровень голода (0 до 100, меньше = голоднее)
    public float hunger = 100f;
    
    // === ЛОКАЦИЯ ===
    // текущая локация игрока (название сцены)
    public string currentLocation = "Basement";
    
    // === ИМУЩЕСТВО (куплено или нет) ===
    // есть ли у игрока компьютер
    public bool hasComputer = true;
    // есть ли у игрока стол
    public bool hasDesk = false;
    // есть ли у игрока кровать
    public bool hasBed = false;
    // есть ли у игрока лампочка/освещение
    public bool hasLight = false;

    /// <summary>
    /// конструктор - вызывается при создании нового PlayerData
    /// устанавливает начальные значения для новой игры
    /// </summary>
    public PlayerData()
    {
        level = 1;
        experience = 0;
        money = 50;
        health = 100f;
        hunger = 80f;
        currentLocation = "Basement";
        hasComputer = true;
        hasDesk = true;
        hasBed = true;
        hasLight = true;
    }

    /// <summary>
    /// вычисляет, сколько опыта нужно для перехода на следующий уровень
    /// формула: текущий_уровень * 100
    /// </summary>
    /// <returns>Количество опыта для следующего уровня</returns>
    public int GetExperienceToNextLevel()
    {
        // чем выше уровень, тем больше опыта нужно
        // уровень 1 -> 100 опыта, Уровень 2 -> 200 опыта, и т.д.
        return level * 100;
    }

    /// <summary>
    /// добавляет опыт игроку и автоматически повышает уровень при необходимости
    /// </summary>
    /// <param name="amount">Количество опыта для добавления</param>
    public void AddExperience(int amount)
    {
        // прибавляем опыт к текущему
        // += означает: experience = experience + amount
        experience += amount;
        // проверяем, не пора ли повысить уровень
        CheckLevelUp();
    }

    /// <summary>
    /// добавляет деньги игроку
    /// </summary>
    /// <param name="amount">Количество денег для добавления</param>
    public void AddMoney(int amount)
    {
        // просто прибавляем деньги
        money += amount;
    }

    /// <summary>
    /// пытается потратить деньги игрока
    /// </summary>
    /// <param name="amount">Количество денег для траты</param>
    /// <returns>true, если денег хватило и они были потрачены; false, если денег недостаточно</returns>
    public bool SpendMoney(int amount)
    {
        // проверяем, достаточно ли денег у игрока
        // >= означает "больше или равно"
        if (money >= amount)
        {
            // если денег хватает, вычитаем их
            money -= amount;
            // возвращаем true (успешная покупка)
            return true;
        }
        // если денег не хватает, возвращаем false (покупка не удалась)
        return false;
    }

    /// <summary>
    /// проверяет и повышает уровень игрока, если опыта достаточно
    /// может повысить несколько уровней за раз
    /// </summary>
    void CheckLevelUp()
    {
        // while - цикл, который повторяется, пока условие истинно
        // продолжаем повышать уровень, пока опыта хватает
        while (experience >= GetExperienceToNextLevel())
        {
            // вычитаем опыт, потраченный на уровень
            experience -= GetExperienceToNextLevel();
            // повышаем уровень на 1
            // ++ означает: level = level + 1
            level++;
            
            // пример: 
            // уровень 1, опыт 250
            // 1. 250 >= 100? Да -> опыт = 150, уровень = 2
            // 2. 150 >= 200? Нет -> выход из цикла
        }
    }
}
