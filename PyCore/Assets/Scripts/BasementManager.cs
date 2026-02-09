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

    private PlayerData playerData;
    private float updateTimer = 0f;

    void Start()
    {
        playerData = GameManager.Instance != null
            ? GameManager.Instance.playerData : new PlayerData();
        UpdateRoomObjects();
    }

    void Update()
    {
        // Обновляем объекты раз в секунду
        updateTimer += Time.deltaTime;
        if (updateTimer >= 1f)
        {
            updateTimer = 0f;
            UpdateRoomObjects();
        }
    }

    void UpdateRoomObjects()
    {
        if (playerData == null) return;
        if (computer != null) computer.SetActive(playerData.hasComputer);
        if (desk != null) desk.SetActive(playerData.hasDesk);
        if (bed != null) bed.SetActive(playerData.hasBed);
        if (lightBulb != null) lightBulb.SetActive(playerData.hasLight);
    }
}
