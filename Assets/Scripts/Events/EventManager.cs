using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    [SerializeField] private SpotlightMovement[] m_SpotlightMovement;
    [SerializeField] private MoveToPosition m_StageRunner;
    [SerializeField] private GameObject m_MaskOnStage;
    [SerializeField] private AudioSource m_AttackedAudio;
    [SerializeField] private GameObject m_MaskAtOfficeDoor;
    [SerializeField] private MaskController m_MaskController;
    [SerializeField] private Transform m_Player;
    [SerializeField] private Transform m_OfficeLocation;
    [SerializeField] private Transform m_Catwalk;
    [SerializeField] private Transform m_MainStageLocation;
    [SerializeField] private SlidingDoor m_SlidingDoor;

    private Coroutine m_NightRoutine;
    private float m_NightTimeRemaining;
    private bool m_IsNightRunning;
    private bool m_MaskIsActive = false;

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
        NightOver();
        UpdateTimerText(m_NightTimeRemaining);

        m_IsNightRunning = false;
        m_NightRoutine = null;
    }

    public void RandomSpotlightMove()
    {
        if (m_PowerOutage.IsPowerOut())
            return;

        if (m_SpotlightMovement == null || m_SpotlightMovement.Length == 0)
            return;

        int startIndex = Random.Range(0, m_SpotlightMovement.Length);
        bool movedOne = false;

        for (int i = 0; i < m_SpotlightMovement.Length; i++)
        {
            int index = startIndex + i;
            if (index >= m_SpotlightMovement.Length) index -= m_SpotlightMovement.Length;

            SpotlightMovement spotlight = m_SpotlightMovement[index];
            if (spotlight == null) continue;

            if (!spotlight.IsDisabled())
            {
                spotlight.MoveOffStage();
                movedOne = true;
                break;
            }
        }

        if (!movedOne)
        {
            SpotlightBlackout();
            return;
        }

        if (AreAllSpotlightsDisabled())
        {
            SpotlightBlackout();
        }
    }

    public void SpotlightBlackout()
    {
        Debug.Log("Spotlight Blackout");
        m_SlidingDoor.Open();
        m_StageRunner.BeginMove();
    }

    public void RandomMaskAppreance()
    {
        if (m_PowerOutage.IsPowerOut() && !m_MaskIsActive)
            return;

        StartCoroutine(MaskAppearanceSequence());

        Debug.Log("Mask Apperance Inbound");
    }

    private IEnumerator MaskAppearanceSequence()
    {
        if (m_MaskIsActive) yield break;

        m_MaskIsActive = true;

        if (m_MaskOnStage != null) m_MaskOnStage.SetActive(false);
        if (m_MaskAtOfficeDoor != null) m_MaskAtOfficeDoor.SetActive(false);

        yield return new WaitForSeconds(Random.Range(6f, 20f));

        Transform closest = GetClosestMaskLocation();
        if (closest == null)
        {
            if (m_MaskOnStage != null) m_MaskOnStage.SetActive(true);
            m_MaskIsActive = false;
            yield break;
        }

        if (closest == m_MainStageLocation)
        {
            if (m_AttackedAudio != null) m_AttackedAudio.Play();
            yield return new WaitForSeconds(1f);
            KilledPlayer();
            yield break;
        }

        GameObject targetMaskObject = null;

        if (closest == m_OfficeLocation || closest == m_Catwalk) targetMaskObject = m_MaskAtOfficeDoor;

        if (targetMaskObject != null) targetMaskObject.SetActive(true);
        if (m_AttackedAudio != null) m_AttackedAudio.Play();

        m_SlidingDoor.Open();

        float endTime = Time.time + m_MaskKillDelay;
        while (Time.time < endTime)
        {
            if (m_MaskController != null && m_MaskController.IsMaskOn)
            {
                if (targetMaskObject != null) targetMaskObject.SetActive(false);
                if (m_MaskOnStage != null) m_MaskOnStage.SetActive(true);
                if (m_AttackedAudio != null) m_AttackedAudio.Stop();
                m_MaskIsActive = false;
                m_SlidingDoor.Close();
                yield break;
            }

            yield return null;
        }

        KilledPlayer();
    }

    private Transform GetClosestMaskLocation()
    {
        if (m_Player == null) return null;

        Transform closest = null;
        float bestSqr = float.MaxValue;

        Transform[] locations = new Transform[] { m_OfficeLocation, m_Catwalk, m_MainStageLocation };

        for (int i = 0; i < locations.Length; i++)
        {
            Transform t = locations[i];
            if (t == null) continue;

            float sqr = (t.position - m_Player.position).sqrMagnitude;
            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                closest = t;
            }
        }

        return closest;
    }


    public void CheckSpotlights()
    {
        if (AreAllSpotlightsDisabled())
        {
            SpotlightBlackout();
        }
    }

    private bool AreAllSpotlightsDisabled()
    {
        for (int i = 0; i < m_SpotlightMovement.Length; i++)
        {
            SpotlightMovement spotlight = m_SpotlightMovement[i];
            if (spotlight == null) continue;

            if (!spotlight.IsDisabled())
                return false;
        }

        return true;
    }

    private void UpdateTimerText(float timeRemainingSeconds)
    {
        if (m_TimerText == null) return;

        int totalSeconds = Mathf.CeilToInt(timeRemainingSeconds);

        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        m_TimerText.text = $"{minutes}:{seconds:00}";
    }

    private void KilledPlayer()
    {
        Debug.Log("Killed Player");
        SceneManager.LoadScene(1);
    }

    private void NightOver()
    {
        Debug.Log("Night Over");
        SceneManager.LoadScene(3);
    }
}
