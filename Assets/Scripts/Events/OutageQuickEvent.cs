using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class OutageQuickEvent : MonoBehaviour
{
    [SerializeField] private GameObject[] m_KeyInterface;
    [SerializeField] private Image m_ProgressRing;
    [SerializeField] private BoxCollider m_BoxCollider;
    [SerializeField] private GameObject m_OutageCanvas;
    [SerializeField] private PowerOutage m_PowerOutage;

    [Header("Settings")]
    [SerializeField] private Vector2 m_RingTotalTime = new Vector2(1.0f, 4.0f);
    [SerializeField] private Vector2 m_NumberOfStrokes = new Vector2(3, 6);
    [SerializeField] private float m_PostResultHoldTime = 1f;

    [Header("Ring Colors")]
    [SerializeField] private Color m_RingIdleColor = Color.white;
    [SerializeField] private Color m_RingFailFlashColor = Color.red;
    [SerializeField] private Color m_RingSuccessColor = Color.green;

    private Tween m_RingTween;
    private Tween m_RingColorTween;
    private Coroutine m_Routine;

    private InputSystem_Actions m_InputActions;

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
        m_RingTween.Kill(false);
        m_RingColorTween.Kill(false);

        if (m_Routine != null)
        {
            StopCoroutine(m_Routine);
            m_Routine = null;
        }
    }

    public void OpenQuickEvent()
    {
        m_PlayerMovement.enabled = false;
        m_PlayerLook.canLook = false;
        m_PlayerCanvas.SetActive(false);
        m_OutageCanvas.SetActive(true);
        m_BoxCollider.enabled = false;

        if (m_Routine != null) StopCoroutine(m_Routine);
        m_Routine = StartCoroutine(QuickEventSequence());
    }

    private IEnumerator QuickEventSequence()
    {
        int strokeNumber = Mathf.RoundToInt(Random.Range(m_NumberOfStrokes.x, m_NumberOfStrokes.y));

        yield return new WaitForSeconds(3.0f);

        for (int i = 0; i < strokeNumber; i++)
        {
            int randomStroke = Random.Range(0, m_KeyInterface.Length);
            float ringTime = Random.Range(m_RingTotalTime.x, m_RingTotalTime.y);

            for (int j = 0; j < m_KeyInterface.Length; j++)
            {
                m_KeyInterface[j].SetActive(false);
            }

            m_KeyInterface[randomStroke].SetActive(true);

            StartRingFill(ringTime);

            bool pressedCorrect = false;
            float endTime = Time.time + ringTime;

            while (Time.time < endTime)
            {
                if (WasCorrectStrokePressedThisFrame(randomStroke))
                {
                    pressedCorrect = true;
                    break;
                }

                yield return null;
            }

            StopRingFill();

            if (pressedCorrect)
            {
                ShowSuccessResult();
            }
            else
            {
                yield return FlashFailResult();
            }

            m_KeyInterface[randomStroke].SetActive(false);

            yield return new WaitForSeconds(m_PostResultHoldTime);

            ResetRingVisuals();
        }

        CloseQuickEvent();
    }

    public void CloseQuickEvent()
    {
        m_PlayerMovement.enabled = true;
        m_PlayerLook.canLook = true;
        m_PlayerCanvas.SetActive(true);
        m_OutageCanvas.SetActive(false);

        m_PowerOutage.PowerRestored();
    }

    private bool WasCorrectStrokePressedThisFrame(int strokeIndex)
    {
        if (strokeIndex == 0) return m_InputActions.Player.A.WasPressedThisFrame();
        if (strokeIndex == 1) return m_InputActions.Player.B.WasPressedThisFrame();
        if (strokeIndex == 2) return m_InputActions.Player.C.WasPressedThisFrame();
        if (strokeIndex == 3) return m_InputActions.Player.D.WasPressedThisFrame();

        return false;
    }

    private void StartRingFill(float ringTime)
    {
        if (m_ProgressRing == null) return;

        m_RingTween.Kill(false);
        m_RingColorTween.Kill(false);

        m_ProgressRing.fillAmount = 0f;
        m_ProgressRing.color = m_RingIdleColor;

        m_RingTween = m_ProgressRing
            .DOFillAmount(1f, ringTime)
            .SetEase(Ease.Linear);
    }

    private void StopRingFill()
    {
        m_RingTween.Kill(false);
    }

    private void ResetRingVisuals()
    {
        if (m_ProgressRing == null) return;

        m_RingTween.Kill(false);
        m_RingColorTween.Kill(false);

        m_ProgressRing.fillAmount = 0f;
        m_ProgressRing.color = m_RingIdleColor;
    }

    private void ShowSuccessResult()
    {
        if (m_ProgressRing == null) return;

        m_RingColorTween.Kill(false);
        m_ProgressRing.fillAmount = 1f;
        m_RingColorTween = m_ProgressRing.DOColor(m_RingSuccessColor, 0.08f);
    }

    private IEnumerator FlashFailResult()
    {
        if (m_ProgressRing == null) yield break;

        m_RingColorTween.Kill(false);
        m_ProgressRing.fillAmount = 1f;

        m_ProgressRing.color = m_RingFailFlashColor;
        m_RingColorTween = m_ProgressRing.DOColor(m_RingIdleColor, 0.25f);

        yield return new WaitForSeconds(0.25f);
    }

    [Header("Player")]
    [SerializeField] private PlayerMovement m_PlayerMovement;
    [SerializeField] private PlayerLook m_PlayerLook;
    [SerializeField] private GameObject m_PlayerCanvas;
}