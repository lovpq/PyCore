using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Управляет объектами в комнате (показывает/скрывает купленные предметы).
/// UI статистики теперь в NeedsManager.
///
/// В Unity:
/// 1. Повесить на любой GameObject в сцене
/// 2. Назначить объекты комнаты (computer, desk, bed, lightBulb)
/// </summary>
public class BasementManager : MonoBehaviour
{
    [Header("Room Objects")]
    public GameObject computer;
    public GameObject desk;
    public GameObject bed;
    public GameObject lightBulb;

    private void Start()
    {
        Refresh();
    }

    /// <summary>Обновляет видимость объектов комнаты. Вызывайте после загрузки сохранения или покупки.</summary>
    public void Refresh()
    {
        UpdateRoomObjects();
    }

    private void UpdateRoomObjects()
    {
        // Получаем актуальные данные напрямую, чтобы не держать устаревшую ссылку
        PlayerData data = GameManager.Instance != null ? GameManager.Instance.playerData : null;
        if (data == null) return;
        if (computer != null) computer.SetActive(data.hasComputer);
        if (desk != null) desk.SetActive(data.hasDesk);
        if (bed != null) bed.SetActive(data.hasBed);
        if (lightBulb != null) lightBulb.SetActive(data.hasLight);
    }
}
