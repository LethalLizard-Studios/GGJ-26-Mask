using DG.Tweening;
using TMPro;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactDistance = 3f;
    public LayerMask interactableLayer;
    public KeyCode interactKey = KeyCode.F;
    public KeyCode sitKey = KeyCode.E;
    public PlayerMovement movement;

    [Header("UI")]
    public TextMeshProUGUI interactText;
    public string promptMessage = "Press F to pick up";
    public string promptSittingMessage = "Press E to sit";

    private Camera cam;
    private Transform currentTarget;
    private PropPickup currentPickup;
    private bool isSitting = false;
    private Vector3 preSeatPosition = Vector3.zero;
    private bool isShowing = false;
    private Tween fadeTween;
    private Tween scaleTween;

    void Start()
    {
        cam = GetComponent<Camera>();
        interactText.gameObject.SetActive(false);
        interactText.alpha = 0f;
        interactText.transform.localScale = Vector3.one;
    }

    void Update()
    {
        HandleRaycast();
    }

    void HandleRaycast()
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactableLayer))
        {
            if (hit.collider.CompareTag("Interactable"))
            {
                currentTarget = hit.transform;
                if (currentTarget.TryGetComponent<PropPickup>(out currentPickup))
                {
                    ShowPrompt((currentPickup.sitPoint != null)
                        ? promptSittingMessage + "\n" + promptMessage
                        : promptMessage);

                    if (Input.GetKeyDown(interactKey))
                    {
                        currentPickup.Pickup();
                        HidePrompt();
                    }
                    else if (Input.GetKeyDown(sitKey) && currentPickup.sitPoint != null)
                    {
                        SitDown(currentPickup.sitPoint);
                    }
                }
                else if (currentTarget.TryGetComponent<Interactable>(out Interactable currentInteractable))
                {
                    if (!currentInteractable.CanBeInteractedWith())
                    {
                        HidePrompt();
                        return;
                    }

                    ShowPrompt(currentInteractable.promptMessage);
                    if (Input.GetKeyDown(interactKey)) currentInteractable.Interact();
                }
                return;
            }
        }

        HidePrompt();
        currentTarget = null;
        currentPickup = null;
    }

    void ShowPrompt(string message)
    {
        if (isShowing && interactText.text == message) return;

        interactText.text = message;
        interactText.gameObject.SetActive(true);
        fadeTween?.Kill();
        scaleTween?.Kill();

        fadeTween = interactText.DOFade(1f, 0.25f);
        interactText.transform.localScale = Vector3.one * 1.15f;
        scaleTween = interactText.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack);
        isShowing = true;
    }

    void HidePrompt()
    {
        if (!isShowing) return;

        fadeTween?.Kill();
        scaleTween?.Kill();
        fadeTween = interactText.DOFade(0f, 0.2f)
            .OnComplete(() => interactText.gameObject.SetActive(false));
        scaleTween = interactText.transform.DOScale(0.95f, 0.2f);
        isShowing = false;
    }

    void SitDown(Transform sitTarget)
    {
        movement.canMove = isSitting;
        if (!isSitting)
        {
            preSeatPosition = movement.transform.position;
            movement.transform.DOMove(sitTarget.position, 0.5f)
                .SetEase(Ease.OutCubic)
                .OnComplete(() => isSitting = true);
        }
        else
        {
            movement.transform.DOMove(preSeatPosition, 0.5f)
                .SetEase(Ease.OutCubic)
                .OnComplete(() => isSitting = false);
        }
    }
}
