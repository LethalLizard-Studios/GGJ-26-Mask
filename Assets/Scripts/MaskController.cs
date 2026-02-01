using UnityEngine;

public class MaskController : MonoBehaviour
{
    [SerializeField] private PlayerMovement m_PlayerMovement;
    [SerializeField] private GameObject m_MaskOverlay;
    [SerializeField] private MeshRenderer m_MeshOnWall;
    [SerializeField] private BoxCollider m_ColliderOnWall;

    [SerializeField] private InputSystem_Actions m_InputActions;

    [HideInInspector] public bool IsMaskOn = false;

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
        m_MaskOverlay.SetActive(true);
        m_MeshOnWall.enabled = false;
        m_ColliderOnWall.enabled=false;
        IsMaskOn = true;
    }

    public void TakeOffMask()
    {
        m_PlayerMovement.enabled = true;
        m_MaskOverlay.SetActive(false);
        m_MeshOnWall.enabled = true;
        m_ColliderOnWall.enabled = true;
        IsMaskOn = false;
    }

    private void Update()
    {
        if (IsMaskOn && m_InputActions.Player.ExitCamera.WasPressedThisFrame())
        {
            TakeOffMask();
        }
    }
}
