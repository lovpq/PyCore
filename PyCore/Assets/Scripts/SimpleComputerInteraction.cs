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
        {
            playerTransform = player.transform;
        }

        pythonUI = FindFirstObjectByType<SimplePythonUI>();
    }

    private void Update()
    {
        if (playerTransform == null) return;
        if (pythonUI == null)
        {
            pythonUI = FindFirstObjectByType<SimplePythonUI>();
            return;
        }

        // Не обрабатываем взаимодействие, если панель уже открыта
        if (pythonUI.IsPanelOpen) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        canInteract = distance <= interactionDistance;

        if (!canInteract) return;

        bool ePressed = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
        
        if (ePressed)
        {
            pythonUI.OpenTaskSystem();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}
