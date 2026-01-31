using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RoomMusicTrigger : MonoBehaviour
{
    [SerializeField] private AudioSource m_RoomMusicSource;

    private void Reset()
    {
        Collider c = GetComponent<Collider>();
        c.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        RoomMusicManager.Instance.EnterRoom(this);
    }

    public AudioSource GetRoomSource()
    {
        return m_RoomMusicSource;
    }
}