using System.Collections;
using TMPro;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    [Header("Night")]
    [Range(1, 10)]
    [SerializeField] private int m_Difficulty;
    [SerializeField] private int m_NightLength = 240;

    [Header("Events")]
    [SerializeField] private int m_PowerOutagePerRun = 1;
    [SerializeField] private Vector2 m_SpotlightMoveDelay = new Vector2(30, 78);
    [SerializeField] private Vector2 m_MaskAppreanceDelay = new Vector2(65, 130);
    [SerializeField] private float m_MaskKillDelay = 6;

    [Header("Interface")]
    [SerializeField] private TextMeshProUGUI m_TimerText;

    [Header("References")]
    [SerializeField] private PowerOutage m_PowerOutage;

    private Coroutine m_NightRoutine;
    private float m_NightTimeRemaining;
    private bool m_IsNightRunning;

    private void Start()
    {
        BeginNight(1);
    }

    public void BeginNight(int index)
    {
        if (m_NightRoutine != null)
        {
            StopCoroutine(m_NightRoutine);
            m_NightRoutine = null;
        }

        m_NightRoutine = StartCoroutine(NightSequence(index));
    }

    private IEnumerator NightSequence(int index)
    {
        m_IsNightRunning = true;

        m_NightTimeRemaining = m_NightLength;
        UpdateTimerText(m_NightTimeRemaining);

        float nightStartTime = Time.time;
        float nightEndTime = nightStartTime + m_NightLength;

        int outagesRemaining = Mathf.Max(0, m_PowerOutagePerRun);

        float safeZone = m_NightLength * 0.2f;
        float outageMinTime = nightStartTime + safeZone;
        float outageMaxTime = nightEndTime - safeZone;

        float nextOutageTime = -1f;
        if (outagesRemaining > 0 && outageMaxTime > outageMinTime)
        {
            nextOutageTime = Random.Range(outageMinTime, outageMaxTime);
        }

        float nextSpotlightMoveTime = nightStartTime + Random.Range(m_SpotlightMoveDelay.x, m_SpotlightMoveDelay.y);
        float nextMaskAppearanceTime = nightStartTime + Random.Range(m_MaskAppreanceDelay.x, m_MaskAppreanceDelay.y);

        while (Time.time < nightEndTime)
        {
            m_NightTimeRemaining = nightEndTime - Time.time;
            UpdateTimerText(m_NightTimeRemaining);

            if (outagesRemaining > 0 && nextOutageTime > 0f && Time.time >= nextOutageTime)
            {
                if (m_PowerOutage != null)
                {
                    m_PowerOutage.StartOutage();
                }

                outagesRemaining--;
                nextOutageTime = -1f;
            }

            if (Time.time >= nextSpotlightMoveTime)
            {
                RandomSpotlightMove();
                nextSpotlightMoveTime = Time.time + Random.Range(m_SpotlightMoveDelay.x, m_SpotlightMoveDelay.y);
            }

            if (Time.time >= nextMaskAppearanceTime)
            {
                RandomMaskAppreance();
                nextMaskAppearanceTime = Time.time + Random.Range(m_MaskAppreanceDelay.x, m_MaskAppreanceDelay.y);
            }

            yield return null;
        }

        m_NightTimeRemaining = 0f;
        UpdateTimerText(m_NightTimeRemaining);

        m_IsNightRunning = false;
        m_NightRoutine = null;
    }

    public void RandomSpotlightMove()
    {
        if (m_PowerOutage.IsPowerOut())
            return;

        Debug.Log("Moving Spotlight");
    }

    public void RandomMaskAppreance()
    {
        if (m_PowerOutage.IsPowerOut())
            return;

        Debug.Log("Mask Apperance Inbound");
    }

    private void UpdateTimerText(float timeRemainingSeconds)
    {
        if (m_TimerText == null) return;

        int totalSeconds = Mathf.CeilToInt(timeRemainingSeconds);
        if (totalSeconds < 0) totalSeconds = 0;

        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        m_TimerText.text = $"{minutes}:{seconds:00}";
    }
}