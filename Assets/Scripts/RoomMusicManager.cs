using UnityEngine;
using DG.Tweening;

public class RoomMusicManager : MonoBehaviour
{
    public static RoomMusicManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private float m_FadeDuration = 1.5f;
    [SerializeField] private float m_TargetVolume = 1f;

    private RoomMusicTrigger m_CurrentRoom;
    private Tween m_FadeOutTween;
    private Tween m_FadeInTween;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void EnterRoom(RoomMusicTrigger room)
    {
        if (room == null) return;
        if (m_CurrentRoom == room) return;

        AudioSource newSource = room.GetRoomSource();
        if (newSource == null) return;

        AudioSource oldSource = m_CurrentRoom != null ? m_CurrentRoom.GetRoomSource() : null;

        if (oldSource != null)
        {
            m_FadeOutTween.Kill(false);
            m_FadeOutTween = oldSource.DOFade(0f, m_FadeDuration).OnComplete(() =>
            {
                oldSource.Stop();
            });
        }

        m_FadeInTween.Kill(false);

        if (!newSource.isPlaying)
        {
            newSource.volume = 0f;
            newSource.Play();
        }

        m_FadeInTween = newSource.DOFade(m_TargetVolume, m_FadeDuration);

        m_CurrentRoom = room;
    }
}
