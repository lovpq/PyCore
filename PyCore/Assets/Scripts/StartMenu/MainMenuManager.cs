using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// управляет главным меню игры
/// 
/// что делает этот скрипт:
/// - Управляет переключением между панелями меню (главное, настройки, о игре)
/// - Обрабатывает кнопки "Играть", "Настройки", "О игре"
/// - Загружает сцену игры при нажатии "Играть"
/// - Интегрируется с SplashScreen для показа меню после заставки
/// 
/// как использовать в unity:
/// 1. Добавьте этот скрипт на GameObject в сцене главного меню
/// 2. Назначьте все панели и кнопки в Inspector
/// 3. Назначьте SplashScreen (если используется)
/// 4. Убедитесь, что сцена "Intro" существует в Build Settings
/// 
/// как работает:
/// - При запуске скрывает все панели меню
/// - Ждет завершения SplashScreen (если есть)
/// - Показывает главное меню после заставки
/// - SetActive(true/false) показывает/скрывает UI панели
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [Header("Menu Panels")]
    // панель главного меню (с кнопками "Играть", "Настройки", "О игре")
    public GameObject mainMenuPanel;
    // панель настроек (громкость, графика и т.д.)
    public GameObject settingsMenuPanel;
    // панель "О игре" (информация об игре)
    public GameObject aboutPanel;
    
    [Header("Buttons")]
    // кнопка "Играть" (начинает игру)
    public Button playButton;
    // кнопка "Настройки" (открывает меню настроек)
    public Button settingsButton;
    // кнопка "О игре" (открывает информацию об игре)
    public Button aboutButton;
    // кнопка "Закрыть" в панели "О игре"
    public Button closeAboutButton;

    [Header("Splash Screen")]
    // ссылка на SplashScreen (заставку с логотипом)
    public SplashScreen splashScreen;

    /// <summary>
    /// вызывается Unity при запуске скрипта
    /// настраивает кнопки и подписывается на события
    /// </summary>
    void Start()
    {
        // настраиваем кнопку "Играть"
        if (playButton != null)
            // при клике вызывается OnPlayButtonClicked
            playButton.onClick.AddListener(OnPlayButtonClicked);
            
        // настраиваем кнопку "Настройки"
        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettingsButtonClicked);
            
        // настраиваем кнопку "О игре"
        if (aboutButton != null)
            aboutButton.onClick.AddListener(OnAboutButtonClicked);
            
        // настраиваем кнопку "Закрыть" в панели "О игре"
        if (closeAboutButton != null)
            closeAboutButton.onClick.AddListener(OnCloseAboutButtonClicked);

        // скрываем все панели при запуске
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);
            
        if (aboutPanel != null)
            aboutPanel.SetActive(false);
            
        if (settingsMenuPanel != null)
            settingsMenuPanel.SetActive(false);

        // если есть SplashScreen (заставка)
        if (splashScreen != null)
        {
            // подписываемся на событие завершения анимации заставки
            // когда заставка завершится, вызовется ShowMainMenu
            splashScreen.onAnimationComplete.AddListener(ShowMainMenu);
        }
        else
        {
            // если заставки нет, сразу показываем главное меню
            ShowMainMenu();
        }
    }

    /// <summary>
    /// показывает главное меню
    /// вызывается после завершения SplashScreen или сразу
    /// </summary>
    public void ShowMainMenu()
    {
        // включаем панель главного меню
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
    }

    /// <summary>
    /// вызывается при клике на кнопку "Играть"
    /// загружает сцену Intro (начинает игру)
    /// </summary>
    public void OnPlayButtonClicked()
    {
        // загружаем сцену "Intro"
        // SceneManager.LoadScene загружает сцену по имени
        // убедитесь, что сцена "Intro" добавлена в Build Settings
        UnityEngine.SceneManagement.SceneManager.LoadScene("Intro");
    }

    /// <summary>
    /// вызывается при клике на кнопку "Настройки"
    /// показывает меню настроек, скрывает главное меню
    /// </summary>
    public void OnSettingsButtonClicked()
    {
        // скрываем главное меню
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);
            
        // показываем меню настроек
        if (settingsMenuPanel != null)
            settingsMenuPanel.SetActive(true);
    }

    /// <summary>
    /// вызывается при клике на кнопку "О игре"
    /// показывает панель с информацией об игре
    /// </summary>
    public void OnAboutButtonClicked()
    {
        // показываем панель "О игре"
        if (aboutPanel != null)
            aboutPanel.SetActive(true);
    }

    /// <summary>
    /// вызывается при клике на кнопку "Закрыть" в панели "О игре"
    /// скрывает панель "О игре"
    /// </summary>
    public void OnCloseAboutButtonClicked()
    {
        // скрываем панель "О игре"
        if (aboutPanel != null)
            aboutPanel.SetActive(false);
    }

    /// <summary>
    /// вызывается из SettingsMenuController при клике на "Назад"
    /// возвращает игрока из настроек в главное меню
    /// </summary>
    public void OnBackFromSettings()
    {
        // скрываем меню настроек
        if (settingsMenuPanel != null)
            settingsMenuPanel.SetActive(false);
            
        // показываем главное меню
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
    }
}
