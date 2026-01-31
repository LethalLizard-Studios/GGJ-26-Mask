using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class OutageQuickEvent : MonoBehaviour
{
    [SerializeField] private KeyCode[] m_KeyPossibilities;
    [SerializeField] private GameObject[] m_KeyInterface;
    [SerializeField] private Image m_ProgressRing;
    [SerializeField] private BoxCollider m_BoxCollider;
    [SerializeField] private GameObject m_OutageCanvas;

    [Header("Settings")]
    [SerializeField] private Vector2 m_RingTotalTime = new Vector2(1.0f, 4.0f);
    [SerializeField] private Vector2 m_NumberOfStrokes = new Vector2(3, 6);

    [Header("Player")]
    [SerializeField] private PlayerMovement m_PlayerMovement;
    [SerializeField] private PlayerLook m_PlayerLook;
    [SerializeField] private GameObject m_PlayerCanvas;

    private Tween m_RingTween;

    public void OpenQuickEvent()
    {
        m_PlayerMovement.enabled = false;
        m_PlayerLook.canLook = false;
        m_PlayerCanvas.SetActive(false);
        m_OutageCanvas.SetActive(true);
        m_BoxCollider.enabled = false;

        StartCoroutine(QuickEventSequence());
    }

    private IEnumerator QuickEventSequence()
    {
        int strokeNumber = Mathf.RoundToInt(Random.Range(m_NumberOfStrokes.x, m_NumberOfStrokes.y));

        yield return new WaitForSeconds(3.0f);

        for (int i = 0; i < strokeNumber; i++)
        {
            int randomStroke = Random.Range(0, m_KeyPossibilities.Length);
            float ringTime = Random.Range(m_RingTotalTime.x, m_RingTotalTime.y);

            for (int j = 0; j < m_KeyInterface.Length; j++)
            {
                m_KeyInterface[j].SetActive(false);
            }
            m_KeyInterface[randomStroke].SetActive(true);
            StartRingFill(ringTime);

            yield return new WaitForSeconds(ringTime);

            StopRingFill();
            m_KeyInterface[randomStroke].SetActive(false);

            yield return new WaitForSeconds(1);
        }

        CloseQuickEvent();
    }

    public void CloseQuickEvent()
    {
        m_PlayerMovement.enabled = true;
        m_PlayerLook.canLook = true;
        m_PlayerCanvas.SetActive(true);
        m_OutageCanvas.SetActive(false);
    }

    private void StartRingFill(float ringTime)
    {
        if (m_ProgressRing == null) return;

        m_RingTween.Kill(false);
        m_ProgressRing.fillAmount = 0f;

        m_RingTween = m_ProgressRing
            .DOFillAmount(1f, ringTime)
            .SetEase(Ease.Linear);
    }

    private void StopRingFill()
    {
        m_RingTween.Kill(false);

        if (m_ProgressRing != null)
        {
            m_ProgressRing.fillAmount = 0f;
        }
    }
}
