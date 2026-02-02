using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class MaskController : MonoBehaviour
{
    [SerializeField] private PlayerMovement m_PlayerMovement;
    [SerializeField] private GameObject m_MaskOverlay;
    [SerializeField] private MeshRenderer m_MeshOnWall;
    [SerializeField] private BoxCollider m_ColliderOnWall;
    [SerializeField] private Image m_MaskOverlayImage;

    [SerializeField] private InputSystem_Actions m_InputActions;

    [HideInInspector] public bool IsMaskOn = false;

    private Sequence m_MaskSequence;

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
    }

    public void WearMask()
    {
        m_PlayerMovement.enabled = false;
        m_MeshOnWall.enabled = false;
        m_ColliderOnWall.enabled = false;
        IsMaskOn = true;

        var rectTransform = (RectTransform)m_MaskOverlayImage.transform;
        rectTransform.localScale = Vector3.one * 0.7f;
        rectTransform.localEulerAngles = new Vector3(25f, 0f, 0f);
        rectTransform.localPosition = new Vector3(rectTransform.localPosition.x, -600f, rectTransform.localPosition.z);
        m_MaskOverlayImage.color = new Color(m_MaskOverlayImage.color.r, m_MaskOverlayImage.color.g, m_MaskOverlayImage.color.b, 0f);
        m_MaskOverlay.SetActive(true);

        m_MaskSequence = DOTween.Sequence();
        m_MaskSequence.Join(m_MaskOverlayImage.DOFade(0.9f, 0.5f));
        m_MaskSequence.Join(rectTransform.DOScale(1f, 0.5f));
        m_MaskSequence.Join(rectTransform.DORotate(new Vector3(-25f, 0f, 0f), 0.5f));
        m_MaskSequence.Join(rectTransform.DOLocalMoveY(-57.05f, 0.5f));
    }

    public void TakeOffMask()
    {
        var rectTransform = (RectTransform)m_MaskOverlayImage.transform;

        m_MaskSequence = DOTween.Sequence();
        m_MaskSequence.Join(m_MaskOverlayImage.DOFade(0f, 0.25f));
        m_MaskSequence.Join(rectTransform.DOScale(0.7f, 0.25f));
        m_MaskSequence.Join(rectTransform.DORotate(new Vector3(25f, 0f, 0f), 0.25f));
        m_MaskSequence.Join(rectTransform.DOLocalMoveY(-600f, 0.25f));
        m_MaskSequence.OnComplete(() =>
        {
            m_MaskOverlay.SetActive(false);
            m_PlayerMovement.enabled = true;
            m_MeshOnWall.enabled = true;
            m_ColliderOnWall.enabled = true;
            IsMaskOn = false;
        });
    }


    private void Update()
    {
        if (IsMaskOn && m_InputActions.Player.ExitCamera.WasPressedThisFrame())
        {
            TakeOffMask();
        }
    }
}