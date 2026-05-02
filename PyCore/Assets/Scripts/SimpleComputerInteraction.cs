using UnityEngine;
using UnityEngine.InputSystem;

public class SimpleComputerInteraction : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float interactionDistance = 5f;

    private Transform playerTransform;
    private SimplePythonUI pythonUI;
    private bool canInteract = false;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;

        pythonUI = FindFirstObjectByType<SimplePythonUI>();
    }

    private void Update()
    {
        if (playerTransform == null) return;

        // Откладываем поиск SimplePythonUI только один раз
        if (pythonUI == null)
        {
            pythonUI = FindFirstObjectByType<SimplePythonUI>();
            return;
        }

        // Не открываем задачи, если уже открыта любая UI-панель
        if (pythonUI.IsPanelOpen) return;
        if (Core.NeedsManager.Instance != null && Core.NeedsManager.Instance.IsAnyPanelOpen()) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        canInteract = distance <= interactionDistance;

        if (!canInteract) return;

        bool ePressed = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
        if (ePressed)
            pythonUI.OpenTaskSystem();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}
