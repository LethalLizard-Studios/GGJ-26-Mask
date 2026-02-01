using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteract : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private PropPickupPoint pickupPoint;
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private PlayerMovement movement;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI interactText;
    [SerializeField] private TextMeshProUGUI throwText;
    [SerializeField] private string promptMessage = "Press F to pick up";
    [SerializeField] private string promptSittingMessage = "Press E to sit";
    [SerializeField] private string throwPromptMessage = "Press LMB to throw";

    private Camera m_cam;
    private Transform m_currentTarget;
    private PropPickup m_currentPickup;
    private PropPickup m_subscribedPickup;
    private bool m_isSitting;
    private Vector3 m_preSeatPosition = Vector3.zero;

    private bool m_isShowingInteract;
    private Tween m_interactFadeTween;
    private Tween m_interactScaleTween;

    private bool m_isShowingThrow;
    private Tween m_throwFadeTween;
    private Tween m_throwScaleTween;

    [SerializeField] private InputSystem_Actions m_InputActions;

    private void Awake()
    {
        m_InputActions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        m_InputActions.Player.Enable();
    }

    private void OnDisable()
    {
        m_InputActions.Player.Disable();

        UnsubscribeFromPickup();
        HideInteractPrompt();
        HideThrowPrompt();
    }

    private void Start()
    {
        m_cam = GetComponent<Camera>();
        InitializeText(interactText);
        InitializeText(throwText);
    }

    private void InitializeText(TextMeshProUGUI text)
    {
        text.gameObject.SetActive(false);
        text.alpha = 0f;
        text.transform.localScale = Vector3.one;
    }

    private void Update()
    {
        HandleRaycast();
    }

    private void HandleRaycast()
    {
        Ray ray = new Ray(m_cam.transform.position, m_cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactableLayer))
        {
            if (hit.collider.CompareTag("Interactable"))
            {
                m_currentTarget = hit.transform;

                if (m_currentTarget.TryGetComponent<PropPickup>(out PropPickup pickup))
                {
                    m_currentPickup = pickup;
                    SubscribeToPickup(m_currentPickup);

                    ShowInteractPrompt((m_currentPickup.SitPoint != null)
                        ? promptSittingMessage + "\n" + promptMessage
                        : promptMessage);

                    if (m_InputActions.Player.Interact.WasPressedThisFrame())
                    {
                        m_currentPickup.Pickup(pickupPoint.transform);
                        HideInteractPrompt();
                    }
                    else if (m_InputActions.Player.Sit.WasPressedThisFrame() && m_currentPickup.SitPoint != null)
                    {
                        SitDown(m_currentPickup.SitPoint);
                    }

                    return;
                }

                if (m_currentTarget.TryGetComponent<Interactable>(out Interactable currentInteractable))
                {
                    if (!currentInteractable.CanBeInteractedWith())
                    {
                        HideInteractPrompt();
                        return;
                    }

                    ShowInteractPrompt(currentInteractable.promptMessage);
                    if (m_InputActions.Player.Interact.WasPressedThisFrame()) currentInteractable.Interact();

                    return;
                }
            }
        }

        HideInteractPrompt();
        m_currentTarget = null;
        m_currentPickup = null;
    }

    private void SubscribeToPickup(PropPickup pickup)
    {
        if (pickup == m_subscribedPickup) return;

        UnsubscribeFromPickup();
        m_subscribedPickup = pickup;

        if (m_subscribedPickup == null) return;

        m_subscribedPickup.HeldChanged += OnPickupHeldChanged;
        OnPickupHeldChanged(m_subscribedPickup.IsHeld);
    }

    private void UnsubscribeFromPickup()
    {
        if (m_subscribedPickup == null) return;

        m_subscribedPickup.HeldChanged -= OnPickupHeldChanged;
        m_subscribedPickup = null;
        HideThrowPrompt();
    }

    private void OnPickupHeldChanged(bool isHeld)
    {
        if (isHeld) ShowThrowPrompt(throwPromptMessage);
        else HideThrowPrompt();
    }

    private void ShowInteractPrompt(string message)
    {
        if (m_isShowingInteract && interactText.text == message) return;

        interactText.text = message;
        interactText.gameObject.SetActive(true);
        m_interactFadeTween?.Kill();
        m_interactScaleTween?.Kill();

        m_interactFadeTween = interactText.DOFade(1f, 0.25f);
        interactText.transform.localScale = Vector3.one * 1.15f;
        m_interactScaleTween = interactText.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack);
        m_isShowingInteract = true;
    }

    private void HideInteractPrompt()
    {
        if (!m_isShowingInteract) return;

        m_interactFadeTween?.Kill();
        m_interactScaleTween?.Kill();
        m_interactFadeTween = interactText.DOFade(0f, 0.2f)
            .OnComplete(() => interactText.gameObject.SetActive(false));
        m_interactScaleTween = interactText.transform.DOScale(0.95f, 0.2f);
        m_isShowingInteract = false;
    }

    private void ShowThrowPrompt(string message)
    {
        if (m_isShowingThrow && throwText.text == message) return;

        throwText.text = message;
        throwText.gameObject.SetActive(true);
        m_throwFadeTween?.Kill();
        m_throwScaleTween?.Kill();

        m_throwFadeTween = throwText.DOFade(1f, 0.25f);
        throwText.transform.localScale = Vector3.one * 1.15f;
        m_throwScaleTween = throwText.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack);
        m_isShowingThrow = true;
    }

    private void HideThrowPrompt()
    {
        if (!m_isShowingThrow) return;

        m_throwFadeTween?.Kill();
        m_throwScaleTween?.Kill();
        m_throwFadeTween = throwText.DOFade(0f, 0.2f)
            .OnComplete(() => throwText.gameObject.SetActive(false));
        m_throwScaleTween = throwText.transform.DOScale(0.95f, 0.2f);
        m_isShowingThrow = false;
    }

    private void SitDown(Transform sitTarget)
    {
        movement.canMove = m_isSitting;
        if (!m_isSitting)
        {
            m_preSeatPosition = movement.transform.position;
            movement.transform.DOMove(sitTarget.position, 0.5f)
                .SetEase(Ease.OutCubic)
                .OnComplete(() => m_isSitting = true);
        }
        else
        {
            movement.transform.DOMove(m_preSeatPosition, 0.5f)
                .SetEase(Ease.OutCubic)
                .OnComplete(() => m_isSitting = false);
        }
    }
}
