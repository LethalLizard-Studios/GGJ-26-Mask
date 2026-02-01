using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class PowerOutage : MonoBehaviour
{
    [SerializeField] private GameObject m_OutageParent;
    [SerializeField] private GameObject m_StageParent;
    [SerializeField] private BoxCollider m_FuseBoxCollider;
    [SerializeField] private SlidingDoor m_SlidingDoor;
    [SerializeField] private BoxCollider m_BoxColliderReturn;

    [Header("Lights")]
    [SerializeField] private Light[] m_Lights;
    [SerializeField] private GameObject[] m_LightSecondaryObject;
    [SerializeField] private float m_FlickerDuration = 3f;
    [SerializeField] private Vector2 m_FlickerInterval = new Vector2(0.05f, 0.2f);

    [Header("Audio")]
    [SerializeField] private AudioSource m_OutageAudioSource;
    [SerializeField] private AudioSource[] m_OutageFlickerAudioSource;
    [SerializeField] private AudioSource[] m_AudioSourcesToStop;
    [SerializeField] private AudioSource m_AttackAudio;

    [Header("Post Processing")]
    [SerializeField] private Volume m_OutageGlobalVolume;
    [SerializeField] private float m_VolumeFadeDuration = 1.5f;

    [Header("Fail Timer")]
    [SerializeField] private float m_KillIfNotRestoredTime = 30f;

    private bool m_IsPowerOut = false;
    private int m_CurrentFlickerIndex = 0;
    private Coroutine m_OutageRoutine;
    private Coroutine m_KillRoutine;
    private Tween m_VolumeTween;

    public void StartOutage()
    {
        if (m_IsPowerOut)
            return;

        if (m_OutageRoutine != null)
        {
            StopCoroutine(m_OutageRoutine);
            m_OutageRoutine = null;
        }

        if (m_KillRoutine != null)
        {
            StopCoroutine(m_KillRoutine);
            m_KillRoutine = null;
        }

        m_IsPowerOut = true;
        m_VolumeTween.Kill(false);
        m_OutageRoutine = StartCoroutine(OutageSequence());
        m_KillRoutine = StartCoroutine(KillIfNotRestoredSequence());
    }

    public bool IsPowerOut() { return m_IsPowerOut; }

    public void PowerRestored()
    {
        if (m_KillRoutine != null)
        {
            StopCoroutine(m_KillRoutine);
            m_KillRoutine = null;
        }

        if (m_AudioSourcesToStop != null)
        {
            for (int i = 0; i < m_AudioSourcesToStop.Length; i++)
            {
                AudioSource s = m_AudioSourcesToStop[i];
                if (s == null) continue;

                s.enabled = true;
            }
        }

        for (int i = 0; i < m_Lights.Length; i++)
        {
            if (m_Lights[i] == null) continue;

            m_Lights[i].enabled = true;
            if (m_LightSecondaryObject[i] != null)
                m_LightSecondaryObject[i].SetActive(true);
        }

        if (m_OutageGlobalVolume != null)
        {
            m_VolumeTween = DOTween.To(
                    () => m_OutageGlobalVolume.weight,
                    v => m_OutageGlobalVolume.weight = v,
                    0.0f,
                    m_VolumeFadeDuration)
                .SetEase(Ease.InOutSine);
        }

        m_BoxColliderReturn.enabled = true;
        m_SlidingDoor.Open();
        m_IsPowerOut = false;
        m_FuseBoxCollider.enabled = false;
        m_OutageParent.SetActive(false);
        m_StageParent.SetActive(true);
    }

    private IEnumerator OutageSequence()
    {
        if (m_OutageGlobalVolume != null)
        {
            m_OutageGlobalVolume.weight = 0f;
        }

        float endTime = Time.time + m_FlickerDuration;

        while (Time.time < endTime)
        {
            for (int i = 0; i < m_Lights.Length; i++)
            {
                if (m_Lights[i] == null) continue;

                bool rand = Random.value > 0.5f;

                if (i % 3 == 0)
                {
                    m_OutageFlickerAudioSource[m_CurrentFlickerIndex].pitch = Random.Range(0.9f, 1.1f);
                    m_OutageFlickerAudioSource[m_CurrentFlickerIndex].Play();
                    m_CurrentFlickerIndex++;

                    if (m_CurrentFlickerIndex >= m_OutageFlickerAudioSource.Length)
                        m_CurrentFlickerIndex = 0;
                }

                if (!rand)
                    m_OutageGlobalVolume.weight += 0.15f;
                else
                    m_OutageGlobalVolume.weight = 0.0f;

                m_Lights[i].enabled = rand;
                if (m_LightSecondaryObject[i] != null)
                    m_LightSecondaryObject[i].SetActive(rand);
            }

            float wait = Random.Range(m_FlickerInterval.x, m_FlickerInterval.y);
            yield return new WaitForSeconds(wait);
        }

        if (m_AudioSourcesToStop != null)
        {
            for (int i = 0; i < m_AudioSourcesToStop.Length; i++)
            {
                AudioSource s = m_AudioSourcesToStop[i];
                if (s == null) continue;

                s.enabled = false;
            }
        }

        if (m_OutageAudioSource != null)
        {
            m_OutageAudioSource.Play();
        }

        for (int i = 0; i < m_Lights.Length; i++)
        {
            if (m_Lights[i] == null) continue;

            m_Lights[i].enabled = false;
            if (m_LightSecondaryObject[i] != null)
                m_LightSecondaryObject[i].SetActive(false);
        }

        if (m_OutageGlobalVolume != null)
        {
            m_VolumeTween = DOTween.To(
                    () => m_OutageGlobalVolume.weight,
                    v => m_OutageGlobalVolume.weight = v,
                    1f,
                    m_VolumeFadeDuration)
                .SetEase(Ease.InOutSine);
        }

        m_StageParent.SetActive(false);

        m_SlidingDoor.Open();
        m_FuseBoxCollider.enabled = true;

        m_OutageParent.SetActive(true);

        m_OutageRoutine = null;
    }

    private IEnumerator KillIfNotRestoredSequence()
    {
        float endTime = Time.time + m_KillIfNotRestoredTime;

        while (Time.time < endTime)
        {
            if (!m_IsPowerOut)
            {
                m_KillRoutine = null;
                yield break;
            }

            yield return null;
        }

        if (m_IsPowerOut)
        {
            m_AttackAudio.Play();

            yield return new WaitForSeconds(1);

            KilledPlayer();
        }

        m_KillRoutine = null;
    }

    private void KilledPlayer()
    {
        Debug.Log("Killed Player");

        SceneManager.LoadScene(1);
    }
}
