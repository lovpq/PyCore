using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// управляет меню настроек
/// 
/// что делает этот скрипт:
/// - Обрабатывает кнопку "Назад" в меню настроек
/// - Возвращает игрока в главное меню
/// - Связывает SettingsMenu с MainMenuManager
/// 
/// как использовать в unity:
/// 1. Добавьте этот скрипт на объект Canvas настроек
/// 2. Назначьте кнопку "Назад" в Inspector
/// 3. Убедитесь, что в сцене есть MainMenuManager
/// 
/// как работает:
/// - При запуске находит MainMenuManager в сцене
/// - При клике на "Назад" вызывает функцию MainMenuManager
/// </summary>
public class SettingsMenuController : MonoBehaviour
{
    // кнопка "Назад" (возврат в главное меню)
    public Button backButton;
    // ссылка на менеджер главного меню
    private MainMenuManager mainMenuManager;

    /// <summary>
    /// вызывается Unity при запуске скрипта
    /// находит MainMenuManager и настраивает кнопку
    /// </summary>
    void Start()
    {
        // ищем MainMenuManager в сцене
        // FindFirstObjectByType находит первый объект указанного типа
        mainMenuManager = FindFirstObjectByType<MainMenuManager>();
        
        // если кнопка "Назад" назначена в Inspector
        if (backButton != null)
        {
            // добавляем обработчик клика кнопки
            // при клике будет вызвана функция OnBackButtonClicked
            backButton.onClick.AddListener(OnBackButtonClicked);
        }
    }

    /// <summary>
    /// вызывается при клике на кнопку "Назад"
    /// возвращает игрока в главное меню
    /// </summary>
    void OnBackButtonClicked()
    {
        // если MainMenuManager найден
        if (mainMenuManager != null)
        {
            // вызываем функцию возврата в главное меню
            // MainMenuManager скроет настройки и покажет главное меню
            mainMenuManager.OnBackFromSettings();
        }
    }
}
