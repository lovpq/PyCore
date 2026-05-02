using UnityEngine;
using UnityEngine.InputSystem;

namespace Core
{
    public class PlayerInteraction : MonoBehaviour
    {
        [Header("Interaction Settings")]
        [SerializeField] private float raycastDistance = 3f;
        [SerializeField] private LayerMask interactableLayer = -1;

        private Camera playerCamera;
        private Interactable currentInteractable;
        private InputAction interactAction;

        private void Awake()
        {
            playerCamera = GetComponentInChildren<Camera>();
        }

        private void Start()
        {
            var playerInput = GetComponent<UnityEngine.InputSystem.PlayerInput>();
            if (playerInput != null)
            {
                interactAction = playerInput.actions["Interact"];
            }
            else
            {
                Debug.LogWarning("PlayerInteraction: PlayerInput не найден. Используется fallback биндинг <Keyboard>/e. Добавьте PlayerInput и настройте Input Actions.");
                interactAction = new InputAction("Interact", binding: "<Keyboard>/e");
                interactAction.Enable();
            }
        }

        private void OnDestroy()
        {
            // Освобождаем InputAction только если создавали его сами (без PlayerInput)
            if (interactAction != null && GetComponent<UnityEngine.InputSystem.PlayerInput>() == null)
            {
                interactAction.Disable();
                interactAction.Dispose();
            }
        }

        private void Update()
        {
            CheckForInteractable();

            if (interactAction != null && interactAction.WasPressedThisFrame())
            {
                if (currentInteractable != null && currentInteractable.IsPlayerNearby)
                {
                    Debug.Log($"PlayerInteraction: Interacting with {currentInteractable.gameObject.name}");
                    currentInteractable.Interact();
                    InteractionUIManager.Instance?.SetKeyPressed(true);
                }
            }

            if (interactAction != null && interactAction.WasReleasedThisFrame())
            {
                InteractionUIManager.Instance?.SetKeyPressed(false);
            }
        }

        private void CheckForInteractable()
        {
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, raycastDistance, interactableLayer))
            {
                Interactable interactable = hit.collider.GetComponent<Interactable>();
                if (interactable != null)
                {
                    if (currentInteractable != interactable)
                    {
                        currentInteractable = interactable;
                        InteractionUIManager.Instance?.SetInteractionPrompt(interactable);
                    }
                    return;
                }
            }

            if (currentInteractable != null)
            {
                currentInteractable = null;
                InteractionUIManager.Instance?.SetInteractionPrompt(null);
            }
        }

        private void OnDrawGizmos()
        {
            if (playerCamera != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * raycastDistance);
            }
        }
    }
}
