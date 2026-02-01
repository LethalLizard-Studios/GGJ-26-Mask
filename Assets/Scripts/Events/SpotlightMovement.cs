using UnityEngine;
using UnityEngine.UI;

public class SpotlightMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MeshRenderer m_MeshRenderer;
    [SerializeField] private MeshRenderer m_MeshRendererStatus;
    [SerializeField] private Light m_Light;
    [SerializeField] private AudioSource m_CompleteSound;

    [Header("Visuals")]
    [SerializeField] private Material m_OnMaterial;
    [SerializeField] private Material m_RedMaterial;
    [SerializeField] private Material m_OnEmmisiveMaterial;
    [SerializeField] private Material m_RedEmissiveMaterial;
    [SerializeField] private Color m_OnColor = Color.white;
    [SerializeField] private Color m_RedColor = Color.red;
    [SerializeField] private Image m_Correct;
    [SerializeField] private Image m_Wrong;

    [Header("Rotation Limits")]
    [SerializeField] private Vector2 m_MinEulerRotation;
    [SerializeField] private Vector2 m_MaxEulerRotation;

    [Header("Correct Rotation")]
    [SerializeField] private float m_CorrectX;
    [SerializeField] private float m_CorrectY;
    [SerializeField] private float m_ValidAngleTolerance = 8f;

    [Header("Input")]
    [SerializeField] private float m_RotationSpeed = 90f;

    [HideInInspector] public bool IsInControl = false;

    [SerializeField] private bool m_IsValid = false;

    [SerializeField] private InputSystem_Actions m_InputActions;
    [SerializeField] private EventManager m_EventManager;

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

    public void TakeControl()
    {
        IsInControl = true;
        ValidateLightPosition();
    }

    public bool IsDisabled() { return !m_IsValid; }

    public void MoveOffStage()
    {
        Debug.Log("Moving spotlight: "+IsInControl.ToString()+", "+m_IsValid.ToString());

        if (IsInControl) return;
        if (!m_IsValid) return;

        transform.localRotation = Quaternion.Euler(m_MaxEulerRotation.x, m_MaxEulerRotation.y, 0f);
        ValidateLightPosition();
    }

    private void Update()
    {
        if (!IsInControl) return;

        Vector2 move = m_InputActions.Player.Move.ReadValue<Vector2>();
        float h = move.x;
        float v = move.y;

        if (Mathf.Approximately(h, 0f) && Mathf.Approximately(v, 0f)) return;

        Vector3 euler = transform.localEulerAngles;

        float x = euler.x + (-v * m_RotationSpeed * Time.deltaTime);
        float y = euler.y + (h * m_RotationSpeed * Time.deltaTime);

        transform.localRotation = Quaternion.Euler(x, y, 0f);

        ClampRotation();
        ValidateLightPosition();
    }

    public void ValidateLightPosition()
    {
        if (m_Light == null || m_MeshRenderer == null) return;

        bool oldIsValid = m_IsValid;

        Vector3 euler = transform.localEulerAngles;

        float xDiff = Mathf.Abs(Mathf.DeltaAngle(euler.x, m_CorrectX));
        float yDiff = Mathf.Abs(Mathf.DeltaAngle(euler.y, m_CorrectY));

        m_IsValid = xDiff <= m_ValidAngleTolerance && yDiff <= m_ValidAngleTolerance;

        m_Correct.enabled = m_IsValid;
        m_Wrong.enabled = !m_IsValid;

        m_EventManager.CheckSpotlights();

        if (m_IsValid)
        {
            if (!oldIsValid && IsInControl)
                m_CompleteSound.Play();

            ApplyOnVisuals();
        }
        else
        {
            ApplyRedVisuals();
        }
    }

    public void DisableLight()
    {
        if (m_Light == null || m_MeshRenderer == null) return;
        ApplyRedVisuals();
    }

    public void ClampRotation()
    {
        Vector3 euler = transform.localEulerAngles;

        float x = ClampAngle(euler.x, m_MinEulerRotation.x, m_MaxEulerRotation.x);
        float y = ClampAngle(euler.y, m_MinEulerRotation.y, m_MaxEulerRotation.y);

        transform.localRotation = Quaternion.Euler(x, y, 0f);
    }

    private void ApplyOnVisuals()
    {
        m_Light.color = m_OnColor;
        if (m_OnMaterial != null)
        {
            m_MeshRenderer.material = m_OnMaterial;
            m_MeshRendererStatus.material = m_OnEmmisiveMaterial;
        }
    }

    private void ApplyRedVisuals()
    {
        m_Light.color = m_RedColor;
        if (m_RedMaterial != null)
        {
            m_MeshRenderer.material = m_RedMaterial;
            m_MeshRendererStatus.material = m_RedEmissiveMaterial;
        }
    }

    private float ClampAngle(float angle, float min, float max)
    {
        float a = NormalizeAngle180(angle);
        float mn = NormalizeAngle180(min);
        float mx = NormalizeAngle180(max);

        if (mn <= mx)
        {
            return Mathf.Clamp(a, mn, mx);
        }

        bool inUpper = a >= mn;
        bool inLower = a <= mx;

        if (inUpper || inLower)
        {
            return a;
        }

        float distToMin = Mathf.Abs(Mathf.DeltaAngle(a, mn));
        float distToMax = Mathf.Abs(Mathf.DeltaAngle(a, mx));

        return distToMin <= distToMax ? mn : mx;
    }

    private float NormalizeAngle180(float angle)
    {
        float a = Mathf.Repeat(angle + 180f, 360f) - 180f;
        return a;
    }
}
