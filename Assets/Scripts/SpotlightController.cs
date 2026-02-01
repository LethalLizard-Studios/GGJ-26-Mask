using UnityEngine;

public class SpotlightController : MonoBehaviour
{
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private Transform[] spotlights;
    [SerializeField] private Transform[] spotlightLookAt;
    [SerializeField] private SpotlightMovement[] spotlightMovements;

    [SerializeField] private GameObject playerParent;
    [SerializeField] private GameObject spotlightParent;

    private bool isInSpotlight = false;
    private Transform currentLookAt;

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
    }

    public void TakeControl(int index)
    {
        int lightIndex = Mathf.Max(Mathf.Min(index, spotlights.Length - 1), 0);

        cameraPivot.position = spotlights[lightIndex].position;
        cameraPivot.GetChild(0).LookAt(spotlightLookAt[lightIndex], Vector3.up);
        currentLookAt = spotlightLookAt[lightIndex];
        spotlightMovements[lightIndex].TakeControl();

        spotlightParent.SetActive(true);
        playerParent.SetActive(false);

        isInSpotlight = true;
    }

    private void FixedUpdate()
    {
        if (currentLookAt != null)
        {
            cameraPivot.GetChild(0).LookAt(currentLookAt, Vector3.up);
        }
    }

    private void Update()
    {
        if (isInSpotlight && m_InputActions.Player.ExitCamera.WasPressedThisFrame())
        {
            LeaveControls();
        }
    }

    public void LeaveControls()
    {
        spotlightParent.SetActive(false);
        playerParent.SetActive(true);

        for (int i = 0; i < spotlightMovements.Length; i++)
        {
            spotlightMovements[i].IsInControl = false;
        }

        isInSpotlight = false;
    }
}
